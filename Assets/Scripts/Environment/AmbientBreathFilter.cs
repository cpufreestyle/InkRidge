using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Environment
{
    /// <summary>
    /// Ambient "breathing filter": while a meditation session runs, a low-pass
    /// filter on the ambience opens on inhale and closes on exhale. The world
    /// audibly swells and settles with each breath — a second, purely
    /// spatial-audio breath guide beside the visual ring.
    ///
    /// Attach to the same GameObject as AmbientAudio (auto-found). AudioSource
    /// components here get an AudioLowPassFilter added at runtime if missing.
    /// </summary>
    [RequireComponent(typeof(AmbientAudio))]
    public class AmbientBreathFilter : MonoBehaviour
    {
        [Header("Filter Range (Hz)")]
        [SerializeField] private float _openCutoff = 12000f;  // inhale: world wide open
        [SerializeField] private float _closedCutoff = 1800f; // exhale: muffled settle

        [Header("Feel")]
        [SerializeField] private float _responseSpeed = 2.5f;

        private BreathGuide _guide;
        private AudioLowPassFilter[] _filters;
        private float _current; // 0 closed .. 1 open

        /// <summary>MeditationPoint binds/unbinds on session start/end.</summary>
        public void Bind(BreathGuide guide)
        {
            _guide = guide;
        }

        public void Unbind()
        {
            _guide = null;
            // Let the world open back up when the session ends.
            ApplyCutoff(Mathf.Lerp(_current, 1f, 1f));
        }

        void Awake()
        {
            var sources = GetComponentsInChildren<AudioSource>(true);
            var list = new System.Collections.Generic.List<AudioLowPassFilter>();
            foreach (var src in sources)
            {
                if (src == null) continue;
                var f = src.GetComponent<AudioLowPassFilter>();
                if (f == null) f = src.gameObject.AddComponent<AudioLowPassFilter>();
                f.cutoffFrequency = _openCutoff;
                list.Add(f);
            }
            _filters = list.ToArray();
        }

        void Update()
        {
            if (_filters == null || _filters.Length == 0) return;

            float target;
            if (_guide != null && _guide.CurrentPhase != BreathGuide.Phase.Idle)
            {
                // Follow the same curve the visual ring uses.
                bool inhale = _guide.CurrentPhase == BreathGuide.Phase.Inhale;
                target = inhale
                    ? Mathf.Lerp(_current, 1f, _guide.PhaseProgress)
                    : Mathf.Lerp(_current, 0f, _guide.PhaseProgress);
            }
            else
            {
                target = 1f; // neutral: fully open
            }

            _current = Mathf.Lerp(_current, target, _responseSpeed * Time.deltaTime);
            ApplyCutoff(_current);
        }

        private void ApplyCutoff(float open01)
        {
            float hz = Mathf.Lerp(_closedCutoff, _openCutoff, Mathf.Clamp01(open01));
            foreach (var f in _filters)
            {
                if (f != null) f.cutoffFrequency = hz;
            }
        }
    }
}
