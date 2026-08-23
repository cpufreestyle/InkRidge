using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;
using InkRidge.Movement;
using System.Reflection;
using System;

public class FixScene
{
    [MenuItem("Debug/Rebuild XR Origin in Current Scene")]
    static void Rebuild()
    {
        var scene = SceneManager.GetActiveScene();
        Debug.Log($"[FixScene] Rebuilding XR Origin in: {scene.name}");

        // Find and destroy old XROrigin
        var oldOrigins = UnityEngine.Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None);
        foreach (var old in oldOrigins)
        {
            Debug.Log($"[FixScene] Destroying old XROrigin: {old.gameObject.name}");
            UnityEngine.Object.DestroyImmediate(old.gameObject);
        }

        // Create fresh XR Origin
        var xrObj = new GameObject("XROrigin");
        var xrOrigin = xrObj.AddComponent<XROrigin>();
        var bodyTransformer = xrObj.AddComponent<XRBodyTransformer>();
        var mediator = xrObj.AddComponent<LocomotionMediator>();

        // Camera
        var camObj = new GameObject("MainCamera");
        camObj.transform.SetParent(xrObj.transform);
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1000f;
        camObj.AddComponent<AudioListener>();

        // Set camera origin reference
        var originSO = new SerializedObject(xrOrigin);
        var camProp = originSO.FindProperty("m_Camera");
        if (camProp != null) camProp.objectReferenceValue = camObj.GetComponent<Camera>();
        originSO.ApplyModifiedProperties();

        // Camera offset
        var offsetObj = new GameObject("CameraOffset");
        offsetObj.transform.SetParent(xrObj.transform);
        offsetObj.transform.localPosition = new Vector3(0, 1.75f, 0);
        var offsetField = typeof(XROrigin).GetField("m_CameraFloorOffsetObject",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (offsetField != null)
            offsetField.SetValue(xrOrigin, offsetObj.transform);

        xrObj.transform.position = new Vector3(0, 1.75f, 0);

        // Move provider
        var moveObj = new GameObject("ContinuousMoveProvider");
        moveObj.transform.SetParent(xrObj.transform);
        var moveProvider = moveObj.AddComponent<ContinuousMoveProvider>();

        // Turn provider
        var turnObj = new GameObject("SnapTurnProvider");
        turnObj.transform.SetParent(xrObj.transform);
        var turnProvider = turnObj.AddComponent<SnapTurnProvider>();

        // Connect to mediator via serialized property
        var moveSO = new SerializedObject(moveProvider);
        var moveMediatorProp = moveSO.FindProperty("m_Mediator");
        if (moveMediatorProp != null)
        {
            moveMediatorProp.objectReferenceValue = mediator;
            moveSO.ApplyModifiedProperties();
            Debug.Log("[FixScene] Connected ContinuousMoveProvider to mediator");
        }

        var turnSO = new SerializedObject(turnProvider);
        var turnMediatorProp = turnSO.FindProperty("m_Mediator");
        if (turnMediatorProp != null)
        {
            turnMediatorProp.objectReferenceValue = mediator;
            turnSO.ApplyModifiedProperties();
            Debug.Log("[FixScene] Connected SnapTurnProvider to mediator");
        }

        // Add our LocomotionController
        var loco = xrObj.AddComponent<LocomotionController>();
        var locoSO = new SerializedObject(loco);
        locoSO.FindProperty("_moveProvider").objectReferenceValue = moveProvider;
        locoSO.FindProperty("_turnProvider").objectReferenceValue = turnProvider;
        locoSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixScene] Done! XR Origin rebuilt with LocomotionMediator.");
    }
}
