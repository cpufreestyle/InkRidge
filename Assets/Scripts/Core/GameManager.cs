using UnityEngine;
using InkRidge.Data;

namespace InkRidge.Core
{
    /// <summary>
    /// Global game state singleton. Persists across scenes.
    /// Manages scene flow, walking time tracking, and meditation completion.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Indices (Build Settings order)")]
        [SerializeField] private int _bambooSceneIndex = 1;
        [SerializeField] private int _summitSceneIndex = 4;
        [SerializeField] private int _endSceneIndex = 5;

        private float _sceneWalkTimer;

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

        [Header("Boot")]
        [SerializeField, Tooltip("When launched in the Start scene, auto-enter the game after this many seconds (set 0 to disable).")]
        private float _autoStartDelay = 2f;

        private float _bootTimer;

        void Update()
        {
            _sceneWalkTimer += Time.deltaTime;

            // Skip the start-scene wait entirely: users launch straight into
            // the meditation flow. Only fires in the Start scene (index 0).
            if (_autoStartDelay > 0f && !_booted)
            {
                _bootTimer += Time.deltaTime;
                if (_bootTimer >= _autoStartDelay)
                {
                    _booted = true;
                    StartGame();
                }
            }
        }

        private bool _booted;

        public void OnMeditationComplete(int sceneIndex)
        {
            SaveManager.AddWalkingTime(_sceneWalkTimer);
            _sceneWalkTimer = 0f;

            int nextScene = sceneIndex + 1;
            if (nextScene > _summitSceneIndex)
                nextScene = _endSceneIndex;

            SceneTransition.Instance?.LoadScene(nextScene);
        }

        public void StartGame()
        {
            _sceneWalkTimer = 0f;
            SceneTransition.Instance?.LoadScene(_bambooSceneIndex);
        }

        public void SkipMeditation(int sceneIndex)
        {
            int nextScene = sceneIndex + 1;
            if (nextScene > _summitSceneIndex)
                nextScene = _endSceneIndex;
            SceneTransition.Instance?.LoadScene(nextScene);
        }
    }
}
