using UnityEditor;
using UnityEngine;

public class BuildAPK
{
    public static void Build()
    {
        string[] scenes = {
            "Assets/Scenes/00_Start.unity",
            "Assets/Scenes/01_Bamboo.unity",
            "Assets/Scenes/02_Waterfall.unity",
            "Assets/Scenes/03_Pavilion.unity",
            "Assets/Scenes/04_Summit.unity",
            "Assets/Scenes/99_End.unity",
        };

        // Set Android build settings
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.development = false;

        // Set player settings
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.bundleVersion = "1.0.0";

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
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"[BuildAPK] {msg.content}");
                }
            }
        }
    }
}
