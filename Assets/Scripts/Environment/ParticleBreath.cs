using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Environment
{
    /// <summary>
    /// Syncs a ParticleSystem emission rate to the breath cycle.
    /// Particles drift outward during inhale, inward during exhale.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleBreath : MonoBehaviour
    {
        private ParticleSystem _ps;
        private ParticleSystem.EmissionModule _emission;
        private ParticleSystem.VelocityOverLifetimeModule _velocity;
        private BreathGuide _guide;

        void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            _emission = _ps.emission;
            _velocity = _ps.velocityOverLifetime;
            _emission.enabled = false;
            _velocity.enabled = true;
        }

        public void StartBreathSync(BreathGuide guide)
        {
            _guide = guide;
            _emission.enabled = true;
            _ps.Play();
        }

        public void StopBreathSync()
        {
            _guide = null;
            _emission.enabled = false;
            _ps.Stop();
        }

        void Update()
        {
            if (_guide == null) return;

            float speed = 0f;
            switch (_guide.CurrentPhase)
            {
                case BreathGuide.Phase.Inhale:
                    speed = 0.3f * _guide.PhaseProgress;
                    break;
                case BreathGuide.Phase.Exhale:
                    speed = -0.3f * (1f - _guide.PhaseProgress);
                    break;
                default:
                    speed = 0f;
                    break;
            }

            _velocity.y = speed;
            _emission.rateOverTime = Mathf.Lerp(2f, 15f, _guide.PhaseProgress);
        }
    }
}
