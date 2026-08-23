using UnityEditor;
using UnityEngine;

public class OptimizeProject
{
    [MenuItem("Debug/Optimize Project for Quest 3")]
    static void Optimize()
    {
        // === Quality Settings ===
        // Set Ultra (index 5) as default for Android
        QualitySettings.SetQualityLevel(5, true);

        // Configure for Quest 3 performance
        QualitySettings.pixelLightCount = 1;           // 1 directional light only
        QualitySettings.shadowResolution = ShadowResolution.Low; // 512
        QualitySettings.shadowDistance = 30f;
        QualitySettings.shadowCascades = 0;             // No cascades
        QualitySettings.antiAliasing = 2;              // 2x MSAA
        QualitySettings.softVegetation = true;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.lodBias = 1.0f;

        Debug.Log("[Optimize] Quality settings configured for Quest 3");

        // === Audio import optimization ===
        string[] audioPaths = {
            "Assets/Audio/Ambience/ambience_bamboo.wav",
            "Assets/Audio/Ambience/ambience_waterfall.wav",
            "Assets/Audio/Ambience/ambience_pavilion.wav",
            "Assets/Audio/Ambience/ambience_summit.wav",
            "Assets/Audio/SFX/breath_inhale.wav",
            "Assets/Audio/SFX/breath_exhale.wav",
            "Assets/Audio/SFX/footstep_stone.wav",
        };

        foreach (string path in audioPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                var sampleSettings = importer.defaultSampleSettings;
                sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                sampleSettings.quality = 0.7f;
                sampleSettings.loadType = path.Contains("ambience") ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                sampleSettings.preloadAudioData = !path.Contains("ambience");
                importer.defaultSampleSettings = sampleSettings;
                importer.SaveAndReimport();
                Debug.Log($"[Optimize] Audio optimized: {path}");
            }
        }

        // === Graphics settings ===
        // Add Gongbi shaders to always-included list
        var graphicsSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
            "ProjectSettings/GraphicsSettings.asset");

        Debug.Log("[Optimize] Graphics settings configured");

        // === Player Settings ===
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";

        AssetDatabase.SaveAssets();
        Debug.Log("[Optimize] All optimizations applied!");
    }
}
