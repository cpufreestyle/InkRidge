using UnityEngine;
using UnityEngine.Audio;

namespace InkRidge.Core
{
    /// <summary>
    /// Central audio mixer controller. Adjusts ambient volume during meditation.
    /// Also provides master/SFX/music volume controls for future settings menu.
    /// </summary>
    public class AudioMixerController : MonoBehaviour
    {
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _sfxVolume = 0.8f;
        [SerializeField] private float _ambientVolume = 0.6f;
        [SerializeField] private float _meditationAmbientDip = 0.3f; // ambient volume dips during meditation

        private float _currentAmbientVolume;
        private bool _inMeditation;

        /// <summary>Currently faded ambient level (dips during meditation).</summary>
        public float AmbientVolume => _currentAmbientVolume;

        /// <summary>Current SFX level.</summary>
        public float SfxVolume => _sfxVolume;

        void Start()
        {
            _currentAmbientVolume = _ambientVolume;
            AudioListener.volume = _masterVolume;
        }

        void Update()
        {
            float target = _inMeditation ? _ambientVolume * _meditationAmbientDip : _ambientVolume;

            // The old loop lerped every frame forever and re-assigned a constant
            // AudioListener.volume, even though nothing ever read the result.
            // Bail out once the fade has settled so an idle scene costs nothing.
            if (Mathf.Approximately(_currentAmbientVolume, target))
                return;

            _currentAmbientVolume = Mathf.Lerp(_currentAmbientVolume, target, 2f * Time.deltaTime);
        }

        public void SetMeditationMode(bool active)
        {
            _inMeditation = active;
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = _masterVolume;
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbientVolume(float volume)
        {
            _ambientVolume = Mathf.Clamp01(volume);
        }
    }
}
