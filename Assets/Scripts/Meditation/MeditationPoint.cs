using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using InkRidge.Data;
using InkRidge.Core;
using InkRidge.Environment;

namespace InkRidge.Meditation
{
    /// <summary>
    /// Placed in-scene at meditation trigger locations.
    /// When the player enters the trigger zone, shows a breath guide ring.
    /// After 3s of gaze confirmation, starts the breathing session.
    ///
    /// Sessions can be ended early by holding a controller menu button —
    /// a VR experience that locks you in for three minutes with no way out
    /// is a comfort problem, not a feature.
    /// </summary>
    public class MeditationPoint : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private int _sceneIndex;
        [SerializeField] private string _sceneName = "Bamboo";

        [Header("Breath Config")]
        [SerializeField] private BreathGuide.Pattern _pattern = BreathGuide.Pattern.Balanced444;
        [SerializeField] private float _sessionDuration = 180f;

        [Header("Gaze Confirm")]
        [SerializeField] private float _gazeHoldSeconds = 3f;

        [Header("Early Exit")]
        [SerializeField] private float _exitHoldSeconds = 1.5f;
        [SerializeField] private float _exitFeedbackInterval = 0.25f;

        [Header("Visuals")]
        [SerializeField] private Renderer _breathCircleRenderer;

        [Header("Feedback (optional, wired by WireMeditation)")]
        [SerializeField] private ParticleBreath _particles;
        [SerializeField] private BreathHaptics _haptics;
        [SerializeField] private BreathAudioSync _breathAudio;
        [SerializeField] private InkRidge.Core.BreathSceneReactive _sceneReactive;
        [SerializeField] private Environment.AmbientBreathFilter _breathFilter;
        [SerializeField] private AudioSource _ambientAudioSource;

        [Header("Ambience Ducking")]
        [SerializeField] private float _meditationAmbientVolume = 0.18f;
        [SerializeField] private float _duckFadeSeconds = 1.5f;

        private BreathGuide _breathGuide;
        private AmbientAudio _ambientAudio;
        private bool _playerInRange;
        private bool _meditationActive;
        private bool _meditationCompleted;
        private bool _awaitingReentry;
        private float _gazeConfirmTimer;
        private float _sessionTimer;
        private float _exitHoldTimer;
        private float _exitFeedbackTimer;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");

        private Material _breathCircleMat;

        void Start()
        {
            _breathGuide = new BreathGuide();

            // Prefer the scene's AmbientAudio (present in all four scenes).
            // _ambientAudioSource is a legacy fallback for scenes wired before
            // AmbientAudio existed.
            _ambientAudio = FindObjectOfType<AmbientAudio>();
            if (_ambientAudio == null && _ambientAudioSource == null)
                Debug.LogWarning(
                    $"[{nameof(MeditationPoint)}] No AmbientAudio in scene and no fallback " +
                    "source assigned — ambience will not duck during meditation.", this);

            if (_haptics == null) _haptics = GetComponent<BreathHaptics>();
            if (_breathAudio == null) _breathAudio = GetComponent<BreathAudioSync>();
            if (_sceneReactive == null) _sceneReactive = FindObjectOfType<InkRidge.Core.BreathSceneReactive>();
            if (_breathFilter == null)
            {
                var ambient = _ambientAudio != null ? _ambientAudio.gameObject
                    : (_ambientAudioSource != null ? _ambientAudioSource.gameObject : null);
                _breathFilter = ambient != null
                    ? ambient.AddComponent<Environment.AmbientBreathFilter>()
                    : FindObjectOfType<Environment.AmbientBreathFilter>();
            }

            if (_breathCircleRenderer != null)
            {
                _breathCircleRenderer.enabled = false;
                // Cache the instance once. Reading Renderer.material every frame
                // re-runs the "has this been instanced yet" lookup and hands back
                // a fresh wrapper object each time.
                _breathCircleMat = _breathCircleRenderer.material;
            }
        }

        void Update()
        {
            if (_meditationActive)
            {
                UpdateMeditation();
            }
            else if (_playerInRange && !_meditationCompleted && !_awaitingReentry)
            {
                UpdateGazeConfirm();
            }
        }

        private void UpdateMeditation()
        {
            _breathGuide.Update(Time.deltaTime);
            _sessionTimer += Time.deltaTime;

            UpdateBreathVisual();

            if (_sessionTimer >= _sessionDuration)
            {
                CompleteMeditation();
                return;
            }

            UpdateExitHold();
        }

        private void UpdateGazeConfirm()
        {
            _gazeConfirmTimer = Mathf.Min(_gazeConfirmTimer + Time.deltaTime, _gazeHoldSeconds);

            // The ring grows as the gaze holds. Without this the player has no
            // idea a 3-second confirm is running — it just looks like the ring
            // appeared and then nothing happened. StartGate already had this
            // feedback; MeditationPoint did not.
            SetRingProgress(_gazeConfirmTimer / _gazeHoldSeconds);

            if (_gazeConfirmTimer >= _gazeHoldSeconds)
                StartMeditation();
        }

