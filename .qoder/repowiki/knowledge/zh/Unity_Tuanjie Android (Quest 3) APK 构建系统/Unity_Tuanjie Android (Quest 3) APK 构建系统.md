---
kind: build_system
name: Unity/Tuanjie Android (Quest 3) APK 构建系统
category: build_system
scope:
    - '**'
source_files:
    - Assets/Editor/BuildAPK.cs
    - Assets/Editor/OptimizeProject.cs
    - Assets/Editor/SceneSetup.cs
    - Assets/Editor/SetAndroidPaths.cs
    - Assets/Editor/SetPrefs.cs
    - ProjectSettings/EditorBuildSettings.asset
    - ProjectSettings/ProjectVersion.txt
    - ProjectSettings/QualitySettings.asset
    - ProjectSettings/GraphicsSettings.asset
    - Assets/XR/Loaders/OpenXRLoader.asset
    - Assets/XR/Loaders/OculusLoader.asset
    - Assets/XR/Loaders/ARCoreLoader.asset
    - Assets/XR/Loaders/MockHMDLoader.asset
    - Assets/XR/Loaders/SimulationLoader.asset
---

## 1. 使用的系统与工具

本项目是一个 Unity / Tuanjie 工程，构建完全基于 **Unity Editor 扩展（Editor Scripts）**，没有 Makefile、Dockerfile、CI 流水线或外部构建脚本。目标平台为 **Android（Meta Quest 3）**，通过 `BuildPipeline.BuildPlayer` 调用 Gradle 生成 APK。

- 编辑器版本：`ProjectVersion.txt` 声明 `2022.3.62f3c1`；构建代码注释中说明实际迁移到了 `Tuanjie 2022.3.62t12`。
- 构建后端：IL2CPP + ARM64，Gradle 作为 Android 构建系统。
- NDK：固定使用 `ndk/23.1.7779620`（R23B）。
- SDK：硬编码到 `/Users/a1-6/Library/Android/sdk`（开发者本机路径）。
- JDK：优先从运行中的编辑器二进制旁 `PlaybackEngines/AndroidPlayer/OpenJDK` 自动解析，回退到旧的 Unity Hub 路径。
- XR 配置：`Assets/XR/` 下包含 OpenXR、Oculus、ARCore、Mock HMD、Simulation 等多套 Loader/Settings，用于在编辑器内切换设备。

## 2. 核心文件与入口

| 文件 | 作用 |
|---|---|
| `Assets/Editor/BuildAPK.cs` | 主构建入口。提供菜单项 `Debug > Build APK`（输出稳定名 `InkRidge.apk`）和 `Debug > Build APK (dated)`（按 `yyyyMMdd-HHmm` 时间戳命名归档）。集中设置 Android 工具链、场景列表、输出目录 `Builds/`。 |
| `Assets/Editor/OptimizeProject.cs` | 一键应用 Quest 3 构建/渲染/音频优化配置：质量等级、IL2CPP Release、strip engine code、always-included shaders、Vorbis 压缩 ambience 流式加载等。还提供 `Debug/Quest 3: Enable Single-Pass Instanced` 开关，并在启用前扫描所有 Gongbi 着色器是否声明了 `UNITY_VERTEX_INPUT_INSTANCE_ID` / `UNITY_VERTEX_OUTPUT_STEREO` 宏。 |
| `Assets/Editor/SceneSetup.cs` | 自动化场景初始化：写入 Player Settings、将 6 个场景加入 Build Settings、创建 Start/Bamboo/Waterfall/Pavilion/Summit/End 场景并注入 GameManager、SceneTransition、XROrigin、LocomotionMediator、MeditationPoint 等组件。 |
| `Assets/Editor/SetAndroidPaths.cs` / `SetPrefs.cs` | 辅助工具，把 Android SDK/JDK/NDK 路径写入 `EditorPrefs`，供其他 Editor 脚本读取。 |
| `ProjectSettings/EditorBuildSettings.asset` | 持久化的 Build Scenes 列表（6 个 unity 场景），与 `BuildAPK.Scenes` 数组保持同步。 |
| `ProjectSettings/QualitySettings.asset` | 质量等级由 `OptimizeProject` 动态写入默认值（Android 平台默认 quality level）。 |
| `ProjectSettings/GraphicsSettings.asset` | `OptimizeProject` 运行时向 `m_AlwaysIncludedShaders` 追加自定义着色器，防止 IL2CPP strip 掉仅通过字符串 `Shader.Find` 引用的 shader。 |
| `Assets/XR/Loaders/*.asset` | OpenXR、Oculus、ARCore、Mock HMD、Simulation 多 loader，配合 `XRGeneralSettingsPerBuildTarget.asset` 选择。 |
| `Assembly-CSharp.csproj` / `InkRidge.Editor.csproj` 等 | VS/Solution 项目文件，由 Unity 自动生成，不参与构建逻辑。 |

