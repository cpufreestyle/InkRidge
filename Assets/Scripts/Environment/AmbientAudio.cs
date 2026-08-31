using System.Collections;
using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Manages 3D ambient audio sources per scene.
    /// Fades in on scene load, fades out on scene exit, and can be ducked
    /// while a meditation session is running.
    /// </summary>
    public class AmbientAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource[] _ambientSources;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private float _targetVolume = 0.6f;

        private Coroutine _fade;
        private float _currentVolume;

        void Start()
        {
            _currentVolume = 0f;
            ApplyVolume();
            foreach (var source in _ambientSources)
            {
                if (source != null) source.Play();
            }
            FadeTo(_targetVolume, _fadeDuration);
        }

        /// <summary>
        /// Pull the ambience down while something more important is playing.
        /// MeditationPoint calls this so the breath guide is audible.
        /// </summary>
        public void Duck(float duckedVolume, float duration) => FadeTo(duckedVolume, duration);

        /// <summary>Back to normal scene ambience level.</summary>
        public void Restore() => FadeTo(_targetVolume, _fadeDuration);

        public void FadeOutAndStop()
        {
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }
            StartCoroutine(FadeOut());
        }

        private void FadeTo(float target, float duration)
        {
            // Without cancelling, a duck and a restore started close together
            // would fight over source.volume every frame.
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }
            _fade = StartCoroutine(FadeRoutine(target, duration));
        }

        private IEnumerator FadeRoutine(float target, float duration)
        {
            float start = _currentVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentVolume = Mathf.Lerp(start, target, elapsed / duration);
                ApplyVolume();
                yield return null;
            }

            _currentVolume = target;
            ApplyVolume();
            _fade = null;
        }

        private IEnumerator FadeOut()
        {
            float start = _currentVolume;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _currentVolume = Mathf.Lerp(start, 0f, elapsed / _fadeDuration);
                ApplyVolume();
                yield return null;
            }

            _currentVolume = 0f;
            ApplyVolume();

            foreach (var source in _ambientSources)
            {
                if (source != null) source.Stop();
            }
        }

        private void ApplyVolume()
        {
            if (_ambientSources == null) return;
            foreach (var source in _ambientSources)
            {
                if (source != null) source.volume = _currentVolume;
            }
        }
    }
}