        private void UpdateExitHold()
        {
            if (!IsExitHeld())
            {
                _exitHoldTimer = 0f;
                _exitFeedbackTimer = 0f;
                return;
            }

            _exitHoldTimer += Time.deltaTime;
            _exitFeedbackTimer += Time.deltaTime;

            if (_exitFeedbackTimer >= _exitFeedbackInterval)
            {
                _exitFeedbackTimer = 0f;
                // Ramp the pulse up as the hold completes, so the player can
                // feel the exit charging without looking at anything.
                float charge = Mathf.Clamp01(_exitHoldTimer / _exitHoldSeconds);
                _haptics?.Pulse(0.15f + 0.35f * charge, 0.06f);
            }

            if (_exitHoldTimer >= _exitHoldSeconds)
                CancelMeditation();
        }

        private static bool IsExitHeld()
        {
            // Input System only (activeInputHandler=2). Legacy Input.GetKeyDown
            // never fires in this project — see XRMenu for the same trap.
            return Held(XRController.leftHand) || Held(XRController.rightHand);
        }

        private static bool Held(XRController controller)
        {
            if (controller == null) return false;
            // XRController (Input System) exposes no typed button properties —
            // Quest controllers surface buttons as child controls named after
            // the XR usage. Query dynamically.
            var menu = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("menuButton");
            if (menu != null && menu.isPressed) return true;
            var secondary = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("secondaryButton");
            return secondary != null && secondary.isPressed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || _meditationCompleted || _awaitingReentry) return;

            _playerInRange = true;
            _gazeConfirmTimer = 0f;
            if (_breathCircleRenderer != null)
                _breathCircleRenderer.enabled = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;
            _gazeConfirmTimer = 0f;
            _exitHoldTimer = 0f;
            // After an early exit the player is still standing in the trigger.
            // Without this they would immediately restart meditation the
            // moment they released the button.
            _awaitingReentry = false;

            if (!_meditationActive && _breathCircleRenderer != null)
                _breathCircleRenderer.enabled = false;
        }

        private void StartMeditation()
        {
            _meditationActive = true;
            _sessionTimer = 0f;
            _exitHoldTimer = 0f;
            _breathGuide.Start(_pattern);

            _haptics?.Bind(_breathGuide);
            _breathAudio?.Bind(_breathGuide);
            _sceneReactive?.SetBreathSource(_breathGuide);
            _breathFilter?.Bind(_breathGuide);

            if (_particles != null)
                _particles.StartBreathSync(_breathGuide);

            if (_ambientAudio != null)
                _ambientAudio.Duck(_meditationAmbientVolume, _duckFadeSeconds);
            else if (_ambientAudioSource != null)
                _ambientAudioSource.volume = _meditationAmbientVolume;
        }

        private void UpdateBreathVisual()
        {
            if (_breathCircleMat == null) return;

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

            SetRingProgress(progress);
        }

        private void SetRingProgress(float progress01)
        {
            if (_breathCircleMat == null) return;
            _breathCircleMat.SetFloat(ProgressId, Mathf.Clamp01(progress01));
        }

        private void CompleteMeditation()
        {
            EndSession(completedEarly: false);

            var record = new BreathData(
                _sceneIndex,
                _sceneName,
                _pattern.ToString(),
                _breathGuide.CompletedCycles,
                _breathGuide.GetTotalDuration(),
                _breathGuide.GetRhythmStability()
            );
            GameManager.Instance?.RecordMeditation(record);
            SaveManager.UnlockScene(_sceneIndex + 1);

            GameManager.Instance?.OnMeditationComplete(_sceneIndex);
        }

        /// <summary>Player ended early. Keep the record, do not unlock the next scene.</summary>
        private void CancelMeditation()
        {
            // A session with zero completed cycles is noise, not data.
            if (_breathGuide.CompletedCycles > 0)
            {
                var record = new BreathData(
                    _sceneIndex,
                    _sceneName,
                    _pattern.ToString(),
                    _breathGuide.CompletedCycles,
                    _breathGuide.GetTotalDuration(),
                    _breathGuide.GetRhythmStability(),
                    completedEarly: true
                );
                GameManager.Instance?.RecordMeditation(record);
            }

            _haptics?.Pulse(0.6f, 0.15f);
            EndSession(completedEarly: true);
        }

        private void EndSession(bool completedEarly)
        {
            _meditationActive = false;
            _exitHoldTimer = 0f;
            _breathGuide.Stop();

            _haptics?.Unbind();
            _breathAudio?.Unbind();
            _sceneReactive?.SetBreathSource(null);
            _breathFilter?.Unbind();

            if (_particles != null)
                _particles.StopBreathSync();

            if (_ambientAudio != null)
                _ambientAudio.Restore();
            else if (_ambientAudioSource != null)
                _ambientAudioSource.volume = 1f;

            if (_breathCircleRenderer != null)
                _breathCircleRenderer.enabled = false;

            if (completedEarly)
            {
                // Stays false, so the point can be re-entered — but only after
                // the player physically leaves the trigger.
                _meditationCompleted = false;
                _awaitingReentry = true;
                _playerInRange = false;
            }
            else
            {
                _meditationCompleted = true;
            }

            _gazeConfirmTimer = 0f;
            SetRingProgress(0f);
        }

        public bool IsCompleted => _meditationCompleted;
    }
}
