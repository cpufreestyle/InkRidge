using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Environment
{
    /// <summary>
    /// Fades the environment lighting based on meditation phase.
    /// During inhale: slightly brighter, warmer.
    /// During exhale: slightly darker, cooler.
    /// Creates a subtle breathing atmosphere.
    /// </summary>
    public class MeditationLightPulse : MonoBehaviour
    {
        [SerializeField] private Light _directionalLight;
        [SerializeField] private float _intensityRange = 0.15f;
        [SerializeField] private float _colorShift = 0.05f;

        private float _baseIntensity;
        private Color _baseColor;
        private BreathGuide _guide;

        void Start()
        {
            if (_directionalLight == null)
                _directionalLight = FindSceneSun();

            if (_directionalLight != null)
            {
                _baseIntensity = _directionalLight.intensity;
                _baseColor = _directionalLight.color;
            }
        }

        /// <summary>
        /// Locate the scene's key light. FindObjectOfType&lt;Light&gt;() returns
        /// the first light of any type, which is fine today (the sun is the only
        /// light) but silently grabs a lamp the moment one is added. Prefer the
        /// sun, then fall back to the first directional light.
        /// </summary>
        private static Light FindSceneSun()
        {
            if (RenderSettings.sun != null)
                return RenderSettings.sun;

            foreach (var candidate in FindObjectsOfType<Light>())
            {
                if (candidate.type == LightType.Directional)
                    return candidate;
            }
            return null;
        }

        public void SetBreathGuide(BreathGuide guide)
        {
            _guide = guide;
        }

        void Update()
        {
            if (_guide == null || _directionalLight == null || _guide.CurrentPhase == BreathGuide.Phase.Idle)
                return;

            float progress = _guide.PhaseProgress;
            float intensityOffset = 0f;
            Color colorOffset = Color.clear;

            switch (_guide.CurrentPhase)
            {
                case BreathGuide.Phase.Inhale:
                    // Brighten and warm
                    intensityOffset = progress * _intensityRange;
                    colorOffset = new Color(progress * _colorShift, 0, -progress * _colorShift * 0.5f);
                    break;
                case BreathGuide.Phase.Exhale:
                    // Darken and cool
                    intensityOffset = -progress * _intensityRange;
                    colorOffset = new Color(-progress * _colorShift * 0.5f, 0, progress * _colorShift);
                    break;
            }

            _directionalLight.intensity = _baseIntensity + intensityOffset;
            _directionalLight.color = _baseColor + colorOffset;
        }
    }
}
