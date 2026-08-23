# InkRidge — Agent Instructions

## Project Overview

InkRidge is a **Unity VR meditation experience** rendered in a traditional Chinese **Gongbi (工笔画) ink-painting** art style. The player walks through a sequence of serene environments, pauses at meditation points for guided breathing exercises, and progresses toward a mountain summit.

- **Engine**: Tuanjie 2022.3.62t12 (Unity 2022.3 LTS fork)
- **Target**: Meta Quest 3 — Android ARM64, IL2CPP
- **XR**: XR Interaction Toolkit 3.x (LocomotionMediator + XRBodyTransformer)
- **Art**: Cel-shaded toon with ink outlines, traditional mineral-pigment palette (GongbiColors)

### Scene Flow

```
00_Start → 01_Bamboo → 02_Waterfall → 03_Pavilion → 04_Summit → 99_End
```

Each scene (01–04) contains a **MeditationPoint**. Completing meditation unlocks the next scene.

---

## Code Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager (singleton), SceneTransition (fade + async load), ComfortSettings (PlayerPrefs)
│   ├── Data/           # SaveManager — JSON save/load at persistentDataPath
│   ├── Environment/    # SceneBuilder base + per-scene builders, WindSystem, GradientFog, ParticleBreath, AmbientAudio, StarfieldRenderer
│   ├── Meditation/     # BreathGuide (plain class, not MonoBehaviour), MeditationPoint (MonoBehaviour trigger), BreathData (serializable record)
│   ├── Movement/       # LocomotionController (smooth move + snap turn), ClimbInteractable
│   ├── UI/             # XRMenu (in-VR settings panel), SummaryScreen (end stats)
│   └── GongbiColors.cs # Static mineral-pigment color palette + Shadow/WarmShadow helpers
├── Shaders/
│   ├── GongbiToon.shader    # Cel-shaded surface + back-face outline pass; supports wind vertex displacement
│   ├── BreathCircle.shader  # Transparent ring driven by _Progress (0–1)
│   └── InkFog.shader        # Height-based distance fog for ink-painting atmosphere
├── Editor/             # Batchmode setup scripts (see scope AGENTS.md)
├── Scenes/             # 6 .unity scenes (00_Start … 99_End)
├── Tests/EditMode/     # EditMode unit tests (BreathGuide, ComfortSettings, SaveManager)
├── Audio/              # Ambience (per-scene .wav), SFX (breath, footstep)
├── Prefabs/            # Climb, Environment, Meditation, Player prefab folders
└── Materials/          # Runtime-generated materials (mostly via SceneBuilder.MakeMat)
```

Scope-level instructions:
- `Assets/Scripts/AGENTS.md` — runtime code conventions
- `Assets/Editor/AGENTS.md` — Editor script conventions

---

## Build & Test

All operations run inside the **Tuanjie/Unity Editor** (no CLI `dotnet build`).

### Build APK

```
# Via Editor menu or batchmode:
Tuanjie -batchmode -executeMethod BuildAPK.Build -quit
# Output: Builds/InkRidge.apk
```

Key build settings (configured in `BuildAPK.cs`):
- ARM64, min SDK 29, IL2CPP, .NET Standard

### Run Tests

Open **Window → General → Test Runner**, select **EditMode**, click **Run All**.

Test files are in `Assets/Tests/EditMode/`:
- `BreathGuideTests.cs` — breathing phase transitions, rhythm stability
- `ComfortSettingsTests.cs` — PlayerPrefs persistence
- `SaveManagerTests.cs` — JSON save/load round-trip

### Editor Setup Scripts

Run via batchmode or the `Debug` menu:

| Method | Purpose |
|--------|---------|
| `SceneSetup.Run()` | Create all 6 scenes, configure XR Origin, build settings |
| `AudioSetup.Setup()` | Wire audio sources + clips into every scene |
| `OptimizeProject.Optimize()` | Quest 3 quality settings, audio compression, player config |
| `SetAndroidPaths.SetPaths()` | Configure Android SDK / NDK / JDK paths |

---

## Key Architecture Conventions

### SceneBuilder Pattern

`SceneBuilder` (abstract base in `Environment/`) is a `MonoBehaviour` that builds all geometry **at runtime** from primitives. Each scene has a derived class:

```
SceneBuilder (base)
├── BambooSceneBuilder
├── WaterfallSceneBuilder
├── PavilionSceneBuilder
└── SummitSceneBuilder
```

**Rules:**
- Override `Build()` (called from `Start()`). Call `base.Build()` first to create `_root`.
- Use helper methods: `Cube()`, `Cylinder()`, `Sphere()`, `Plane()`.
- All materials go through `MakeMat()` or `MakeWindMat()` — these apply `Gongbi/Toon` shader with `GongbiColors` palette.
- Use `SetupSkybox()`, `SetupFog()`, `SetupLighting()` for scene-wide rendering config.
- All generated objects parent under `_root` (`__Scene_{name}`).

### Gongbi Shader System

**`Gongbi/Toon`** — two-pass cel-shading:
1. **Outline pass**: back-face extrusion in view space (`Cull Front`), width via `_OutlineWidth`
2. **Surface pass**: 3-band cel shading (shadow / mid / light) with toon specular

Key properties: `_MainColor`, `_ShadowColor`, `_OutlineColor`, `_OutlineWidth`, `_WindSway`

**Wind integration**: `WindSystem.cs` sets global shader floats (`_WindSpeed`, `_WindMagnitude`, etc.). Materials with `_WindSway > 0` receive vertex displacement proportional to world-space height.

### Singleton Pattern

`GameManager` and `SceneTransition` use a `DontDestroyOnLoad` singleton. Access via `.Instance`.

### Meditation Flow

```
Player enters trigger → 3s gaze confirm → BreathGuide.Start() → 
  phase loop (Inhale → Hold → Exhale → Hold) → session timer expires →
  BreathData saved → GameManager.OnMeditationComplete() → next scene
```

`BreathGuide` is a **plain C# class** (not MonoBehaviour). `MeditationPoint` calls `_breathGuide.Update(Time.deltaTime)` each frame.

### Namespace Convention

All runtime code uses `InkRidge.{Folder}`:
- `InkRidge.Core`, `InkRidge.Data`, `InkRidge.Environment`, `InkRidge.Meditation`, `InkRidge.Movement`, `InkRidge.UI`

`GongbiColors` is in the global namespace (shared palette, no folder prefix).

Editor scripts have no namespace.
