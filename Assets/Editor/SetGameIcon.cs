using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetGameIcon
{
    // Assign the ink-wash icon to PlayerSettings (Android + default), then
    // rebuild. Quest reads the 512 adaptive icon from the APK manifest.
    const string IconPath = "Assets/Editor/Icons/icon_512.png";

    public static void Run()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (tex == null)
        {
            Debug.LogError("[SetGameIcon] icon not found at " + IconPath);
            EditorApplication.Exit(1);
            return;
        }

        // Import as a sprite-free readable texture is not required for icons.
        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";

        var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Android);
        var set = new Texture2D[icons.Length];
        // Unity icon slots: smallest first (48..192); assign 512 to the largest
        // slot(s), letting Unity downscale, and also set as the high-res slot.
        for (int i = 0; i < set.Length; i++)
        {
            set[i] = (i == set.Length - 1) ? tex : (icons[i] != null ? icons[i] : tex);
        }
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, set);

        AssetDatabase.SaveAssets();
        Debug.Log("[SetGameIcon] icon assigned for Android (" + set.Length + " slots)");
    }
}
