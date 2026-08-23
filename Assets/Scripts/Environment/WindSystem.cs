using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Global wind system that drives shader parameters for vegetation swaying.
    /// Adapted from Google Daydream Elements GlobalDynamicWindColorSimple (Apache 2.0).
    /// Sets global shader floats (_WindSpeed, _WindMagnitude, etc.) that vegetation
    /// shaders can read to animate vertex positions and colors.
    /// </summary>
    public class WindSystem : MonoBehaviour
    {
        [Header("Wind Properties")]
        [SerializeField] private float _windSpeed = 1.5f;
        [SerializeField] private float _windMagnitude = 0.15f;
        [SerializeField] private float _windTurbulence = 1.5f;
        [SerializeField] private Vector2 _windDirection = new Vector2(1f, 0.3f);
        [SerializeField] private float _gustDensity = 0.6f;

        [Header("Intensity")]
        [SerializeField] private float _targetIntensity = 1f;
        [SerializeField] private float _transitionSpeed = 2f;

        private float _currentIntensity;

        // Shader property IDs
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");
        private static readonly int WindMagnitudeId = Shader.PropertyToID("_WindMagnitude");
        private static readonly int WindTurbulenceId = Shader.PropertyToID("_WindTurbulence");
        private static readonly int WindDirXId = Shader.PropertyToID("_WindDirectionX");
        private static readonly int WindDirZId = Shader.PropertyToID("_WindDirectionZ");
        private static readonly int GustDensityId = Shader.PropertyToID("_GustDensity");
        private static readonly int WindIntensityId = Shader.PropertyToID("_WindIntensity");

        void Start()
        {
            _currentIntensity = 0f;
        }

        void Update()
        {
            // Smooth transition to target intensity
            _currentIntensity = Mathf.Lerp(_currentIntensity, _targetIntensity,
                _transitionSpeed * Time.deltaTime);

            // Animate wind direction slowly
            float t = Time.time * 0.1f;
            Vector2 dir = _windDirection + new Vector2(Mathf.Sin(t) * 0.2f, Mathf.Cos(t * 0.7f) * 0.2f);
            dir.Normalize();

            Shader.SetGlobalFloat(WindSpeedId, _windSpeed);
            Shader.SetGlobalFloat(WindMagnitudeId, _windMagnitude * _currentIntensity);
            Shader.SetGlobalFloat(WindTurbulenceId, _windTurbulence);
            Shader.SetGlobalFloat(WindDirXId, dir.x);
            Shader.SetGlobalFloat(WindDirZId, dir.y);
            Shader.SetGlobalFloat(GustDensityId, _gustDensity);
            Shader.SetGlobalFloat(WindIntensityId, _currentIntensity);
        }

        /// <summary>Smoothly set wind intensity (0 = calm, 1 = full wind).</summary>
        public void SetIntensity(float intensity)
        {
            _targetIntensity = Mathf.Clamp01(intensity);
        }
    }
}
