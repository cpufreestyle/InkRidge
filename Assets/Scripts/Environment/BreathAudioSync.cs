using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Environment
{
    /// <summary>
    /// Plays breath inhale/exhale SFX synchronized with BreathGuide phases.
    /// Attach to the same GameObject as MeditationPoint or nearby.
    ///
    /// Subscribes to BreathGuide.PhaseChanged rather than being polled. The
    /// previous version exposed a public OnPhaseChanged() that nothing in the
    /// project ever called, so breath audio never played — and the component
    /// was never placed in any scene either. Both are fixed by WireMeditation.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BreathAudioSync : MonoBehaviour
    {
        [SerializeField] private AudioClip _inhaleClip;
        [SerializeField] private AudioClip _exhaleClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.5f;

        private AudioSource _source;
        private BreathGuide _guide;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.spatialBlend = 0f;  // 2D — breath cues should not pan
            _source.volume = _volume;
            _source.loop = false;
        }

        public void Bind(BreathGuide guide)
        {
            Unbind();
            _guide = guide;
            if (_guide != null)
                _guide.PhaseChanged += OnPhaseChanged;
        }

        public void Unbind()
        {
            if (_guide != null)
                _guide.PhaseChanged -= OnPhaseChanged;
            _guide = null;
        }

        void OnDestroy() => Unbind();

        /// <summary>Editor-facing: are both clips assigned?</summary>
        public bool IsConfigured => _inhaleClip != null && _exhaleClip != null;

        private void OnPhaseChanged(BreathGuide.Phase phase)
        {
            switch (phase)
            {
                case BreathGuide.Phase.Inhale:
                    Play(_inhaleClip);
                    break;
                case BreathGuide.Phase.Exhale:
                    Play(_exhaleClip);
                    break;
                // Hold phases stay silent — the point is the breath, not a
                // metronome. Haptics cover the holds.
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || _source == null) return;
            _source.PlayOneShot(clip, _volume);
        }
    }
}
