using UnityEngine;
using UnityEngine.XR;
using InkRidge.Core;

namespace InkRidge.Meditation
{
    /// <summary>
    /// Fires a short controller pulse on every breath phase change, so the
    /// player can follow the rhythm with their eyes closed instead of staring
    /// at the breath ring.
    ///
    /// Before this existed the project had zero haptic feedback anywhere —
    /// `grep Haptic|Impulse|vibrat` across Assets/Scripts returned nothing.
    /// For a guided-breathing app that is the cheapest channel to add: it
    /// turns "watch the guide" into "feel the guide".
    ///
    /// Attach to the same GameObject as MeditationPoint and call Bind().
    /// </summary>
    public class BreathHaptics : MonoBehaviour
    {
        [Header("Pulse Strength (0-1)")]
        [SerializeField, Range(0f, 1f)] private float _inhaleAmplitude = 0.30f;
        [SerializeField, Range(0f, 1f)] private float _exhaleAmplitude = 0.22f;
        [SerializeField, Range(0f, 1f)] private float _holdAmplitude = 0.10f;

        [Header("Pulse Shape")]
        [SerializeField] private float _pulseDuration = 0.12f;
        [SerializeField] private bool _pulseBothHands = true;

        private BreathGuide _guide;
        private InputDevice _left;
        private InputDevice _right;
        private bool _devicesResolved;

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

        /// <summary>Fires an immediate one-off pulse. Used for exit feedback.</summary>
        public void Pulse(float amplitude, float duration)
        {
            if (!ComfortSettings.HapticsEnabled) return;
            ResolveDevices();
            Send(_left, amplitude, duration);
            if (_pulseBothHands) Send(_right, amplitude, duration);
        }

        private void OnPhaseChanged(BreathGuide.Phase phase)
        {
            if (!ComfortSettings.HapticsEnabled) return;

            float amplitude;
            switch (phase)
            {
                case BreathGuide.Phase.Inhale: amplitude = _inhaleAmplitude; break;
                case BreathGuide.Phase.Exhale: amplitude = _exhaleAmplitude; break;
                case BreathGuide.Phase.HoldAfterInhale:
                case BreathGuide.Phase.HoldAfterExhale: amplitude = _holdAmplitude; break;
                default: return;   // Idle — session ended, stay silent
            }

            if (amplitude <= 0.001f) return;

            // Re-resolve on every phase change rather than every frame: Quest
            // controllers drop off and re-attach when the headset wakes, and a
            // stale InputDevice silently swallows the impulse.
            ResolveDevices();

            Send(_left, amplitude, _pulseDuration);
            if (_pulseBothHands)
                Send(_right, amplitude, _pulseDuration);
        }

        private void ResolveDevices()
        {
            if (!_left.isValid)
                _left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (!_right.isValid)
                _right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            _devicesResolved = _left.isValid || _right.isValid;
        }

        private static void Send(InputDevice device, float amplitude, float duration)
        {
            if (!device.isValid) return;
            // Amplitude and duration are advisory — some runtimes clamp them.
            device.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), Mathf.Max(duration, 0.01f));
        }

        /// <summary>Editor-facing check so a misconfigured scene is obvious.</summary>
        public bool HasController =>
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).isValid ||
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).isValid;
    }
}
