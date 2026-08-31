using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Applies the Quest 3 build/profile configuration for InkRidge.
/// Run from Debug > Optimize Project for Quest 3. Safe to run repeatedly.
/// </summary>
public class OptimizeProject
{
    private const string QualityLevelName = "Quest 3";

    /// <summary>
    /// Shaders that must survive build stripping.
    ///
    /// Every scene in this project is generated at runtime: SceneBuilder calls
    /// MakeMat(), which does Shader.Find("Gongbi/Toon"). Nothing under Assets/
    /// holds a serialized reference to most of these, so Unity's player build
    /// treats them as unused and strips them. Shader.Find then returns null at
    /// runtime and the material silently falls back to the error shader
    /// (magenta) — with no warning in the log.
    ///
    /// Today only Gongbi/Toon is reachable from a scene file (00_Start.unity).
    /// Gongbi/InkSkybox, Gongbi/BreathGlow and Hidden/Vignette are referenced
    /// purely by name string and are one stripping pass away from breaking.
    /// </summary>
    private static readonly string[] RequiredShaders =
    {
        "Gongbi/Toon",
        "Gongbi/InkSkybox",
        "Gongbi/BreathCircle",
        "Gongbi/BreathGlow",
        "Gongbi/InkFog",
        "Hidden/Vignette",
    };

