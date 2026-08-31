---
kind: dependency_management
name: Unity 包管理与 XR 插件依赖（无外部语言依赖）
category: dependency_management
scope:
    - '**'
source_files:
    - ProjectSettings/PackageManagerSettings.asset
    - ProjectSettings/ProjectVersion.txt
    - Assets/XR/Loaders/OpenXRLoader.asset
    - Assets/XR/Loaders/OculusLoader.asset
    - Assets/XR/Loaders/ARCoreLoader.asset
    - Assets/XR/Loaders/SimulationLoader.asset
    - Assets/XRI/Settings/XRInteractionEditorSettings.asset
    - Assets/Resources/BillingMode.json
    - Assets/Scripts/InkRidge.asmdef
    - Assets/Editor/InkRidge.Editor.asmdef
---

## 1. 使用的系统/方法

本项目是一个 Unity/Tuanjie VR 游戏工程，**不使用任何外部语言级包管理器**（仓库中不存在 `package.json`、`go.mod`、`pom.xml`、`requirements.txt`、`Cargo.toml`、`Gemfile` 等）。所有第三方依赖通过 **Unity Package Manager (UPM)** 以 Unity 包的形式管理。

- UPM 默认源被替换为国内镜像：`ProjectSettings/PackageManagerSettings.asset` 将 `m_Url` 设置为 `https://packages.unity.cn`，并标记为默认注册表（`m_IsDefault: 1`），用于加速 Unity 官方包的下载与安装。
- 未启用预发布包：`m_EnablePreReleasePackages: 0`。
- 项目根目录的 `Packages/` 文件夹为空，说明没有使用 Git URL / Local / Tarball 形式的 UPM 包；也没有 `Packages/manifest.json` 或 `Packages/packages-lock.json` 文件，表明当前工程可能由 Unity 编辑器在打开时自动生成这些文件，或者依赖全部通过 Unity 编辑器 UI 直接安装到 Library 缓存中。

## 2. 关键文件与位置

| 文件 | 作用 |
|---|---|
| `ProjectSettings/PackageManagerSettings.asset` | 定义 UPM 注册表（指向 `packages.unity.cn`） |
| `ProjectSettings/ProjectVersion.txt` | 锁定 Unity Editor 版本为 `2022.3.62f3c1`，间接约束了可安装的包版本范围 |
| `Assets/XR/Loaders/*.asset` | OpenXR / Oculus / ARCore / MockHMD / Simulation 等 XR 运行时加载器配置，代表已引入的 XR 相关包 |
| `Assets/XRI/Settings/*.asset` | XR Interaction 相关设置，对应 Unity XR Interaction Toolkit 包 |
| `Assets/Resources/BillingMode.json` | Unity IAP 的计费模式配置，对应 Unity IAP 包 |
| `Assets/Scripts/InkRidge.asmdef`、`Assets/Editor/InkRidge.Editor.asmdef` | 自定义 Assembly Definition，用于隔离脚本编译域，是 Unity 工程中模块化的基础 |
| `*.csproj`、`InkRidge.sln` | Visual Studio / Rider 解决方案，反映已解析的脚本依赖关系 |

## 3. 架构与约定

- **依赖来源单一**：所有第三方能力均来自 Unity 官方包生态（OpenXR、Oculus、ARCore、XR Interaction Toolkit、Unity IAP 等），通过 UPM 安装后以 `.asset` 配置文件形式存在于 `Assets/XR*` 下。
- **平台化 XR 依赖**：`Assets/XR/Loaders/` 下同时存在多个平台的 Loader（OpenXR、Oculus、ARCore、MockHMD、Simulation），构建时通过 Unity 的 Build Target 选择激活哪一个，属于典型的“多后端可选”依赖策略。
- **编辑器扩展与运行时分离**：`Assets/Editor/` 下的脚本通过独立的 `InkRidge.Editor.asmdef` 声明，仅参与编辑器构建，避免打包进最终应用——这是 Unity 工程中隔离编辑器依赖的标准做法。
- **无 vendoring / 无私有 NuGet/npm 源**：仓库中没有 `vendor/`、`node_modules/`、`__pycache__/` 等第三方代码副本，也没有 `nuget.config`、`.npmrc` 等私有源配置。依赖完全由 UPM + Unity 编辑器缓存管理。

## 4. 约定与约束

- **Unity 版本锁定**：`ProjectVersion.txt` 固定为 `2022.3.62f3c1`，团队应以此版本协作，避免因 Editor 升级导致 UPM 包兼容性问题。
- **统一使用国内 UPM 镜像**：`PackageManagerSettings.asset` 强制使用 `packages.unity.cn`，保证中国大陆网络环境下包拉取可用。
- **不提交 UPM 清单文件**：当前仓库未包含 `Packages/manifest.json` 和 `Packages/packages-lock.json`，因此依赖状态实际上由团队成员本地 Unity 缓存决定，缺乏可复现的依赖快照——这是一个需要改进的点。
- **XR 依赖按需启用**：各 XR 平台 Loader 共存于工程，但实际启用由 Unity 的 XR Settings 控制，不会同时打包进目标平台。
- **无其他语言依赖**：仓库中未发现 Go、Node.js、Python、Java、Rust 等语言的依赖声明文件，因此本项目的依赖管理仅限于 Unity 生态。

## 5. 结论

该仓库的依赖管理完全围绕 Unity Package Manager 展开，采用“官方包 + 国内镜像 + 编辑器版本锁定”的模式，没有引入任何外部语言级别的包管理器或私有注册表。由于缺少 `Packages/manifest.json` / `packages-lock.json`，依赖版本并未以文本形式纳入版本控制，可复现性较弱。