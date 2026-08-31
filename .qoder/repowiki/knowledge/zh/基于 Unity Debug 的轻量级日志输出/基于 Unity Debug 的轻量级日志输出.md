---
kind: logging_system
name: 基于 Unity Debug 的轻量级日志输出
category: logging_system
scope:
    - '**'
source_files:
    - Assets/Editor/BuildAPK.cs
    - Assets/Editor/AudioSetup.cs
    - Assets/Editor/OptimizeProject.cs
    - Assets/Editor/InstallCodelyBridge.cs
    - Assets/Editor/AddStartGate.cs
    - Assets/Editor/AttachBreathReactive.cs
    - Assets/Scripts/Core/VRPerformanceBootstrap.cs
    - Assets/Scripts/Core/VignetteDriver.cs
    - Assets/Scripts/Core/VignetteEffect.cs
    - Assets/Scripts/Data/SaveManager.cs
    - Assets/Scripts/Environment/DailyZen.cs
    - Assets/Scripts/Meditation/MeditationPoint.cs
---

## 1. 使用的系统/方法

本项目没有引入任何第三方日志框架（如 NLog、Serilog、log4net、Microsoft.Extensions.Logging），也没有自定义 Logger 抽象或日志门面。所有运行时与编辑器脚本均直接使用 Unity 内置的 `UnityEngine.Debug` API：
- `Debug.Log(...)` — 常规信息
- `Debug.LogWarning(...)` — 警告
- `Debug.LogError(...)` — 错误

日志输出目标为 Unity Console / Player Log，未配置文件 sink、结构化字段或远程上报通道。

## 2. 关键文件

- `Assets/Editor/*.cs`：大量 Editor 工具脚本使用 `Debug.Log/Warning/Error` 记录构建、资源装配、优化流程状态（例如 `BuildAPK.cs`、`AudioSetup.cs`、`OptimizeProject.cs`、`InstallCodelyBridge.cs`、`AddStartGate.cs`、`AttachBreathReactive.cs`）。
- `Assets/Scripts/Core/VRPerformanceBootstrap.cs`：在 VR 性能初始化时输出 FFR 设置结果。
- `Assets/Scripts/Core/VignetteDriver.cs`、`Assets/Scripts/Core/VignetteEffect.cs`：在缺少组件或着色器时发出 `Debug.LogWarning`。
- `Assets/Scripts/Data/SaveManager.cs`：在 Save/Load 异常时通过 `Debug.LogError` 输出失败原因。
- `Assets/Scripts/Environment/DailyZen.cs`：记录每日打卡事件。
- `Assets/Scripts/Meditation/MeditationPoint.cs`：在条件不满足时输出警告。

## 3. 架构与约定

- **无集中式日志模块**：每个脚本直接调用 `Debug.Log/Warning/Error`，不存在统一的日志入口、日志级别枚举或可注入的 logger 接口。
- **前缀约定**：日志消息普遍以方括号标签开头标识来源，如 `[AddStartGate]`、`[AudioSetup]`、`[BuildAPK]`、`[VRPerf]`、`[VignetteDriver]`、`[DailyZen]`，便于在 Unity Console 中快速筛选。
- **级别选择**：正常流程走 `Debug.Log`；配置缺失/降级走 `Debug.LogWarning`；数据持久化失败等异常情况走 `Debug.LogError`。
- **无结构化字段**：日志为纯字符串拼接或 `$"..."` 插值，不包含 JSON/XML 结构体、trace id、时间戳等结构化字段。
- **无开关/过滤**：代码中没有按环境（开发/发布）切换日志级别的逻辑，也没有全局开关来批量禁用日志输出。

## 4. 约定与约束

- **约定（描述性）**：所有日志都通过 `UnityEngine.Debug` 输出到 Unity Console；Editor 脚本用带类名前缀的消息记录工具执行过程；运行时脚本仅在关键路径（性能设置、异常、配置缺失）输出日志。
- **约束（实际存在）**：项目未集成任何外部日志库，也未定义自定义日志抽象；因此无法在不修改源码的情况下切换到其他后端（文件、网络、结构化格式）。当前实现完全依赖 Unity 引擎提供的 `Debug` 子系统。

总体而言，这是一个非常轻量的日志实践：仅使用 Unity 原生 `Debug` API，配合统一的前缀标签风格，适合小型 VR 项目的调试需求，但不具备生产级日志系统的特性（结构化、分级控制、多 sink、采样等）。