    [MenuItem("Debug/Optimize Project for Quest 3")]
    public static void Optimize()
    {
        EnsureAndroidTarget();
        ConfigureQuality();
        ConfigurePlayer();
        ConfigureGraphics();
        OptimizeAudio();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Optimize] Quest 3 profile applied.");
    }

    // ---------------------------------------------------------------- quality

    private static void ConfigureQuality()
    {
        int level = ResolveQualityLevel();
        QualitySettings.SetQualityLevel(level, true);

        // Pixel lights: one directional sun. SceneBuilder.SetupLighting already
        // creates it with LightShadows.None, so keep the shadow system off
        // entirely rather than paying for a shadow pass nobody uses.
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.pixelLightCount = 1;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 30f;
        QualitySettings.shadowCascades = 0;

        // 2x MSAA. The Gongbi outline pass draws expanded back-faces, and
        // aliasing on those ink lines is very visible in VR. 4x is the next
        // step up if the frame budget allows it.
        QualitySettings.antiAliasing = 2;

        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.softVegetation = true;
        QualitySettings.softParticles = false;      // depth-read per particle; not needed here
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.particleRaycastBudget = 64;
        QualitySettings.lodBias = 1.0f;
        QualitySettings.vSyncCount = 0;             // VR compositor owns pacing

        SetAndroidDefaultQuality(level);
        Debug.Log($"[Optimize] Quality '{QualitySettings.names[level]}' configured for Quest 3");
    }

    /// <summary>
    /// Prefer a level named "Quest 3". If it does not exist, fall back to the
    /// top level and say so loudly — we overwrite its values either way.
    /// </summary>
    private static int ResolveQualityLevel()
    {
        var names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == QualityLevelName) return i;
        }

        int fallback = names.Length - 1;
        Debug.LogWarning($"[Optimize] No '{QualityLevelName}' quality level found — " +
                         $"reconfiguring '{names[fallback]}' in place. Rename it to " +
                         $"'{QualityLevelName}' in Project Settings > Quality if you want a dedicated one.");
        return fallback;
    }

    /// <summary>
    /// Pin the chosen level as Android's default. SetQualityLevel alone only
    /// applies to the currently active platform, which is easy to get wrong.
    /// </summary>
    private static void SetAndroidDefaultQuality(int index)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/QualitySettings.asset");
        if (asset == null) return;

        var so = new SerializedObject(asset);
        var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
        if (perPlatform == null)
        {
            Debug.LogWarning("[Optimize] Could not locate m_PerPlatformDefaultQuality — set the Android default manually.");
            return;
        }

        for (int i = 0; i < perPlatform.arraySize; i++)
        {
            var entry = perPlatform.GetArrayElementAtIndex(i);
            var key = entry.FindPropertyRelative("first");
            if (key != null && key.stringValue == "Android")
            {
                entry.FindPropertyRelative("second").intValue = index;
                so.ApplyModifiedProperties();
                return;
            }
        }

        // No Android entry yet — append one.
        int added = perPlatform.arraySize++;
        var newEntry = perPlatform.GetArrayElementAtIndex(added);
        newEntry.FindPropertyRelative("first").stringValue = "Android";
        newEntry.FindPropertyRelative("second").intValue = index;
        so.ApplyModifiedProperties();
    }

    // ----------------------------------------------------------------- player

    private static void ConfigurePlayer()
    {
        // Stereo rendering path is deliberately NOT touched here. See
        // EnableSinglePassInstanced() — the Gongbi shaders are not stereo-instanced
        // yet, and flipping the switch before they are breaks rendering.
        Debug.Log($"[Optimize] Stereo rendering path left as " +
                  $"{PlayerSettings.stereoRenderingPath} (use 'Quest 3: Enable Single-Pass Instanced' " +
                  $"once the shaders declare the stereo macros).");

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

        // NOTE: stereo rendering path is deliberately left alone. See
        // TryEnableSinglePassInstanced() — the custom Gongbi shaders do not
        // declare the instancing/stereo macros, so flipping this on silently
        // breaks VR rendering. Use the separate menu item once the shaders
        // have been patched.

        // Skip the final swapchain blit — VR renders straight to the compositor.
        PlayerSettings.Android.blitType = AndroidBlitType.Never;
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.Android.androidIsGame = true;   // Quest: appear in the game library, not a 2D panel

        PlayerSettings.MTRendering = true;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.stripUnusedMeshComponents = true;

        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";

        Debug.Log("[Optimize] Player settings configured (ARM64 / IL2CPP Release / no-blit / frame pacing)");
    }

    private static void EnsureAndroidTarget()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) return;

        Debug.LogWarning($"[Optimize] Active build target is " +
                         $"{EditorUserBuildSettings.activeBuildTarget}, not Android. " +
                         $"Player settings are written for Android but the quality-level " +
                         $"default may land on the wrong platform. Switch to Android and re-run.");
    }

    /// <summary>
    /// Single-pass instanced stereo is the biggest remaining rendering win: it
    /// halves the number of scene traversals and draw-call submissions per
    /// frame, which is exactly what the Gongbi two-pass (outline + surface)
    /// shader is most expensive at.
    ///
    /// It cannot be turned on yet. Single-pass instancing requires every
    /// shader to resolve per-eye matrices through the stereo macros, and none
    /// of the Gongbi shaders declare them. Enabling it anyway silently
    /// renders both eyes from eye 0's view matrix — flat, wrong stereo with no
    /// error in the log. GongbiToon's outline pass is a raw vertex/fragment
    /// shader reading unity_ObjectToWorld directly, so it is the worst case.
    ///
    /// This entry point reports exactly what is missing and refuses to flip
    /// the switch until the shaders are patched.
    /// </summary>
    [MenuItem("Debug/Quest 3: Enable Single-Pass Instanced")]
    public static void EnableSinglePassInstanced()
    {
        if (PlayerSettings.stereoRenderingPath == StereoRenderingPath.Instancing)
        {
            Debug.Log("[Optimize] Single-pass instanced is already enabled.");
            return;
        }

        var missing = new System.Collections.Generic.List<string>();
        foreach (var path in GongbiShaderPaths)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) continue;

            string source = System.IO.File.ReadAllText(path);
            bool hasStereoMacros =
                source.Contains("UNITY_VERTEX_INPUT_INSTANCE_ID") ||
                source.Contains("UNITY_VERTEX_OUTPUT_STEREO");

            if (!hasStereoMacros)
                missing.Add(System.IO.Path.GetFileName(path));
        }

        if (missing.Count > 0)
        {
            Debug.LogError("[Optimize] Single-pass instanced NOT enabled — these shaders " +
                           "declare no stereo-instancing macros and would render flat/wrong:\n  " +
                           string.Join("\n  ", missing) +
                           "\n\nEach raw vertex/fragment pass needs UNITY_VERTEX_INPUT_INSTANCE_ID " +
                           "in appdata, UNITY_SETUP_INSTANCE_ID(v) at the top of vert, and " +
                           "UNITY_VERTEX_OUTPUT_STEREO / UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO " +
                           "in v2f. Surface-shader passes additionally need the instance ID " +
                           "threaded into their custom vertex function. Verify on device afterwards.");
            return;
        }

        PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
        Debug.Log("[Optimize] Single-pass instanced enabled.");
    }

    private static readonly string[] GongbiShaderPaths =
    {
        "Assets/Shaders/GongbiToon.shader",
        "Assets/Shaders/GongbiInkSkybox.shader",
        "Assets/Shaders/BreathCircle.shader",
        "Assets/Shaders/BreathGlow.shader",
        "Assets/Shaders/InkFog.shader",
        "Assets/Shaders/Vignette.shader",
    };

    // --------------------------------------------------------------- graphics

    private static void ConfigureGraphics()
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
        if (asset == null)
        {
            Debug.LogWarning("[Optimize] GraphicsSettings.asset not found.");
            return;
        }

        var so = new SerializedObject(asset);
        var shaders = so.FindProperty("m_AlwaysIncludedShaders");
        if (shaders == null)
        {
            Debug.LogWarning("[Optimize] Could not locate m_AlwaysIncludedShaders.");
            return;
        }

        shaders.ClearArray();
        int added = 0;
        foreach (var name in RequiredShaders)
        {
            var shader = Shader.Find(name);
            if (shader == null)
            {
                Debug.LogWarning($"[Optimize] Shader '{name}' not found — skipped.");
                continue;
            }

            shaders.InsertArrayElementAtIndex(added);
            shaders.GetArrayElementAtIndex(added).objectReferenceValue = shader;
            added++;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"[Optimize] Always-included shaders: {added}/{RequiredShaders.Length} registered");
    }

    // ------------------------------------------------------------------ audio

    private static void OptimizeAudio()
    {
        // Discover clips instead of hardcoding paths, so new ambience tracks are
        // picked up automatically.
        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("[Optimize] No AudioClips found under Assets/Audio.");
            return;
        }

        int optimized = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            // Ambience loops are long: stream them rather than holding the whole
            // decoded buffer in memory. One-shots stay decompressed so they fire
            // without a decode hitch.
            bool isAmbience = path.IndexOf("ambience", System.StringComparison.OrdinalIgnoreCase) >= 0;

            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.loadType = isAmbience
                ? AudioClipLoadType.Streaming
                : AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = !isAmbience;

            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            optimized++;
        }

        Debug.Log($"[Optimize] Audio optimized: {optimized} clip(s)");
    }
}
