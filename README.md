# 墨岭 Ink Ridge

> Meta Quest 3 VR 探索 + 冥想游戏 — 水墨工笔风格

玩家在一幅水墨山水画中徒步登山，沿途经过 4 个冥想点，通过呼吸引导完成正念体验。

## 截景

| 场景 | 意境 | 呼吸法 |
|---|---|---|
| 🎋 竹林溪径 | 清幽 | 4-4-4 均衡呼吸 |
| 💧 岩壁瀑布 | 激荡 | 4-7-8 放松呼吸 |
| 🏯 云雾古亭 | 空灵 | 箱式呼吸 4-4-4-4 |
| 🌙 山顶星空 | 释然 | 深长自由呼吸 |

## 技术栈

- **引擎**: Tuanjie 2022.3.62t12 (Unity 2022.3 LTS 内核)
- **渲染**: Built-in RP + Gongbi/Toon shader (三段色阶 cel shading + 背面描边)
- **XR**: Meta XR SDK 4.5.1 + XR Interaction Toolkit 3.2.2
- **音频**: 7 个 AI 生成音频（4 环境音 + 呼吸 SFX + 脚步声）
- **开源组件**: Google Daydream Elements (风系统/渐变雾/星空渲染, Apache 2.0)

## 功能

- ✅ 4 个水墨风格场景（竹林/瀑布/古亭/山顶）
- ✅ 呼吸引导系统（4 种呼吸模式，光圈可视化）
- ✅ 风系统（竹子/树木随风摇摆，Gongbi shader 顶点位移）
- ✅ 渐变雾效（天顶/地平线双色雾）
- ✅ 程序化星空（Fibonacci 分布，闪烁，暖色变体）
- ✅ VR 舒适度设置（移动速度/转身角度/vignette/坐站模式）
- ✅ 本地数据持久化（冥想记录 JSON 存档）
- ✅ 攀岩段（XR 手柄抓握攀爬）
- ✅ 脚步声系统
- ✅ 场景异步加载 + 黑屏过渡

## 项目结构

```
InkRidge/
├── Assets/
│   ├── Shaders/
│   │   ├── GongbiToon.shader       # 工笔卡通 shader + 风位移
│   │   ├── BreathCircle.shader     # 呼吸引导光圈
│   │   └── InkFog.shader           # 水墨大气雾
│   ├── Scripts/
│   │   ├── Core/                   # GameManager, SceneTransition, ComfortSettings
│   │   ├── Movement/               # LocomotionController, ClimbInteractable
│   │   ├── Meditation/            # BreathGuide, BreathData, MeditationPoint
│   │   ├── Environment/           # SceneBuilder, WindSystem, GradientFog, StarfieldRenderer
│   │   ├── UI/                    # XRMenu, SummaryScreen
│   │   └── Data/                  # SaveManager
│   ├── Scenes/                    # 6 个场景 (00_Start ~ 99_End)
│   ├── Audio/                     # 7 个音频文件
│   └── Tests/                     # 22 个 EditMode 测试
└── Builds/
    └── InkRidge.apk               # 35MB ARM64 IL2CPP
```

## 构建

```bash
# Tuanjie batchmode 构建
/Applications/Tuanjie/Hub/Editor/2022.3.62t12/Tuanjie.app/Contents/MacOS/Tuanjie \
  -batchmode -projectPath . -executeMethod BuildAPK.Build -quit

# Gradle 打包（阿里云镜像）
cd Library/Bee/Android/Prj/IL2CPP/Gradle
gradle assembleDebug --no-daemon
```

## License

MIT
