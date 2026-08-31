using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Android (Quest 3) APK build entry points.
///   Debug > Build APK          -> Builds/InkRidge.apk          (stable name, overwritten)
///   Debug > Build APK (dated)  -> Builds/InkRidge_1.0.0_20260830-1548.apk (archived)
///
/// The dated variant exists because the previous workflow was "build once, then
/// hand-rename to _opt / _fix / _vk / _gate2 so the next build doesn't clobber
/// it". That is how Builds/ accumulated fifteen near-identical 22 MB APKs with
/// no record of what any of them contained. Prefer Build APK (dated) when the
/// result is worth keeping; use the stable name for adb scripts.
/// </summary>
public class BuildAPK
{
    /// <summary>Single source of truth for scene order. Must match
    /// SceneSetup.EnsureScenesInBuildSettings() and GameManager's indices.</summary>
    public static readonly string[] Scenes =
    {
        "Assets/Scenes/00_Start.unity",
        "Assets/Scenes/01_Bamboo.unity",
        "Assets/Scenes/02_Waterfall.unity",
        "Assets/Scenes/03_Pavilion.unity",
        "Assets/Scenes/04_Summit.unity",
        "Assets/Scenes/99_End.unity",
    };

    /// <summary>Project root, derived from the Assets folder rather than the
    /// current directory — batchmode's CWD is wherever the shell happened to be.</summary>
    private static string ProjectRoot =>
        Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    [MenuItem("Debug/Build APK")]
    public static void Build() => Run("InkRidge.apk");

    [MenuItem("Debug/Build APK (dated)")]
    public static void BuildDated()
    {
        string stamp = System.DateTime.Now.ToString("yyyyMMdd-HHmm");
        Run($"InkRidge_{PlayerSettings.bundleVersion}_{stamp}.apk");
    }

    private static void Run(string fileName)
    {
        ConfigureAndroidToolchain();

        string outputDir = Path.Combine(ProjectRoot, "Builds");
        string apkPath = Path.Combine(outputDir, fileName);
        Directory.CreateDirectory(outputDir);

        Debug.Log($"[BuildAPK] Output: {apkPath}");

        var report = BuildPipeline.BuildPlayer(Scenes, apkPath, BuildTarget.Android, BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[BuildAPK] BUILD SUCCESS! {report.summary.totalSize / 1048576} MB -> {apkPath}");
        else
            Debug.LogError($"[BuildAPK] BUILD FAILED! {report.summary.result} " +
                           $"({report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings)");
    }

    /// <summary>
    /// Point the editor at the Android toolchain.
    ///
    /// The JDK path used to be hardcoded to a sibling *Unity* 2022.3.62f3c1
    /// install. This project has since moved to Tuanjie 2022.3.62t12
    /// (ProjectVersion.txt), so that path only worked by luck — remove that
    /// Unity install and every build breaks. Resolve the JDK next to whichever
    /// editor is actually running, and fall back only if it is missing.
    /// </summary>
    private static void ConfigureAndroidToolchain()
    {
        string jdk = ResolveJdkPath();
        string sdk = "/Users/a1-6/Library/Android/sdk";
        string ndk = sdk + "/ndk/23.1.7779620";

        EditorPrefs.SetString("JdkPath", jdk);
        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        EditorPrefs.SetString("AndroidNdkRootR23B", ndk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);
        EditorPrefs.SetString("AndroidJdkRoot", jdk);
        EditorPrefs.SetString("AndroidJavaTools", jdk + "/bin");
        EditorPrefs.SetString("AndroidSdkCommandLineTools", sdk + "/cmdline-tools/6.0");
        EditorPrefs.SetString("AndroidExternalToolsSettings.JdkPath", jdk);
        EditorPrefs.SetString("AndroidExternalToolsSettings.SdkPath", sdk);
        EditorPrefs.SetString("AndroidExternalToolsSettings.NdkPath", ndk);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.development = false;

        // Everything else lives in OptimizeProject. These few are repeated so a
        // build is still correct on a machine that never ran the profile.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = true;

        Debug.Log($"[BuildAPK] JDK={jdk} (exists: {Directory.Exists(jdk)})");
    }

    private static string ResolveJdkPath()
    {
        // EditorApplication.applicationPath is the running editor binary; the
        // bundled JDK sits alongside it under PlaybackEngines.
        string editorDir = Path.GetDirectoryName(EditorApplication.applicationPath);
        string bundled = Path.Combine(editorDir, "PlaybackEngines/AndroidPlayer/OpenJDK");
        if (Directory.Exists(bundled)) return bundled;

        const string legacy = "/Applications/Unity/Hub/Editor/2022.3.62f3c1/PlaybackEngines/AndroidPlayer/OpenJDK";
        Debug.LogWarning($"[BuildAPK] No bundled JDK at '{bundled}'; falling back to '{legacy}'.");
        return legacy;
    }
}
