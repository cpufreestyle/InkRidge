using UnityEngine;
using InkRidge.Data;
using InkRidge.Meditation;

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

        // Journey-scoped stats. SaveManager only ever keeps lifetime totals,
        // which made SummaryScreen print the same number under both "本次"
        // (this run) and "累计" (all time). These track the current playthrough
        // so the two rows can show different things.
        private float _journeyWalkTime;
        private float _journeyMeditationTime;
        private int _journeyBreathCycles;
        private float _journeyStabilitySum;
        private int _journeyStabilityCount;

        /// <summary>Walking time accumulated in this playthrough only.</summary>
        public float JourneyWalkTime => _journeyWalkTime;
        /// <summary>Meditation time accumulated in this playthrough only.</summary>
        public float JourneyMeditationTime => _journeyMeditationTime;
        public int JourneyBreathCycles => _journeyBreathCycles;
        /// <summary>Mean rhythm stability across this playthrough's sessions. 0 if none.</summary>
        public float JourneyAverageStability =>
            _journeyStabilityCount > 0 ? _journeyStabilitySum / _journeyStabilityCount : 0f;
        public int JourneySessionCount => _journeyStabilityCount;

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

        /// <summary>
        /// Records a finished (or abandoned) breathing session into this
        /// playthrough's totals and the persistent save.
        ///
        /// Called for both completed and early-exited sessions. SummaryScreen
        /// used to derive "this run" by taking the last four records, which
        /// silently broke once early exits started writing extra records.
        /// </summary>
        public void RecordMeditation(BreathData record)
        {
            if (record == null) return;

            _journeyMeditationTime += record.totalDurationSec;
            _journeyBreathCycles += record.completedCycles;
            _journeyStabilitySum += record.rhythmStability;
            _journeyStabilityCount++;

            SaveManager.AddMeditationRecord(record);
        }

        public void OnMeditationComplete(int sceneIndex)
        {
            SaveManager.AddWalkingTime(_sceneWalkTimer);
            _journeyWalkTime += _sceneWalkTimer;
            _sceneWalkTimer = 0f;

            int nextScene = sceneIndex + 1;
            if (nextScene > _summitSceneIndex)
                nextScene = _endSceneIndex;

            SceneTransition.Instance?.LoadScene(nextScene);
        }

        public void StartGame()
        {
            _sceneWalkTimer = 0f;
            _journeyWalkTime = 0f;
            _journeyMeditationTime = 0f;
            _journeyBreathCycles = 0;
            _journeyStabilitySum = 0f;
            _journeyStabilityCount = 0;
            SceneTransition.Instance?.LoadScene(_bambooSceneIndex);
        }

        public void SkipMeditation(int sceneIndex)
        {
            // The player still walked here even though they skipped the
            // breathing session, so the time counts in both totals. The
            // previous version dropped it from the journey entirely.
            SaveManager.AddWalkingTime(_sceneWalkTimer);
            _journeyWalkTime += _sceneWalkTimer;
            _sceneWalkTimer = 0f;

            int nextScene = sceneIndex + 1;
            if (nextScene > _summitSceneIndex)
                nextScene = _endSceneIndex;
            SceneTransition.Instance?.LoadScene(nextScene);
        }
    }
}
