using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioSetup
{
    [MenuItem("Debug/Setup Audio in All Scenes")]
    static void Setup()
    {
        string[] scenes = {
            "Assets/Scenes/01_Bamboo.unity",
            "Assets/Scenes/02_Waterfall.unity",
            "Assets/Scenes/03_Pavilion.unity",
            "Assets/Scenes/04_Summit.unity",
        };

        string[] ambienceClips = {
            "Assets/Audio/Ambience/ambience_bamboo",
            "Assets/Audio/Ambience/ambience_waterfall",
            "Assets/Audio/Ambience/ambience_pavilion",
            "Assets/Audio/Ambience/ambience_summit",
        };

        for (int i = 0; i < scenes.Length; i++)
        {
            var scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Single);

            // Find or create AudioRoot
            var root = GameObject.Find("AudioRoot");
            if (root == null)
            {
                root = new GameObject("AudioRoot");
            }

            // Ambient audio source (3D, loop)
            var ambObj = new GameObject("AmbientSource");
            ambObj.transform.SetParent(root.transform);
            ambObj.transform.position = new Vector3(0, 2f, 0);
            var ambSource = ambObj.AddComponent<AudioSource>();
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ambienceClips[i] + ".wav");
            if (clip != null)
            {
                ambSource.clip = clip;
                ambSource.loop = true;
                ambSource.spatialBlend = 1.0f; // 3D
                ambSource.volume = 0f; // will fade in via AmbientAudio
                ambSource.minDistance = 1f;
                ambSource.maxDistance = 50f;
                ambSource.rolloffMode = AudioRolloffMode.Logarithmic;
                Debug.Log($"[AudioSetup] {scenes[i]} ambient clip: {clip.name}");
            }
            else
            {
                Debug.LogWarning($"[AudioSetup] Could not find clip at {ambienceClips[i]}.wav");
            }

            // Add AmbientAudio component
            var ambientAudio = root.GetComponent<InkRidge.Environment.AmbientAudio>();
            if (ambientAudio == null)
                ambientAudio = root.AddComponent<InkRidge.Environment.AmbientAudio>();

            var so = new SerializedObject(ambientAudio);
            var sourcesProp = so.FindProperty("_ambientSources");
            if (sourcesProp != null && sourcesProp.arraySize == 0)
            {
                sourcesProp.InsertArrayElementAtIndex(0);
                sourcesProp.GetArrayElementAtIndex(0).objectReferenceValue = ambSource;
            }
            so.ApplyModifiedProperties();

            // Breath audio source (2D, for meditation)
            var breathObj = new GameObject("BreathSource");
            breathObj.transform.SetParent(root.transform);
            var breathSource = breathObj.AddComponent<AudioSource>();
            breathSource.spatialBlend = 0f; // 2D
            breathSource.volume = 0.6f;
            breathSource.loop = false;

            // Footstep audio source
            var footObj = new GameObject("FootstepSource");
            footObj.transform.SetParent(root.transform);
            var footSource = footObj.AddComponent<AudioSource>();
            footSource.spatialBlend = 0.5f;
            footSource.volume = 0.4f;
            var footClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/footstep_stone.wav");
            if (footClip != null)
                footSource.clip = footClip;

            // Connect footstep to LocomotionController
            var loco = Object.FindObjectOfType<InkRidge.Movement.LocomotionController>();
            if (loco != null)
            {
                var locoSO = new SerializedObject(loco);
                var footProp = locoSO.FindProperty("_footstepSource");
                var footClipProp = locoSO.FindProperty("_footstepClip");
                if (footProp != null) footProp.objectReferenceValue = footSource;
                if (footClipProp != null) footClipProp.objectReferenceValue = footClip;
                locoSO.ApplyModifiedProperties();
                Debug.Log("[AudioSetup] Connected footstep to LocomotionController");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AudioSetup] {scenes[i]} audio configured!");
        }

        Debug.Log("[AudioSetup] All scenes configured with audio!");
    }
}
