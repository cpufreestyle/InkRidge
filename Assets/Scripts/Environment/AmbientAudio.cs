using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Manages 3D ambient audio sources per scene.
    /// Fades in on scene load, fades out on scene exit.
    /// </summary>
    public class AmbientAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource[] _ambientSources;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private float _targetVolume = 0.6f;

        void Start()
        {
            foreach (var source in _ambientSources)
            {
                source.volume = 0f;
                source.Play();
            }
            StartCoroutine(FadeIn());
        }

        System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                foreach (var source in _ambientSources)
                    source.volume = Mathf.Lerp(0f, _targetVolume, t);
                yield return null;
            }
        }

        public void FadeOutAndStop()
        {
            StartCoroutine(FadeOut());
        }

        System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float[] startVolumes = new float[_ambientSources.Length];
            for (int i = 0; i < _ambientSources.Length; i++)
                startVolumes[i] = _ambientSources[i].volume;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                for (int i = 0; i < _ambientSources.Length; i++)
                    _ambientSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
                yield return null;
            }

            foreach (var source in _ambientSources)
                source.Stop();
        }
    }
}
