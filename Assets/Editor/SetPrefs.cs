using UnityEngine;
using UnityEditor;

public static class SetPrefs
{
    public static void Run()
    {
        EditorPrefs.SetString("AndroidSdkRoot", "/Users/a1-6/Library/Android/sdk");
        EditorPrefs.SetString("AndroidNdkRootR23B", "/Users/a1-6/Library/Android/sdk/ndk/23.1.7779620");
        EditorPrefs.SetString("JdkPath", "/Applications/Tuanjie/Hub/Editor/2022.3.62t12/PlaybackEngines/AndroidPlayer/OpenJDK");
        Debug.Log("AndroidNdkRootR23B=" + EditorPrefs.GetString("AndroidNdkRootR23B"));
        Debug.Log("JdkPath=" + EditorPrefs.GetString("JdkPath"));
        Debug.Log("AndroidSdkRoot=" + EditorPrefs.GetString("AndroidSdkRoot"));
        EditorApplication.Exit(0);
    }
}
