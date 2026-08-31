---
kind: configuration_system
name: 基于 PlayerPrefs + JSON 的运行时配置与持久化系统
category: configuration_system
scope:
    - '**'
source_files:
    - Assets/Scripts/Core/ComfortSettings.cs
    - Assets/Scripts/Data/SaveManager.cs
    - Assets/Scripts/UI/XRMenu.cs
    - Assets/Editor/SetPrefs.cs
    - ProjectSettings/ProjectSettings.asset
    - ProjectSettings/AudioManager.asset
    - ProjectSettings/GraphicsSettings.asset
    - ProjectSettings/InputManager.asset
    - ProjectSettings/QualitySettings.asset
    - ProjectSettings/XRPackageSettings.asset
    - Assets/XR/Settings/OpenXR Package Settings.asset
    - Assets/XRI/Settings/XRInteractionEditorSettings.asset
    - Assets/Resources/BillingMode.json
---

## 1. 使用的系统与方案

本项目采用 **双轨制** 的配置/持久化方案：
- **运行时用户偏好（VR Comfort Settings）**：通过 Unity `PlayerPrefs` 存储，由 `Assets/Scripts/Core/ComfortSettings.cs` 提供统一的静态 API。
- **游戏进度与统计数据**：通过 `Assets/Scripts/Data/SaveManager.cs` 将结构化数据序列化为 JSON 文件，保存在 `Application.persistentDataPath/inkridge_save.json`。

此外，Unity 工程级配置集中在 `ProjectSettings/*.asset`、`XR/Settings/*.asset`、`XRI/Settings/*.asset` 等 Unity 原生设置文件中；编辑器专用路径通过 `Assets/Editor/SetPrefs.cs` 写入 `EditorPrefs`。

## 2. 关键文件与包

| 文件 | 职责 |
|---|---|
| `Assets/Scripts/Core/ComfortSettings.cs` | 所有 VR 舒适设置的唯一入口，封装 `PlayerPrefs` 读写 |
| `Assets/Scripts/Data/SaveManager.cs` | JSON 序列化/反序列化保存文件，管理冥想记录、解锁场景、每日打卡 |
| `Assets/Scripts/UI/XRMenu.cs` | 在 VR 中暴露设置面板，绑定到 `ComfortSettings` 属性 |
| `Assets/Editor/SetPrefs.cs` | 编辑器工具脚本，写入 Android SDK/NDK/JDK 路径到 `EditorPrefs` |
| `ProjectSettings/*.asset` | Unity 引擎级配置（音频、图形、输入、质量、XR 等） |
| `Assets/Resources/BillingMode.json` | Unity IAP 计费模式配置 |

## 3. 架构与设计约定

### 3.1 运行时偏好：单例式静态类
`ComfortSettings` 是 `InkRidge.Core` 命名空间下的 `static class`，每个偏好项以 `const string KEY_*` 常量定义键名（如 `"Comfort_MoveSpeed"`），并以 C# 自动属性暴露。默认值集中声明为 `DEFAULT_*` 常量（移动速度 1.2f、转向角 30°、晕影开启、站立模式、触觉反馈开启）。新增偏好需遵循相同模式：添加 KEY 常量 → DEFAULT 常量 → 属性 getter/setter。

### 3.2 强制即时持久化
注释明确说明：Quest 后台杀进程会阻止 `PlayerPrefs` 正常 flush，因此每个 setter 都调用 `PlayerPrefs.Save()` 强制落盘。这是项目对平台限制的显式约束。

### 3.3 应用层联动
- `ApplySeatedMode(Transform xrOrigin)` 根据 `SeatedMode` 调整 XR Origin 高度（坐姿 0.8m / 站姿 1.75m）。
- `XRMenu` 作为 UI 层，启动时从 `ComfortSettings` 读取当前值填充滑块/Toggle，并在变更回调中写回。
- `VignetteEffect`、`BreathHaptics`、`LocomotionController` 等组件直接消费 `ComfortSettings` 的属性值驱动行为。

### 3.4 存档数据：JSON + JsonUtility
`SaveManager.SaveFile` 使用 `[Serializable]` 标记，字段包括最高解锁场景、总冥想时长、总行走时长、会话数、冥想记录列表、每日打卡日期列表。`Load()` 失败返回空对象而非抛异常，保证鲁棒性；`Save()` 用 `JsonUtility.ToJson(data, true)` 格式化输出。文件路径通过 `Path.Combine(Application.persistentDataPath, "inkridge_save.json")` 计算。

### 3.5 编辑器配置
`SetPrefs.Run()` 是一个一次性编辑器工具，将本机 Android SDK、NDK、JDK 路径写入 `EditorPrefs` 并退出编辑器，用于初始化构建环境。

## 4. 约定与约束

- **偏好键命名规范**：统一使用 `Comfort_` 前缀的字符串常量（`KEY_MOVE_SPEED`、`KEY_TURN_ANGLE`、`KEY_VIGNETTE`、`KEY_SEATED`、`KEY_HAPTICS`），避免硬编码散落的字符串。
- **布尔值存储约定**：`PlayerPrefs` 不原生支持 bool，统一用 `GetInt(key, default ? 1 : 0) == 1` 读取、`SetInt(value ? 1 : 0)` 写入。
- **每次修改即持久化**：所有 setter 必须调用 `PlayerPrefs.Save()`，不可依赖 Unity 退出时的自动保存。
- **UI 层只读/写桥接**：`XRMenu` 仅负责把 UI 控件的值同步到 `ComfortSettings`，不持有状态副本；其他模块通过 `ComfortSettings` 读取最新值。
- **存档文件位置固定**：始终位于 `Application.persistentDataPath/inkridge_save.json`，不存在则创建新对象。
- **错误处理策略**：`SaveManager.Load/Save` 捕获异常并通过 `Debug.LogError` 记录，不中断主流程。
- **Unity 工程配置**：所有引擎级参数（音频、图形、输入、质量、XR 加载器、OpenXR 设置等）均通过 ProjectSettings 和 XR/XRI 子目录下的 `.asset` 文件管理，不在代码中硬编码。
- **资源级配置**：`Assets/Resources/BillingMode.json` 用于 Unity IAP 计费模式，属于打包期配置。

## 5. 未覆盖的范围

项目中未发现以下机制：
- 无外部配置文件（`.env`、`.yaml`、`.toml`、`.json` 形式的运行时配置）被程序加载。
- 无 Feature Flag 系统或远程配置服务。
- 无多环境（开发/测试/生产）配置切换逻辑。
- 无加密或校验的敏感信息存储（`PlayerPrefs` 明文保存，JSON 存档明文存放）。
- 无配置迁移或版本兼容逻辑（`SaveFile` 结构未包含版本号字段）。

综上，该项目的配置系统简单直接：用户偏好走 `PlayerPrefs`，游戏进度走本地 JSON，引擎配置走 Unity 原生设置文件，没有引入第三方配置框架。