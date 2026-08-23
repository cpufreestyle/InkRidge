using UnityEngine;
using InkRidge.Data;
using InkRidge.Core;
using InkRidge.Environment;

namespace InkRidge.Meditation
{
    /// <summary>
    /// Placed in-scene at meditation trigger locations.
    /// When the player enters the trigger zone, shows a breath guide ring.
    /// After 3s of gaze confirmation, starts the breathing session.
    /// </summary>
    public class MeditationPoint : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private int _sceneIndex;
        [SerializeField] private string _sceneName = "Bamboo";
        [SerializeField] private string _themeText = "山不来就你，你便去就山";

        [Header("Breath Config")]
        [SerializeField] private BreathGuide.Pattern _pattern = BreathGuide.Pattern.Balanced444;
        [SerializeField] private float _sessionDuration = 180f;

        [Header("Visuals")]
        [SerializeField] private Transform _breathCircleTransform;
        [SerializeField] private Renderer _breathCircleRenderer;
        [SerializeField] private ParticleBreath _particles;

        [Header("Audio")]
        [SerializeField] private AudioSource _breathAudioSource;
        [SerializeField] private AudioSource _ambientAudioSource;

        private BreathGuide _breathGuide;
        private bool _playerInRange;
        private bool _meditationActive;
        private bool _meditationCompleted;
        private float _gazeConfirmTimer;
        private float _sessionTimer;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");

        void Start()
        {
            _breathGuide = new BreathGuide();
            if (_breathCircleRenderer != null)
                _breathCircleRenderer.enabled = false;
        }

        void Update()
        {
            if (_meditationActive)
            {
                _breathGuide.Update(Time.deltaTime);
                _sessionTimer += Time.deltaTime;

                UpdateBreathVisual();

                if (_sessionTimer >= _sessionDuration)
                {
                    CompleteMeditation();
                }
            }
            else if (_playerInRange && !_meditationCompleted)
            {
                _gazeConfirmTimer += Time.deltaTime;
                if (_gazeConfirmTimer >= 3f)
                {
                    StartMeditation();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !_meditationCompleted)
            {
                _playerInRange = true;
                _gazeConfirmTimer = 0f;
                if (_breathCircleRenderer != null)
                    _breathCircleRenderer.enabled = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                _gazeConfirmTimer = 0f;
                if (!_meditationActive && _breathCircleRenderer != null)
                    _breathCircleRenderer.enabled = false;
            }
        }

        private void StartMeditation()
        {
            _meditationActive = true;
            _sessionTimer = 0f;
            _breathGuide.Start(_pattern);

            if (_particles != null)
                _particles.StartBreathSync(_breathGuide);

            if (_ambientAudioSource != null)
                _ambientAudioSource.Play();
        }

        private void UpdateBreathVisual()
        {
            if (_breathCircleRenderer == null) return;

            float progress = 0f;
            switch (_breathGuide.CurrentPhase)
            {
                case BreathGuide.Phase.Inhale:
                    progress = _breathGuide.PhaseProgress;
                    break;
                case BreathGuide.Phase.HoldAfterInhale:
                    progress = 1f;
                    break;
                case BreathGuide.Phase.Exhale:
                    progress = 1f - _breathGuide.PhaseProgress;
                    break;
                case BreathGuide.Phase.HoldAfterExhale:
                    progress = 0f;
                    break;
            }

            var mat = _breathCircleRenderer.material;
            mat.SetFloat(ProgressId, progress);
        }

        private void CompleteMeditation()
        {
            _meditationActive = false;
            _meditationCompleted = true;
            _breathGuide.Stop();

            var record = new BreathData(
                _sceneIndex,
                _sceneName,
                _pattern.ToString(),
                _breathGuide.CompletedCycles,
                _breathGuide.GetTotalDuration(),
                _breathGuide.GetRhythmStability()
            );
            SaveManager.AddMeditationRecord(record);
            SaveManager.UnlockScene(_sceneIndex + 1);

            if (_breathCircleRenderer != null)
                _breathCircleRenderer.enabled = false;

            if (_particles != null)
                _particles.StopBreathSync();

            if (_ambientAudioSource != null)
                _ambientAudioSource.Stop();

            GameManager.Instance?.OnMeditationComplete(_sceneIndex);
        }

        public bool IsCompleted => _meditationCompleted;
    }
}
