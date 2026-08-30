using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InkRidge.Core;

public static class AttachBreathReactive
{
    // Attach BreathSceneReactive to the four meditation scenes. MeditationPoint
    // auto-finds it via FindObjectOfType, so wiring is just "add component".
    // Summit also enables the fog response (ink-wash haze that parts on inhale).
    static readonly (string path, bool fog)[] Scenes =
    {
        ("Assets/Scenes/01_Bamboo.unity", false),
        ("Assets/Scenes/02_Waterfall.unity", false),
        ("Assets/Scenes/03_Pavilion.unity", false),
        ("Assets/Scenes/04_Summit.unity", true),
    };

    public static void Run()
    {
        foreach (var (path, fog) in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Idempotent: skip scenes that already carry the component.
            var existing = Object.FindObjectsOfType<BreathSceneReactive>();
            if (existing.Length > 0)
            {
                Debug.Log("[AttachBreathReactive] already present in " + path);
                continue;
            }

            // Parent under the SceneBuilder root so it dies with a rebuild.
            GameObject host = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name.StartsWith("__Scene_")) { host = go; break; }
            }
            if (host == null) host = new GameObject("__Scene_BreathReactive");

            var comp = host.AddComponent<BreathSceneReactive>();
            if (fog)
            {
                var so = new SerializedObject(comp);
                so.FindProperty("_affectFog").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AttachBreathReactive] attached" + (fog ? " (+fog)" : "") + ": " + path);
        }
        Debug.Log("[AttachBreathReactive] Done");
    }
}
