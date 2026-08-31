using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Gradient fog with separate zenith/horizon colors for ink-painting atmosphere.
    /// Adapted from Google Daydream Elements SimpleCustomFog (Apache 2.0).
    /// Sets global shader parameters for custom fog shaders.
    /// </summary>
    [ExecuteInEditMode]
    public class GradientFog : MonoBehaviour
    {
        [System.Serializable]
        public struct FogSettings
        {
            public float start;
            public float end;
            public Color horizonColor;
            public Color horizonColorDistance;
            public Color zenithColor;

            public static FogSettings operator +(FogSettings a1, FogSettings a2)
            {
                FogSettings a = new FogSettings();
                a.start = a1.start + a2.start;
                a.end = a1.end + a2.end;
                a.horizonColor = a1.horizonColor + a2.horizonColor;
                a.horizonColorDistance = a1.horizonColorDistance + a2.horizonColorDistance;
                a.zenithColor = a1.zenithColor + a2.zenithColor;
                return a;
            }

            public static FogSettings operator *(float scaler, FogSettings a1)
            {
                FogSettings a = new FogSettings();
                a.start = scaler * a1.start;
                a.end = scaler * a1.end;
                a.horizonColor = scaler * a1.horizonColor;
                a.horizonColorDistance = scaler * a1.horizonColorDistance;
                a.zenithColor = scaler * a1.zenithColor;
                return a;
            }
        }

        [SerializeField] public FogSettings fog = new FogSettings
        {
            start = 5f,
            end = 60f,
            horizonColor = new Color(0.88f, 0.85f, 0.80f),
            horizonColorDistance = new Color(0.75f, 0.72f, 0.68f),
            zenithColor = new Color(0.65f, 0.70f, 0.75f)
        };

        private static readonly int FogDistanceId = Shader.PropertyToID("_FogDistance");
        private static readonly int FogZenithId = Shader.PropertyToID("_FogColorZenith");
        private static readonly int FogHorizonId = Shader.PropertyToID("_FogColorHorizon");
        private static readonly int FogHorizonDistId = Shader.PropertyToID("_FogColorHorizonDistance");

        /// <summary>
        /// These globals are constant for the lifetime of the scene — nothing
        /// tweaks them at runtime. They used to be pushed in LateUpdate, costing
        /// four global-shader writes per frame (which dirty the global constant
        /// buffer) to upload values that never changed. Push once instead.
        /// </summary>
        void OnEnable() => Apply();

        // Keeps the editor viewport live while the inspector is being tweaked.
        void OnValidate() => Apply();

        void OnDisable() => Clear();

        /// <summary>Push the current settings to the global shader state.</summary>
        public void Apply()
        {
            float span = Mathf.Max(fog.end - fog.start, 0.0001f);
            Shader.SetGlobalVector(FogDistanceId, new Vector4(1f / span, fog.start, 0f, 0f));
            Shader.SetGlobalVector(FogZenithId, fog.zenithColor);
            Shader.SetGlobalVector(FogHorizonId, fog.horizonColor);
            Shader.SetGlobalVector(FogHorizonDistId, fog.horizonColorDistance);
        }

        /// <summary>
        /// Reset to a neutral, invisible fog. Global shader values survive scene
        /// loads, so without this a scene with no GradientFog (e.g. 04_Summit)
        /// inherits the previous scene's fog colours.
        /// </summary>
        public static void Clear()
        {
            Shader.SetGlobalVector(FogDistanceId, new Vector4(0f, 0f, 0f, 0f));
            Shader.SetGlobalVector(FogZenithId, Color.white);
            Shader.SetGlobalVector(FogHorizonId, Color.white);
            Shader.SetGlobalVector(FogHorizonDistId, Color.white);
        }
    }
}
