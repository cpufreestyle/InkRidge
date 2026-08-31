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

        /// <summary>
        /// True while the effect still has something to draw — either the player
        /// is moving, or the fade-out hasn't finished.
        ///
        /// OnRenderImage costs a full-screen blit per eye even when it just
        /// passes the image through, because the destination target still has to
        /// be written. The driver turns this component off entirely whenever
        /// this returns false, so the whole pass disappears from the frame.
        /// </summary>
        public bool WantsRender =>
            ComfortSettings.VignetteEnabled &&
            (_movementActive || _currentIntensity > 0.001f);

        void Awake()
        {
            var shader = Shader.Find("Hidden/Vignette");
            if (shader == null)
            {
                Debug.LogWarning("[VignetteEffect] Vignette shader not found, disabling.");
                enabled = false;
                return;
            }
            _vignetteMat = new Material(shader);
            _currentIntensity = 0f;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            _currentIntensity = Mathf.Lerp(_currentIntensity,
                _movementActive ? _maxIntensity : 0f,
                _fadeSpeed * Time.deltaTime);

            _vignetteMat.SetFloat(IntensityId, _currentIntensity);
            Graphics.Blit(src, dst, _vignetteMat);
        }

        public void SetMovementActive(bool active)
        {
            _movementActive = active;
        }
    }
}
