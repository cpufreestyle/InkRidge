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

        void Start()
        {
            _currentAmbientVolume = _ambientVolume;
        }

        void Update()
        {
            float target = _inMeditation ? _ambientVolume * _meditationAmbientDip : _ambientVolume;
            _currentAmbientVolume = Mathf.Lerp(_currentAmbientVolume, target, 2f * Time.deltaTime);

            AudioListener.volume = _masterVolume;
        }

        public void SetMeditationMode(bool active)
        {
            _inMeditation = active;
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
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
