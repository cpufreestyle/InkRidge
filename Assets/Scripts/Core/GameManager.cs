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
        [SerializeField] private int _startSceneIndex = 0;
        [SerializeField] private int _bambooSceneIndex = 1;
        [SerializeField] private int _waterfallSceneIndex = 2;
        [SerializeField] private int _pavilionSceneIndex = 3;
        [SerializeField] private int _summitSceneIndex = 4;
        [SerializeField] private int _endSceneIndex = 5;

        private float _sceneWalkTimer;
        private int _currentSceneIndex;

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

        void Update()
        {
            _sceneWalkTimer += Time.deltaTime;
        }

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
            _currentSceneIndex = _bambooSceneIndex;
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
