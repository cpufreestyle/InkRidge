using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InkRidge.UI;

public static class AddStartGate
{
    // Attach StartGate to the 00_Start scene: no script previously called
    // GameManager.StartGame(), so the player could never leave the start scene.
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/00_Start.unity", OpenSceneMode.Single);

        GameObject gate = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "StartGate") { gate = go; break; }
        }

        if (gate == null)
        {
            gate = new GameObject("StartGate");
            UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(gate, scene);
        }

        var gateComp = gate.GetComponent<StartGate>() ?? gate.AddComponent<StartGate>();

        // Wire the stele as gaze target.
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "StoneStele")
            {
                var so = new SerializedObject(gateComp);
                so.FindProperty("_gazeTarget").objectReferenceValue = go.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[AddStartGate] gaze target -> StoneStele");
                break;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AddStartGate] Done");
    }
}