## 3. 架构与约定

### 构建流程
1. 开发者在 Unity Editor 中选择 `Debug > Optimize Project for Quest 3` 应用性能/渲染/音频配置。
2. 选择 `Debug > Build APK` 或 `Build APK (dated)`，`BuildAPK.Run()` 会：
   - 调用 `ConfigureAndroidToolchain()` 设置 JDK/Sdk/Ndk 路径、切换 Android 目标、启用 Gradle、关闭 development build。
   - 强制设置 `ARM64`、`minSdk=29`、`targetSdk=34`、`IL2CPP Release`、`stripEngineCode=true`、`blitType=Never`、`optimizedFramePacing=true`。
   - 调用 `BuildPipeline.BuildPlayer(Scenes, apkPath, BuildTarget.Android, BuildOptions.None)`。
   - 输出到仓库根 `Builds/` 目录。
3. 带日期的变体使用 `PlayerSettings.bundleVersion`（由 `OptimizeProject` 设为 `1.0.0`）+ 时间戳命名，避免覆盖。

### 场景清单约定
- 场景顺序是**单一事实来源**：`BuildAPK.Scenes` 数组必须与 `SceneSetup.EnsureScenesInBuildSettings()` 以及 `GameManager` 的索引保持一致。
- 场景以数字前缀排序：`00_Start` → `01_Bamboo` → `02_Waterfall` → `03_Pavilion` → `04_Summit` → `99_End`。
- `EditorBuildSettings.asset` 中的 scenes 列表由 `SceneSetup` 每次运行时重写。

### 着色器保护机制
由于大部分场景是运行时生成的（`SceneBuilder.MakeMat()` 通过 `Shader.Find("Gongbi/Toon")` 获取材质），Unity player build 的 strip 阶段无法识别这些引用。`OptimizeProject.ConfigureGraphics()` 会把 `RequiredShaders` 列表（`Gongbi/Toon`、`Gongbi/InkSkybox`、`Gongbi/BreathCircle`、`Gongbi/BreathGlow`、`Gongbi/InkFog`、`Hidden/Vignette`）写入 GraphicsSettings 的 always-included 列表，确保不被 strip。

### 单通道实例化（Single-Pass Instanced）安全门
`EnableSinglePassInstanced` 会在打开该选项前扫描所有 Gongbi 着色器源码，检查是否包含 `UNITY_VERTEX_INPUT_INSTANCE_ID` 或 `UNITY_VERTEX_OUTPUT_STEREO`；若缺失则拒绝启用并列出具体文件名，避免静默渲染出错误的立体画面。

### 音频优化约定
`OptimizeProject.OptimizeAudio()` 扫描 `Assets/Audio` 下所有 `AudioClip`，根据路径是否包含 `ambience` 区分：环境音使用 Vorbis 压缩 + Streaming，一次性音效使用 DecompressOnLoad + preloadAudioData=true。

## 4. 约定与约束

- **无 CI/CD**：仓库中没有 GitHub Actions、GitLab CI、Jenkins 等配置文件，构建仅在本地 Unity Editor 中触发。
- **无 Makefile/Dockerfile**：不存在跨平台编译或容器化构建。
- **路径硬编码**：Android SDK、NDK、JDK 路径直接写死在 `BuildAPK.cs` 和 `SetAndroidPaths.cs` 中（如 `/Users/a1-6/Library/Android/sdk`），移植到其他机器需要修改。
- **bundleVersion 来源**：版本号 `1.0.0` 由 `OptimizeProject.ConfigurePlayer()` 在运行时写入 `PlayerSettings.bundleVersion`，而非来自 git tag 或环境变量。
- **场景顺序强一致**：`BuildAPK.Scenes`、`EditorBuildSettings.scenes`、`GameManager` 索引三者必须同步，否则场景跳转会错位。
- **XR 多后端共存**：`Assets/XR/Loaders/` 同时保留 OpenXR、Oculus、ARCore、Mock HMD、Simulation loader，通过 `XRPackageSettings` 和 `XRGeneralSettingsPerBuildTarget` 切换，但 Quest 3 发布时依赖 OpenXR/Oculus。
- **版本锁定**：NDK 固定为 `23.1.7779620`，Android API Level 固定为 `29→34`，IL2CPP 编译器配置为 Release。
- **构建产物位置**：所有 APK 统一输出到仓库根 `Builds/` 目录，稳定名 `InkRidge.apk` 会被覆盖，归档名带日期和时间戳。