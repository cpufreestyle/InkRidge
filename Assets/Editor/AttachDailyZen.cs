using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InkRidge.Environment;

public static class AttachDailyZen
{
    // Attach DailyZen to the four meditation scenes (mounts under the
    // SceneBuilder root). DailyZen varies fog color and wind direction per
    // calendar day; the Summit keeps density swing at 0 because
    // BreathSceneReactive drives Summit fog density during meditation.
    static readonly (string path, float densitySwing)[] Scenes =
    {
        ("Assets/Scenes/01_Bamboo.unity", 0.30f),
        ("Assets/Scenes/02_Waterfall.unity", 0.30f),
        ("Assets/Scenes/03_Pavilion.unity", 0.30f),
        ("Assets/Scenes/04_Summit.unity", 0f),
    };

    public static void Run()
    {
        foreach (var (path, swing) in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var existing = Object.FindObjectsOfType<DailyZen>();
            if (existing.Length > 0)
            {
                Debug.Log("[AttachDailyZen] already present in " + path);
                continue;
            }

            GameObject host = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name.StartsWith("__Scene_")) { host = go; break; }
            }
            if (host == null) host = new GameObject("__Scene_DailyZen");

            var comp = host.AddComponent<DailyZen>();
            var so = new SerializedObject(comp);
            so.FindProperty("_fogDensitySwing").floatValue = swing;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AttachDailyZen] attached (densitySwing={swing}): " + path);
        }
        Debug.Log("[AttachDailyZen] Done");
    }
}
