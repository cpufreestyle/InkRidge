using UnityEditor;
using UnityEngine;

public class SetAndroidPaths
{
    public static void Run()
    {
        EditorPrefs.SetString("AndroidSdkRoot", "/Users/a1-6/Library/Android/sdk");
        EditorPrefs.SetString("JdkPath", "/Applications/Tuanjie/Hub/Editor/2022.3.62t12/PlaybackEngines/AndroidPlayer/OpenJDK");
        EditorPrefs.SetString("AndroidNdkRootR23B", "/Users/a1-6/Library/Android/sdk/ndk/23.1.7779620");
        
        // Also set via PlayerSettings
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        Debug.Log("[SetAndroidPaths] SDK=" + EditorPrefs.GetString("AndroidSdkRoot"));
        Debug.Log("[SetAndroidPaths] JDK=" + EditorPrefs.GetString("JdkPath"));
        Debug.Log("[SetAndroidPaths] NDK=" + EditorPrefs.GetString("AndroidNdkRootR23B"));
    }
}
