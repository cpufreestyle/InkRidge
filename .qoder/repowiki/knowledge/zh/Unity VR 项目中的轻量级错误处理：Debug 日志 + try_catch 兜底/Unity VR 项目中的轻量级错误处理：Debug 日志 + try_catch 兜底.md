---
kind: error_handling
name: Unity VR 项目中的轻量级错误处理：Debug 日志 + try/catch 兜底
category: error_handling
scope:
    - '**'
source_files:
    - Assets/Scripts/Data/SaveManager.cs
    - Assets/Scripts/Core/VignetteDriver.cs
    - Assets/Scripts/Core/VignetteEffect.cs
    - Assets/Scripts/Meditation/MeditationPoint.cs
---

## 1. 使用的系统/方法

该 Unity/Tuanjie VR 项目没有引入统一的异常框架、错误码枚举或自定义异常类型。错误处理采用 Unity 原生方式：
- 使用 `UnityEngine.Debug.LogWarning` / `Debug.LogError` 记录运行时问题。
- 对可能抛出 I/O 异常的持久化操作（`SaveManager`）使用 C# 标准 `try { ... } catch (Exception e)` 包裹，捕获后降级为默认值并记录错误。
- 通过组件缺失检查（`GetComponent` / `FindObjectOfType` 返回 null）配合警告日志实现“优雅降级”，而非抛出异常中断流程。

项目中未发现 `throw new`、自定义 Exception 子类、全局异常处理器（如 `Application.logMessageReceived`）、`Assert` 或单元测试断言等机制。

## 2. 关键文件与位置

| 文件 | 作用 |
|---|---|
| `Assets/Scripts/Data/SaveManager.cs` | 唯一集中使用 `try/catch(Exception)` 的位置；Load/Save 失败时记录 `Debug.LogError` 并返回空数据/静默忽略 |
| `Assets/Scripts/Core/VignetteDriver.cs` | 缺少依赖组件时 `Debug.LogWarning` 并禁用自身 |
| `Assets/Scripts/Core/VignetteEffect.cs` | Shader 未找到时 `Debug.LogWarning` 并禁用自身 |
| `Assets/Scripts/Meditation/MeditationPoint.cs` | 场景缺少 `AmbientAudio` 时 `Debug.LogWarning`，后续逻辑通过可选字段（`?` 调用）继续运行 |

## 3. 架构与约定

- **无统一错误类型**：所有错误以字符串消息形式直接写入 Unity Console，没有错误码、错误对象或可被上层业务判断的返回值。
- **降级优先于中断**：当资源缺失（Vignette shader、VignetteEffect 组件、AmbientAudio）时，代码选择记录警告并跳过相关功能，而不是让游戏崩溃。这是本项目最稳定的错误处理模式。
- **I/O 异常局部消化**：只有 `SaveManager.Load` / `Save` 两个入口用 try/catch 包裹 JSON 序列化/反序列化；其他模块不主动捕获异常。
- **无全局中间件**：作为 Unity MonoBehaviour 驱动的游戏，不存在类似 Web 框架的错误中间件；错误在各自脚本生命周期（Awake/Start/Update/OnRenderImage）内就地处理。
- **无 panic/recover 对应物**：C# 中不使用 `throw` 控制正常流程，也没有 `finally` 之外的恢复逻辑。

## 4. 约定与约束

- **可观察性约定**：运行时异常路径必须通过 `Debug.LogError` 输出带上下文的消息（见 `SaveManager.Load failed: {e.Message}`、`SaveManager.Save failed: {e.Message}`），便于在 Unity Console 中定位。
- **缺失依赖的约定**：组件/Shader 查找失败时先 `Debug.LogWarning` 再 `enabled = false` 或跳过渲染，保证主循环继续运行。
- **持久化容错约定**：保存/加载失败时不向调用方暴露异常，而是返回安全默认值（`new SaveFile()`），使上层业务无需感知存储层故障。
- **约束来源**：这些行为由现有脚本的实现模式所体现，并非来自文档化的强制规范；仓库中未见 `.editorconfig`、`AGENTS.md`（仅存在 `Assets/Editor/AGENTS.md` 和 `Assets/Scripts/AGENTS.md`，内容未涉及错误策略）或 CI lint 规则对此进行强制约束。

总体而言，这是一个面向单体验证阶段的轻量错误处理方案：以 `Debug.Log*` 作为主要诊断手段，辅以最小范围的 try/catch 保护易错的 I/O 路径，并通过组件级降级避免单点故障扩散到整个游戏循环。