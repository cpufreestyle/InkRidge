using UnityEditor;
using UnityEngine;

public class BuildAPK
{
    public static void Build()
    {
        string sdkRoot = "/Users/a1-6/Library/Android/sdk";
        string jdkPath = "/Applications/Tuanjie/Hub/Editor/2022.3.62t12/PlaybackEngines/AndroidPlayer/OpenJDK";
        string ndkPath = "/Users/a1-6/Library/Android/sdk/ndk/23.1.7779620";

        EditorPrefs.SetString("AndroidSdkRoot", sdkRoot);
        EditorPrefs.SetString("JdkPath", jdkPath);
        EditorPrefs.SetString("AndroidNdkRootR23B", ndkPath);

        // Set SDK cmdline tools path (required by Unity)
        EditorPrefs.SetString("AndroidSdkCommandLineTools", sdkRoot + "/cmdline-tools/6.0");

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.development = false;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        
        // Fix input handling - use Input System only (not Both)

        
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.stripEngineCode = true;

        Debug.Log("[BuildAPK] SDK: " + sdkRoot);
        Debug.Log("[BuildAPK] JDK: " + jdkPath);
        Debug.Log("[BuildAPK] NDK: " + ndkPath);

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
