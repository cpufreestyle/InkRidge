using UnityEngine;

namespace InkRidge.Core
{
    /// <summary>
    /// Simple vignette effect for VR comfort (darkens screen edges during movement).
    /// Attach to Main Camera. Toggled by ComfortSettings.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class VignetteEffect : MonoBehaviour
    {
        [SerializeField] private float _maxIntensity = 0.4f;
        [SerializeField] private float _fadeSpeed = 3f;

        private Material _vignetteMat;
        private float _currentIntensity;
        private bool _movementActive;

        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        void Awake()
        {
            _vignetteMat = new Material(Shader.Find("Hidden/Vignette"));
            if (_vignetteMat == null)
            {
                Debug.LogWarning("[VignetteEffect] Vignette shader not found, disabling.");
                enabled = false;
                return;
            }
            _currentIntensity = 0f;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (_vignetteMat == null || !ComfortSettings.VignetteEnabled)
            {
                Graphics.Blit(src, dst);
                return;
            }

            _currentIntensity = Mathf.Lerp(_currentIntensity,
                _movementActive ? _maxIntensity : 0f,
                _fadeSpeed * Time.deltaTime);

            if (_currentIntensity < 0.01f)
            {
                Graphics.Blit(src, dst);
                return;
            }

            _vignetteMat.SetFloat(IntensityId, _currentIntensity);
            Graphics.Blit(src, dst, _vignetteMat);
        }

        public void SetMovementActive(bool active)
        {
            _movementActive = active;
        }
    }
}
