# Scripts — Agent Instructions

Runtime gameplay code for InkRidge. All scripts here are compiled into the `InkRidge` assembly (`InkRidge.asmdef`).

## Namespace Convention

Every subfolder maps to a namespace:

| Folder | Namespace | Purpose |
|--------|-----------|---------|
| `Core/` | `InkRidge.Core` | GameManager (singleton), SceneTransition (fade), ComfortSettings (static) |
| `Data/` | `InkRidge.Data` | SaveManager — JSON save at `persistentDataPath/inkridge_save.json` |
| `Environment/` | `InkRidge.Environment` | SceneBuilder base class + per-scene builders, WindSystem, GradientFog, ParticleBreath, AmbientAudio, StarfieldRenderer |
| `Meditation/` | `InkRidge.Meditation` | BreathGuide (plain class), MeditationPoint (MonoBehaviour), BreathData (DTO) |
| `Movement/` | `InkRidge.Movement` | LocomotionController (XR Origin), ClimbInteractable |
| `UI/` | `InkRidge.UI` | XRMenu (in-VR settings), SummaryScreen (end screen) |

Exception: `GongbiColors.cs` at root level has no namespace — it's a global static palette.

## Coding Conventions

- **Fields**: private `_camelCase` with `[SerializeField]`; public properties use `PascalCase`.
- **Singletons**: `DontDestroyOnLoad` pattern — only `GameManager` and `SceneTransition` use it.
- **Scene geometry**: always built at runtime via `SceneBuilder` subclasses, never placed in the `.unity` scene file directly.
- **Materials**: always created through `SceneBuilder.MakeMat()` / `MakeWindMat()` — never `new Material()` elsewhere.
- **Shaders**: referenced by string name `"Gongbi/Toon"`, `"Gongbi/BreathCircle"`, `"Gongbi/InkFog"`.
- **Wind**: `WindSystem` sets global shader floats; individual materials opt in via `_WindSway > 0`.
- **Colors**: all scene colors come from `GongbiColors` static palette. Use `GongbiColors.Shadow(mainColor)` for shadow tones.
- **XR**: uses XR Interaction Toolkit 3.x (`LocomotionMediator`, `ContinuousMoveProvider`, `SnapTurnProvider`).
- **BreathGuide**: plain C# class (not MonoBehaviour) — instantiated and ticked by `MeditationPoint`.
- **Save data**: `SaveManager` is static; `BreathData` is `[Serializable]` with `ToJson()`/`FromJson()`.

## Adding a New Scene

1. Create `Scenes/NN_Name.unity` (copy from existing scene as template).
2. Create `Environment/NameSceneBuilder.cs` extending `SceneBuilder`. Override `Build()` and call `base.Build()` first.
3. Add scene index to `GameManager` serialized fields and `BuildAPK.cs` scene array.
4. Add to `SceneSetup.EnsureScenesInBuildSettings()` array.
5. Place a `MeditationPoint` trigger in the scene; configure `_sceneIndex`, `_pattern`, `_sessionDuration`.

## Common Modification Patterns

**Change movement speed defaults** → `Core/ComfortSettings.cs` constants (`DEFAULT_MOVE_SPEED`, etc.)

**Add a breath pattern** → `Meditation/BreathGuide.cs`: add enum value to `Pattern`, add row to `PatternTimings[,]`.

**Modify art style** → `Shaders/GongbiToon.shader` for rendering; `GongbiColors.cs` for palette; per-scene `*SceneBuilder.cs` for geometry.

**Add wind to new objects** → use `SceneBuilder.MakeWindMat()` with `_WindSway > 0`; ensure `WindSystem` component exists in the scene.

**Modify save schema** → `Data/SaveManager.cs` `SaveFile` class — maintain backward compatibility (new fields get defaults on deserialization).
