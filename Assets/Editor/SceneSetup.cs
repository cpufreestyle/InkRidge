using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;
using InkRidge.Core;
using InkRidge.Movement;
using InkRidge.Meditation;
using InkRidge.Environment;
using InkRidge.UI;

/// <summary>
/// Automated scene setup script. Run via Tuanjie batchmode.
/// Creates all 6 scenes, configures Player Settings, adds to Build Settings.
/// </summary>
public class SceneSetup
{
    public static void Run()
    {
        Debug.Log("[SceneSetup] Starting automated scene setup...");

        ConfigurePlayerSettings();
        EnsureScenesInBuildSettings();
        SetupBambooScene();
        SetupStartScene();
        SetupEndScene();
        CreateEmptyScenes();

        Debug.Log("[SceneSetup] Done! All scenes created.");
    }

    static void ConfigurePlayerSettings()
    {
        Debug.Log("[SceneSetup] Configuring Player Settings...");

        PlayerSettings.companyName = "MichaelQiu";
        PlayerSettings.productName = "InkRidge";
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

        AssetDatabase.SaveAssets();
        Debug.Log("[SceneSetup] Player Settings configured.");
    }

    static void EnsureScenesInBuildSettings()
    {
        string[] scenePaths = {
            "Assets/Scenes/00_Start.unity",
            "Assets/Scenes/01_Bamboo.unity",
            "Assets/Scenes/02_Waterfall.unity",
            "Assets/Scenes/03_Pavilion.unity",
            "Assets/Scenes/04_Summit.unity",
            "Assets/Scenes/99_End.unity",
        };

        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach (string path in scenePaths)
        {
            buildScenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("[SceneSetup] Build scenes configured (6 scenes).");
    }

    static void SetupBambooScene()
    {
        Debug.Log("[SceneSetup] Setting up Bamboo scene...");
        string scenePath = "Assets/Scenes/01_Bamboo.unity";

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add BambooSceneBuilder
        var sceneRoot = new GameObject("SceneRoot");
        sceneRoot.AddComponent<BambooSceneBuilder>();

        // Add GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // Add SceneTransition with fade canvas
        var stObj = new GameObject("SceneTransition");
        var st = stObj.AddComponent<SceneTransition>();
        var canvas = new GameObject("FadeCanvas");
        canvas.transform.SetParent(stObj.transform);
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        var cg = canvas.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        // Set via serialization-friendly approach
        var stSO = new SerializedObject(st);
        stSO.FindProperty("_fadeCanvas").objectReferenceValue = cg;
        stSO.FindProperty("_fadeDuration").floatValue = 1.5f;
        stSO.ApplyModifiedProperties();

        // Add XR Origin
        var xrOriginObj = CreateXROrigin();
        xrOriginObj.transform.position = new Vector3(0, 1.75f, 0);

        // Add LocomotionController
        var loco = xrOriginObj.AddComponent<LocomotionController>();
        var moveProviderObj = new GameObject("ContinuousMoveProvider");
        moveProviderObj.transform.SetParent(xrOriginObj.transform);
        var moveProvider = moveProviderObj.AddComponent<ContinuousMoveProvider>();
        var turnProviderObj = new GameObject("SnapTurnProvider");
        turnProviderObj.transform.SetParent(xrOriginObj.transform);
        var turnProvider = turnProviderObj.AddComponent<SnapTurnProvider>();

        var locoSO = new SerializedObject(loco);
        locoSO.FindProperty("_moveProvider").objectReferenceValue = moveProvider;
        locoSO.FindProperty("_turnProvider").objectReferenceValue = turnProvider;
        locoSO.ApplyModifiedProperties();

        // Add MeditationPoint with trigger zone
        var meditationObj = new GameObject("MeditationPoint");
        meditationObj.transform.position = new Vector3(0, 0, 60f); // at end of trail
        var trigger = meditationObj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(4f, 3f, 4f);
        var mp = meditationObj.AddComponent<MeditationPoint>();

        var mpSO = new SerializedObject(mp);
        mpSO.FindProperty("_sceneIndex").intValue = 1;
        mpSO.FindProperty("_sceneName").stringValue = "Bamboo";
        mpSO.FindProperty("_pattern").enumValueIndex = 0; // Balanced444
        mpSO.FindProperty("_sessionDuration").floatValue = 180f;
        mpSO.ApplyModifiedProperties();

        // Add breath circle quad
        var breathQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        breathQuad.name = "BreathCircle";
        breathQuad.transform.SetParent(meditationObj.transform);
        breathQuad.transform.position = new Vector3(0, 1.5f, 2f);
        // Create BreathCircle material
        var breathMat = new Material(Shader.Find("Gongbi/BreathCircle"));
        breathQuad.GetComponent<Renderer>().material = breathMat;
        breathQuad.GetComponent<Renderer>().enabled = false;

        var mpSO2 = new SerializedObject(mp);
        mpSO2.FindProperty("_breathCircleRenderer").objectReferenceValue = breathQuad.GetComponent<Renderer>();
        mpSO2.ApplyModifiedProperties();

        // Add ParticleBreath
        var particleObj = new GameObject("BreathParticles");
        particleObj.transform.SetParent(meditationObj.transform);
        particleObj.transform.localPosition = Vector3.zero;
        var ps = particleObj.AddComponent<ParticleSystem>();
        var pb = particleObj.AddComponent<ParticleBreath>();

        var mpSO3 = new SerializedObject(mp);
        mpSO3.FindProperty("_particles").objectReferenceValue = pb;
        mpSO3.ApplyModifiedProperties();

        // Tag player
        // Add "Player" tag if not exists, then tag the XR Origin camera
        var cam = xrOriginObj.GetComponentInChildren<Camera>();
        if (cam != null) cam.gameObject.tag = "Player";

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[SceneSetup] Bamboo scene saved.");
    }

    static void SetupStartScene()
    {
        Debug.Log("[SceneSetup] Setting up Start scene...");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // SceneTransition
        var stObj = new GameObject("SceneTransition");
        var st = stObj.AddComponent<SceneTransition>();
        var canvas = new GameObject("FadeCanvas");
        canvas.transform.SetParent(stObj.transform);
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        var cg = canvas.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        var stSO = new SerializedObject(st);
        stSO.FindProperty("_fadeCanvas").objectReferenceValue = cg;
        stSO.ApplyModifiedProperties();

        // Stone stele
        var stele = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stele.name = "StoneStele";
        stele.transform.position = new Vector3(0, 1.2f, 3f);
        stele.transform.localScale = new Vector3(0.4f, 2.4f, 0.15f);
        var steleMat = new Material(Shader.Find("Gongbi/Toon"));
        steleMat.SetColor("_MainColor", new Color(0.77f, 0.60f, 0.42f));
        steleMat.SetColor("_ShadowColor", new Color(0.42f, 0.33f, 0.23f));
        steleMat.SetColor("_OutlineColor", new Color(0.1f, 0.08f, 0.06f, 0.9f));
        steleMat.SetFloat("_OutlineWidth", 0.01f);
        stele.GetComponent<Renderer>().material = steleMat;

        // XR Origin
        var xrOriginObj = CreateXROrigin();
        xrOriginObj.transform.position = new Vector3(0, 1.75f, 0);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/00_Start.unity");
        Debug.Log("[SceneSetup] Start scene saved.");
    }

    static void SetupEndScene()
    {
        Debug.Log("[SceneSetup] Setting up End scene...");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // SceneTransition
        var stObj = new GameObject("SceneTransition");
        var st = stObj.AddComponent<SceneTransition>();
        var canvas = new GameObject("FadeCanvas");
        canvas.transform.SetParent(stObj.transform);
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        var cg = canvas.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        var stSO = new SerializedObject(st);
        stSO.FindProperty("_fadeCanvas").objectReferenceValue = cg;
        stSO.ApplyModifiedProperties();

        // Summary screen
        var summaryObj = new GameObject("SummaryScreen");
        var summaryCanvas = summaryObj.AddComponent<Canvas>();
        summaryCanvas.renderMode = RenderMode.WorldSpace;
        summaryObj.transform.position = new Vector3(0, 1.5f, 2f);
        summaryObj.AddComponent<SummaryScreen>();

        // XR Origin
        var xrOriginObj = CreateXROrigin();
        xrOriginObj.transform.position = new Vector3(0, 1.75f, 0);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/99_End.unity");
        Debug.Log("[SceneSetup] End scene saved.");
    }

    static void CreateEmptyScenes()
    {
        // Waterfall scene
        Debug.Log("[SceneSetup] Creating Waterfall scene...");
        var wfScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var wfRoot = new GameObject("SceneRoot");
        wfRoot.AddComponent<WaterfallSceneBuilder>();
        AddCoreManagers(wfScene);
        var wfXr = CreateXROrigin();
        wfXr.transform.position = new Vector3(0, 1.75f, 5f);
        AddMeditationPoint(wfScene, new Vector3(0, 20.1f, 2f), 2, "Waterfall", BreathGuide.Pattern.Relax478, 180f);
        AddLocomotion(wfXr);
        EditorSceneManager.SaveScene(wfScene, "Assets/Scenes/02_Waterfall.unity");

        // Pavilion scene
        Debug.Log("[SceneSetup] Creating Pavilion scene...");
        var pvScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var pvRoot = new GameObject("SceneRoot");
        pvRoot.AddComponent<PavilionSceneBuilder>();
        AddCoreManagers(pvScene);
        var pvXr = CreateXROrigin();
        pvXr.transform.position = new Vector3(0, 1.75f, 5f);
        AddMeditationPoint(pvScene, new Vector3(0, 0.15f, 0), 3, "Pavilion", BreathGuide.Pattern.Box4444, 180f);
        AddLocomotion(pvXr);
        EditorSceneManager.SaveScene(pvScene, "Assets/Scenes/03_Pavilion.unity");

        // Summit scene
        Debug.Log("[SceneSetup] Creating Summit scene...");
        var smScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var smRoot = new GameObject("SceneRoot");
        smRoot.AddComponent<SummitSceneBuilder>();
        AddCoreManagers(smScene);
        var smXr = CreateXROrigin();
        smXr.transform.position = new Vector3(0, 1.75f, 0);
        AddMeditationPoint(smScene, new Vector3(0, 0.25f, 0), 4, "Summit", BreathGuide.Pattern.Free, 180f);
        AddLocomotion(smXr);
        EditorSceneManager.SaveScene(smScene, "Assets/Scenes/04_Summit.unity");

        Debug.Log("[SceneSetup] All scenes created.");
    }

    static void AddCoreManagers(UnityEngine.SceneManagement.Scene scene)
    {
        // GameManager + SceneTransition (if not already persistent)
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        var stObj = new GameObject("SceneTransition");
        var st = stObj.AddComponent<SceneTransition>();
        var canvas = new GameObject("FadeCanvas");
        canvas.transform.SetParent(stObj.transform);
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        var cg = canvas.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        var stSO = new SerializedObject(st);
        stSO.FindProperty("_fadeCanvas").objectReferenceValue = cg;
        stSO.ApplyModifiedProperties();
    }

    static void AddLocomotion(GameObject xrOriginObj)
    {
        var loco = xrOriginObj.AddComponent<LocomotionController>();
        var moveProviderObj = new GameObject("ContinuousMoveProvider");
        moveProviderObj.transform.SetParent(xrOriginObj.transform);
        var moveProvider = moveProviderObj.AddComponent<ContinuousMoveProvider>();
        var turnProviderObj = new GameObject("SnapTurnProvider");
        turnProviderObj.transform.SetParent(xrOriginObj.transform);
        var turnProvider = turnProviderObj.AddComponent<SnapTurnProvider>();

        var locoSO = new SerializedObject(loco);
        locoSO.FindProperty("_moveProvider").objectReferenceValue = moveProvider;
        locoSO.FindProperty("_turnProvider").objectReferenceValue = turnProvider;
        locoSO.ApplyModifiedProperties();
    }

    static void AddMeditationPoint(UnityEngine.SceneManagement.Scene scene, Vector3 pos,
        int sceneIndex, string sceneName, BreathGuide.Pattern pattern, float duration)
    {
        var mpObj = new GameObject("MeditationPoint");
        mpObj.transform.position = pos;
        var trigger = mpObj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(4f, 3f, 4f);
        var mp = mpObj.AddComponent<MeditationPoint>();

        var mpSO = new SerializedObject(mp);
        mpSO.FindProperty("_sceneIndex").intValue = sceneIndex;
        mpSO.FindProperty("_sceneName").stringValue = sceneName;
        mpSO.FindProperty("_pattern").enumValueIndex = (int)pattern;
        mpSO.FindProperty("_sessionDuration").floatValue = duration;
        mpSO.ApplyModifiedProperties();

        // Breath circle
        var breathQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        breathQuad.name = "BreathCircle";
        breathQuad.transform.SetParent(mpObj.transform);
        breathQuad.transform.localPosition = new Vector3(0, 0, 2f);
        var breathMat = new Material(Shader.Find("Gongbi/BreathCircle"));
        breathQuad.GetComponent<Renderer>().material = breathMat;
        breathQuad.GetComponent<Renderer>().enabled = false;

        var mpSO2 = new SerializedObject(mp);
        mpSO2.FindProperty("_breathCircleRenderer").objectReferenceValue = breathQuad.GetComponent<Renderer>();
        mpSO2.ApplyModifiedProperties();

        // Particles
        var particleObj = new GameObject("BreathParticles");
        particleObj.transform.SetParent(mpObj.transform);
        particleObj.transform.localPosition = Vector3.zero;
        particleObj.AddComponent<ParticleSystem>();
        var pb = particleObj.AddComponent<ParticleBreath>();

        var mpSO3 = new SerializedObject(mp);
        mpSO3.FindProperty("_particles").objectReferenceValue = pb;
        mpSO3.ApplyModifiedProperties();
    }

    static GameObject CreateXROrigin()
    {
        var xrObj = new GameObject("XROrigin");
        var xrOrigin = xrObj.AddComponent<XROrigin>();

        // Camera
        var camObj = new GameObject("MainCamera");
        camObj.transform.SetParent(xrObj.transform);
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1000f;
        var audioListener = camObj.AddComponent<AudioListener>();
        var tracker = camObj.AddComponent<TrackedPoseDriver>();

        // Camera offset
        var offsetObj = new GameObject("CameraOffset");
        offsetObj.transform.SetParent(xrObj.transform);
        offsetObj.transform.localPosition = new Vector3(0, 1.75f, 0);

        // Left controller (controller component configured via XRIT Starter Assets at runtime)
        // Right controller

        return xrObj;
    }
}
