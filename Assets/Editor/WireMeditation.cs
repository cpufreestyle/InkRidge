using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InkRidge.Core;
using InkRidge.Environment;
using InkRidge.Meditation;

/// <summary>
/// Wires the breath feedback components into the four meditation scenes.
///
/// Both BreathHaptics and BreathAudioSync were written but never placed in a
/// single scene — their .meta GUIDs appear zero times across Assets/Scenes,
/// and every MeditationPoint has `_haptics` / `_breathAudio` left at
/// {fileID: 0}. MeditationPoint.Start() only falls back to
/// GetComponent(), which finds nothing when the component was never added,
/// so the haptics and breath SFX stayed silent despite being wired correctly
/// in code. This script is the missing step.
///
/// Idempotent: safe to re-run. Scenes that are already wired are skipped.
/// </summary>
public static class WireMeditation
{
    static readonly string[] Scenes =
    {
        "Assets/Scenes/01_Bamboo.unity",
        "Assets/Scenes/02_Waterfall.unity",
        "Assets/Scenes/03_Pavilion.unity",
        "Assets/Scenes/04_Summit.unity",
    };

    const string InhaleClipPath = "Assets/Audio/SFX/breath_inhale.wav";
    const string ExhaleClipPath = "Assets/Audio/SFX/breath_exhale.wav";

    [MenuItem("Debug/Wire Meditation Feedback")]
    public static void Run()
    {
        var inhale = AssetDatabase.LoadAssetAtPath<AudioClip>(InhaleClipPath);
        var exhale = AssetDatabase.LoadAssetAtPath<AudioClip>(ExhaleClipPath);

        if (inhale == null || exhale == null)
        {
            Debug.LogError(
                $"[WireMeditation] Breath clips missing. Expected '{InhaleClipPath}' and " +
                $"'{ExhaleClipPath}'. Nothing was wired.");
            return;
        }

        int scenesWired = 0;
        int pointsWired = 0;

        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var points = Object.FindObjectsOfType<MeditationPoint>(includeInactive: true);
            if (points.Length == 0)
            {
                Debug.LogWarning($"[WireMeditation] No MeditationPoint in {path} — skipped.");
                continue;
            }

            var sceneReactive = Object.FindObjectOfType<BreathSceneReactive>(includeInactive: true);
            if (sceneReactive == null)
                Debug.LogWarning($"[WireMeditation] No BreathSceneReactive in {path}. Run AttachBreathReactive first.");

            bool dirty = false;
            foreach (var point in points)
            {
                if (WirePoint(point, sceneReactive, inhale, exhale))
                {
                    pointsWired++;
                    dirty = true;
                }
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                scenesWired++;
            }

            Debug.Log($"[WireMeditation] {path}: {points.Length} MeditationPoint(s) checked.");
        }

        Debug.Log($"[WireMeditation] Done. Wired {pointsWired} point(s) across {scenesWired} scene(s).");
    }

    /// <summary>Returns true if anything changed.</summary>
    static bool WirePoint(MeditationPoint point, BreathSceneReactive sceneReactive,
                          AudioClip inhale, AudioClip exhale)
    {
        bool changed = false;
        var go = point.gameObject;
        var so = new SerializedObject(point);

        // --- haptics -------------------------------------------------------
        var hapticsProp = so.FindProperty("_haptics");
        if (hapticsProp != null && hapticsProp.objectReferenceValue == null)
        {
            var haptics = go.GetComponent<BreathHaptics>();
            if (haptics == null)
            {
                haptics = go.AddComponent<BreathHaptics>();
                Debug.Log($"[WireMeditation] + BreathHaptics on '{go.name}' ({go.scene.name})");
            }
            hapticsProp.objectReferenceValue = haptics;
            changed = true;
        }

        // --- breath sfx ----------------------------------------------------
        var audioProp = so.FindProperty("_breathAudio");
        if (audioProp != null)
        {
            var sync = audioProp.objectReferenceValue as BreathAudioSync;
            if (sync == null)
            {
                sync = go.GetComponent<BreathAudioSync>();
                if (sync == null)
                {
                    sync = go.AddComponent<BreathAudioSync>();
                    Debug.Log($"[WireMeditation] + BreathAudioSync on '{go.name}' ({go.scene.name})");
                }
                audioProp.objectReferenceValue = sync;
                changed = true;
            }

            // Clips live on the BreathAudioSync component, not the point.
            var syncSo = new SerializedObject(sync);
            var inhaleProp = syncSo.FindProperty("_inhaleClip");
            var exhaleProp = syncSo.FindProperty("_exhaleClip");

            if (inhaleProp != null && inhaleProp.objectReferenceValue == null)
            {
                inhaleProp.objectReferenceValue = inhale;
                changed = true;
            }
            if (exhaleProp != null && exhaleProp.objectReferenceValue == null)
            {
                exhaleProp.objectReferenceValue = exhale;
                changed = true;
            }
            syncSo.ApplyModifiedPropertiesWithoutUndo();

            // BreathAudioSync declares [RequireComponent(typeof(AudioSource))],
            // so this exists — but make sure it is not muted or 3D-panned.
            var source = sync.GetComponent<AudioSource>();
            if (source != null && (source.spatialBlend > 0.001f || source.mute))
            {
                source.spatialBlend = 0f;
                source.mute = false;
                EditorUtility.SetDirty(source);
                changed = true;
            }
        }

        // --- scene-reactive (fog / light response) -------------------------
        var reactiveProp = so.FindProperty("_sceneReactive");
        if (reactiveProp != null && reactiveProp.objectReferenceValue == null && sceneReactive != null)
        {
            reactiveProp.objectReferenceValue = sceneReactive;
            changed = true;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        if (changed) EditorUtility.SetDirty(point);
        return changed;
    }

    /// <summary>Verification pass: reports what is still unwired.</summary>
    [MenuItem("Debug/Verify Meditation Wiring")]
    public static void Verify()
    {
        Debug.Log("=== WireMeditation verification ===");
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var point in Object.FindObjectsOfType<MeditationPoint>(includeInactive: true))
            {
                var go = point.gameObject;
                bool hasHaptics = go.GetComponent<BreathHaptics>() != null;
                bool hasAudio = go.GetComponent<BreathAudioSync>() != null;
                var sync = go.GetComponent<BreathAudioSync>();
                bool clipsOk = sync != null && sync.IsConfigured;

                string status = (hasHaptics && hasAudio && clipsOk) ? "OK" : "INCOMPLETE";
                Debug.Log($"[{status}] {go.scene.name} / {go.name} — " +
                          $"haptics={hasHaptics} audio={hasAudio} clips={clipsOk}");
            }
        }
        Debug.Log("=== end ===");
    }
}
