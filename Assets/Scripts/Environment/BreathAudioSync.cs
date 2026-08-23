using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Environment
{
    /// <summary>
    /// Plays breath inhale/exhale SFX synchronized with BreathGuide phases.
    /// Attach to the same GameObject as MeditationPoint or nearby.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BreathAudioSync : MonoBehaviour
    {
        [SerializeField] private AudioClip _inhaleClip;
        [SerializeField] private AudioClip _exhaleClip;
        [SerializeField] private float _volume = 0.5f;

        private AudioSource _source;
        private BreathGuide.Phase _lastPhase = BreathGuide.Phase.Idle;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.spatialBlend = 0f; // 2D
            _source.volume = _volume;
        }

        public void OnPhaseChanged(BreathGuide guide)
        {
            if (guide.CurrentPhase == BreathGuide.Phase.Inhale && _lastPhase != BreathGuide.Phase.Inhale)
            {
                if (_inhaleClip != null) _source.PlayOneShot(_inhaleClip);
            }
            else if (guide.CurrentPhase == BreathGuide.Phase.Exhale && _lastPhase != BreathGuide.Phase.Exhale)
            {
                if (_exhaleClip != null) _source.PlayOneShot(_exhaleClip);
            }
            _lastPhase = guide.CurrentPhase;
        }
    }
}
