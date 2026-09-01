using UnityEngine;
using InkRidge.Environment;

namespace InkRidge.Core
{
    /// <summary>
    /// Makes the scene "breathe" with the player. While a meditation session
    /// is running, inhale gathers the wind (vegetation leans in, fireflies
    /// brighten, fog thins) and exhale releases it — the landscape literally
    /// follows the breath cycle instead of just the HUD ring.
    ///
    /// Attach alongside a SceneBuilder in each meditation scene. Optional
    /// (BreathGuide, WindSystem, ParticleBreath, AmbientAudio) references are
    /// auto-resolved like MeditationPoint does.
    /// </summary>
    public class BreathSceneReactive : MonoBehaviour
    {
        [Header("References (auto-resolved if empty)")]
        [SerializeField] private Meditation.BreathGuide _guide;
        [SerializeField] private WindSystem _wind;

        [Header("Wind Response")]
        [SerializeField] private float _gatherIntensity = 1.35f;  // inhale peak
        [SerializeField] private float _releaseIntensity = 0.45f; // exhale floor
        [SerializeField] private float _responseSpeed = 3f;

        [Header("Light Response (fireflies / lamps / moon)")]
        [SerializeField, Tooltip("Optional lights that swell on inhale. Auto-collected at start.")]
        private Light[] _breathLights;
        [SerializeField] private float _lightDim = 0.55f;  // exhale
        [SerializeField] private float _lightGlow = 1.45f; // inhale

        [Header("Fog Response")]
        [SerializeField, Tooltip("Fog thins on inhale, thickens on exhale.")]
        private bool _affectFog = false;
        [SerializeField, Tooltip("Inhale density = baseline * this.")]
        private float _fogThinFactor = 0.6f;
        [SerializeField, Tooltip("Exhale density = baseline * this.")]
        private float _fogThickFactor = 1.5f;

        private float _fogBaseline;

        [Header("Emissive Renderer Response (firefly planes / glow objects)")]
        [SerializeField, Tooltip("Optional renderers whose materials brighten on inhale.")]
        private Renderer[] _glowRenderers;
        [SerializeField] private string _glowColorProp = "_MainColor";
        [SerializeField] private float _glowDim = 0.7f;
        [SerializeField] private float _glowGlow = 1.3f;

        private float _currentBlend; // 0 = released, 1 = gathered
        private float[] _lightBase;
        private Color[] _glowBase;

        void Start()
        {
            if (_wind == null) _wind = FindObjectOfType<WindSystem>();
            if (_breathLights == null || _breathLights.Length == 0)
                _breathLights = System.Array.Empty<Light>();
            _lightBase = new float[_breathLights.Length];
            for (int i = 0; i < _breathLights.Length; i++)
                _lightBase[i] = _breathLights[i] != null ? _breathLights[i].intensity : 1f;

            if (_glowRenderers != null)
            {
                _glowBase = new Color[_glowRenderers.Length];
                for (int i = 0; i < _glowRenderers.Length; i++)
                    _glowBase[i] = _glowRenderers[i] != null && _glowRenderers[i].material.HasProperty(_glowColorProp)
                        ? _glowRenderers[i].material.GetColor(_glowColorProp)
                        : Color.white;
            }

            if (_affectFog && !RenderSettings.fog)
                _affectFog = false; // scene doesn't use fog; respect that
        }

        /// <summary>
        /// MeditationPoint calls this when a session starts/stops so the
        /// effect only runs while the breath guide is actually live.
        /// </summary>
        public void SetBreathSource(Meditation.BreathGuide guide)
        {
            _guide = guide;
            if (guide != null && _fogBaseline <= 0f)
                _fogBaseline = RenderSettings.fogDensity; // capture once, includes DailyZen drift
            if (guide == null)
            {
                // Release the scene back to neutral.
                _currentBlend = 0f;
                if (_wind != null)
                    _wind.SetIntensity(Mathf.Lerp(_releaseIntensity, 1f, 0.5f));
                if (_affectFog && _fogBaseline > 0f)
                    RenderSettings.fogDensity = _fogBaseline;
                ApplyLights(1f);
                ApplyGlow(1f);
            }
        }

        void Update()
        {
            if (_guide == null) return;
            if (_guide.CurrentPhase == Meditation.BreathGuide.Phase.Idle) return;

            // Inhale → gather (wind swells), exhale → release (wind falls).
            float target = _guide.CurrentPhase == Meditation.BreathGuide.Phase.Inhale
                ? Mathf.Lerp(_currentBlend, 1f, _guide.PhaseProgress)
                : Mathf.Lerp(_currentBlend, 0f, _guide.PhaseProgress);

            _currentBlend = Mathf.Lerp(_currentBlend, target, _responseSpeed * Time.deltaTime);

            // Map the blend onto the wind intensity band the shader was
            // tuned for (SetIntensity already smooths toward its own target).
            if (_wind != null)
            {
                float intensity = Mathf.Lerp(_releaseIntensity, _gatherIntensity, _currentBlend);
                _wind.SetIntensity(intensity);
            }

            // World light swells with the breath — fireflies and lamps
            // brighten on inhale, soften on exhale.
            float lightScale = Mathf.Lerp(_lightDim, _lightGlow, _currentBlend);
            ApplyLights(lightScale);

            // Fog parts on inhale (mountain summit reads clearer) and settles
            // back in on exhale — an ink-wash inhale/exhale.
            if (_affectFog && _fogBaseline > 0f)
                RenderSettings.fogDensity = Mathf.Lerp(
                    _fogBaseline * _fogThickFactor,
                    _fogBaseline * _fogThinFactor,
                    _currentBlend);

            ApplyGlow(Mathf.Lerp(_glowDim, _glowGlow, _currentBlend));
        }

        private void ApplyLights(float scale)
        {
            if (_breathLights == null) return;
            for (int i = 0; i < _breathLights.Length; i++)
            {
                if (_breathLights[i] != null)
                    _breathLights[i].intensity = _lightBase[i] * scale;
            }
        }

        private void ApplyGlow(float scale)
        {
            if (_glowRenderers == null) return;
            for (int i = 0; i < _glowRenderers.Length; i++)
            {
                if (_glowRenderers[i] != null && _glowRenderers[i].material.HasProperty(_glowColorProp))
                    _glowRenderers[i].material.SetColor(_glowColorProp, _glowBase[i] * scale);
            }
        }
    }
}
