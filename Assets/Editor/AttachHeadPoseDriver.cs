using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AttachHeadPoseDriver
{
    // Attach HeadPoseDriver to the XR rig camera in all scenes. The scene's
    // TrackedPoseDriver has empty InputAction bindings, so the camera never
    // follows the headset — the player sees a static flat view ("not VR").

    static readonly string[] Scenes =
    {
        "Assets/Scenes/00_Start.unity",
        "Assets/Scenes/01_Bamboo.unity",
        "Assets/Scenes/02_Waterfall.unity",
        "Assets/Scenes/03_Pavilion.unity",
        "Assets/Scenes/04_Summit.unity",
        "Assets/Scenes/99_End.unity",
    };

    public static void Run()
    {
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Find the camera GO (either "MainCamera" in XROrigin, or default).
            GameObject camGo = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                var t = go.transform;
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    if (child.name == "MainCamera")
                    {
                        camGo = child.gameObject;
                        break;
                    }
                }
                if (camGo != null) break;

                // Deeper: any GO named MainCamera
                var found = GameObject.Find("MainCamera");
                if (found != null) { camGo = found; break; }
            }
            if (camGo == null)
            {
                Debug.LogError($"[AttachHeadPoseDriver] No MainCamera in {path}");
                continue;
            }

            if (camGo.GetComponent<InkRidge.Movement.HeadPoseDriver>() == null)
                camGo.AddComponent<InkRidge.Movement.HeadPoseDriver>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AttachHeadPoseDriver] attached: {path}");
        }
        Debug.Log("[AttachHeadPoseDriver] Done");
    }
}
