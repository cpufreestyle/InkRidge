using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InkRidge.Core
{
    /// <summary>
    /// Async scene loading with black fade transition.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        [SerializeField] private CanvasGroup _fadeCanvas;
        [SerializeField] private float _fadeDuration = 1.5f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            yield return Fade(0f, 1f);

            var op = SceneManager.LoadSceneAsync(sceneIndex);
            while (!op.isDone)
                yield return null;

            yield return Fade(1f, 0f);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_fadeCanvas == null) yield break;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }
            _fadeCanvas.alpha = to;
        }
    }
}
