using UnityEditor;
using UnityEngine;

public class BuildAPK
{
    public static void Build()
    {
        // Ensure SDK paths
        EditorPrefs.SetString("AndroidSdkRoot", "/Users/a1-6/Library/Android/sdk");
        EditorPrefs.SetString("JdkPath", "/Applications/Tuanjie/Hub/Editor/2022.3.62t12/PlaybackEngines/AndroidPlayer/OpenJDK");
        EditorPrefs.SetString("AndroidNdkRootR23B", "/Users/a1-6/Library/Android/sdk/ndk/23.1.7779620");

        // Use Gradle settings that skip SDK update check
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.development = false;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";

        string[] scenes = {
            "Assets/Scenes/00_Start.unity",
            "Assets/Scenes/01_Bamboo.unity",
            "Assets/Scenes/02_Waterfall.unity",
            "Assets/Scenes/03_Pavilion.unity",
            "Assets/Scenes/04_Summit.unity",
            "Assets/Scenes/99_End.unity",
        };

        string apkPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Builds/InkRidge.apk");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(apkPath));

        var report = BuildPipeline.BuildPlayer(scenes, apkPath, BuildTarget.Android, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[BuildAPK] BUILD SUCCESS! APK at: {apkPath}");
            Debug.Log($"[BuildAPK] Size: {report.summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[BuildAPK] BUILD FAILED! Result: {report.summary.result}");
        }
    }
}
