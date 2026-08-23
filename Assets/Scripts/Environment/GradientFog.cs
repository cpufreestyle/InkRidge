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

        void LateUpdate()
        {
            Shader.SetGlobalVector("_FogDistance",
                new Vector4(1f / (fog.end - fog.start), fog.start, 0, 0));
            Shader.SetGlobalVector("_FogColorZenith", fog.zenithColor);
            Shader.SetGlobalVector("_FogColorHorizon", fog.horizonColor);
            Shader.SetGlobalVector("_FogColorHorizonDistance", fog.horizonColorDistance);
        }
    }
}
