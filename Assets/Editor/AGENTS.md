# Editor — Agent Instructions

Unity Editor scripts for InkRidge project setup, build, and optimization. Compiled into the `InkRidge.Editor` assembly (`InkRidge.Editor.asmdef`).

## Conventions

- No namespace — all Editor scripts are in the global namespace.
- All public methods are `static` — designed to be invoked via Unity batchmode (`-executeMethod ClassName.MethodName`) or from the Editor menu.
- Use `EditorUtility.SetDirty()` / `EditorSceneManager.MarkSceneDirty()` before saving.
- Log prefix convention: `[ClassName]` — e.g., `[SceneSetup]`, `[AudioSetup]`, `[BuildAPK]`.

## Script Inventory

| Script | Menu / Batch Entry | Purpose |
|--------|--------------------|---------|
| `SceneSetup.cs` | `SceneSetup.Run()` | Creates all 6 scenes, configures XR Origin, Player Settings, Build Settings. ~400 lines, most complex. |
| `AudioSetup.cs` | `Debug/Setup Audio in All Scenes` | Iterates gameplay scenes, creates AudioRoot with ambient/breath/footstep sources, wires clips. |
| `BuildAPK.cs` | `BuildAPK.Build()` | Configures SDK paths, sets IL2CPP + ARM64, builds APK to `Builds/InkRidge.apk`. |
| `OptimizeProject.cs` | `Debug/Optimize Project for Quest 3` | Quality settings (2x MSAA, low shadows), audio import compression (Vorbis), player stripping. |
| `SetAndroidPaths.cs` | `SetAndroidPaths.SetPaths()` | Sets `EditorPrefs` for Android SDK, NDK, JDK paths. |

## Scene Setup Details

`SceneSetup.cs` is the primary bootstrap script. Key patterns:

- **XR Origin creation** (`CreateXROrigin()`): creates `XROrigin` + `XRBodyTransformer` + `Camera` with `TrackedPoseDriver` + `CameraOffset` at y=1.75.
- **Locomotion wiring** (`AddLocomotion()`): creates `LocomotionMediator`, `ContinuousMoveProvider`, `SnapTurnProvider` as child GameObjects; uses `SerializedObject` to wire mediator references.
- **Meditation points** (`AddMeditationPoint()`): creates trigger `BoxCollider` (4×3×4), `MeditationPoint` component, breath circle quad with `Gongbi/BreathCircle` shader, `ParticleSystem` + `ParticleBreath`.
- **SerializedObject usage**: required when setting private `[SerializeField]` fields programmatically (no public setters).

## Adding a New Scene to Build

1. Add scene path to `SceneSetup.EnsureScenesInBuildSettings()` array.
2. Add scene path to `BuildAPK.Build()` `scenes` array.
3. Add scene setup logic (copy from existing scene method pattern).
4. Add audio setup in `AudioSetup.cs` — add scene path and ambience clip to the arrays.

## Build Pipeline Order

```
SceneSetup.Run()          → creates scenes + XR rig
AudioSetup.Setup()        → wires audio into scenes
OptimizeProject.Optimize() → applies Quest 3 quality settings
BuildAPK.Build()          → produces APK
```

All scripts can be run independently or chained. Idempotent — safe to re-run.
