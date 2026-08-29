using UnityEditor;
using UnityEngine;

public class BuildAPK
{
    public static void Build()
    {
        string jdk = "/Applications/Unity/Hub/Editor/2022.3.62f3c1/PlaybackEngines/AndroidPlayer/OpenJDK";
        string sdk = "/Users/a1-6/Library/Android/sdk";
        string ndk = "/Users/a1-6/Library/Android/sdk/ndk/23.1.7779620";
        string cmdlineTools = "/Users/a1-6/Library/Android/sdk/cmdline-tools/6.0";

        // Set ALL possible EditorPrefs keys in THIS batchmode session
        EditorPrefs.SetString("JdkPath", jdk);
        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRootR23B", ndk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);
        EditorPrefs.SetString("AndroidJdkRoot", jdk);
        EditorPrefs.SetString("AndroidJavaTools", jdk + "/bin");
        EditorPrefs.SetString("AndroidSdkCommandLineTools", cmdlineTools);
        EditorPrefs.SetString("AndroidExternalToolsSettings.JdkPath", jdk);
        EditorPrefs.SetString("AndroidExternalToolsSettings.SdkPath", sdk);
        EditorPrefs.SetString("AndroidExternalToolsSettings.NdkPath", ndk);

        // Verify they were set
        Debug.Log("[BuildAPK] JdkPath=" + EditorPrefs.GetString("JdkPath"));
        Debug.Log("[BuildAPK] AndroidSdkRoot=" + EditorPrefs.GetString("AndroidSdkRoot"));
        Debug.Log("[BuildAPK] AndroidNdkRootR23B=" + EditorPrefs.GetString("AndroidNdkRootR23B"));

        // Force build target to Android
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.development = false;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.stripEngineCode = true;

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
            Debug.Log("[BuildAPK] BUILD SUCCESS! Size: " + report.summary.totalSize / 1024 / 1024 + " MB");
        else
            Debug.LogError("[BuildAPK] BUILD FAILED! Result: " + report.summary.result);
    }
}
