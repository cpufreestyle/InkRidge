using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;

public class FixLocomotion
{
    [MenuItem("Debug/Fix Locomotion in Current Scene")]
    static void Fix()
    {
        var scene = SceneManager.GetActiveScene();
        Debug.Log($"[FixLocomotion] Fixing scene: {scene.name}");

        // Find all ContinuousMoveProvider and SnapTurnProvider
        var moveProviders = Object.FindObjectsByType<ContinuousMoveProvider>(FindObjectsSortMode.None);
        var turnProviders = Object.FindObjectsByType<SnapTurnProvider>(FindObjectsSortMode.None);
        var xrOrigins = Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None);

        Debug.Log($"[FixLocomotion] Found: {moveProviders.Length} move, {turnProviders.Length} turn, {xrOrigins.Length} origin");

        foreach (var origin in xrOrigins)
        {
            var go = origin.gameObject;

            // Add XRBodyTransformer if missing
            var body = go.GetComponent<XRBodyTransformer>();
            if (body == null)
            {
                body = go.AddComponent<XRBodyTransformer>();
                Debug.Log($"[FixLocomotion] Added XRBodyTransformer to {go.name}");
            }

            // Add LocomotionMediator if missing
            var mediator = go.GetComponent<LocomotionMediator>();
            if (mediator == null)
            {
                mediator = go.AddComponent<LocomotionMediator>();
                Debug.Log($"[FixLocomotion] Added LocomotionMediator to {go.name}");
            }

            // Connect providers to mediator
            foreach (var mp in moveProviders)
            {
                if (mp.mediator == null)
                {
                    mp.mediator = mediator;
                    Debug.Log($"[FixLocomotion] Connected {mp.gameObject.name} to mediator");
                }
            }
            foreach (var tp in turnProviders)
            {
                if (tp.mediator == null)
                {
                    tp.mediator = mediator;
                    Debug.Log($"[FixLocomotion] Connected {tp.gameObject.name} to mediator");
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixLocomotion] Scene saved with locomotion fix!");
    }
}
