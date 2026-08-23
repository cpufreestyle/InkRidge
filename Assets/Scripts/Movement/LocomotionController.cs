using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using InkRidge.Core;

namespace InkRidge.Movement
{
    /// <summary>
    /// Configures smooth locomotion + snap turn from ComfortSettings.
    /// Attach to the XR Origin GameObject.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class LocomotionController : MonoBehaviour
    {
        [SerializeField] private ContinuousMoveProvider _moveProvider;
        [SerializeField] private SnapTurnProvider _turnProvider;

        [Header("Footsteps")]
        [SerializeField] private AudioSource _footstepSource;
        [SerializeField] private AudioClip _footstepClip;
        [SerializeField] private float _stepInterval = 0.8f;

        private float _stepTimer;

        void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            if (_moveProvider != null)
            {
                _moveProvider.moveSpeed = ComfortSettings.MoveSpeed;
            }

            if (_turnProvider != null)
            {
                _turnProvider.turnAmount = ComfortSettings.TurnAngle;
            }

            var xrOrigin = GetComponent<XROrigin>();
            if (xrOrigin != null)
            {
                ComfortSettings.ApplySeatedMode(xrOrigin.transform);
            }
        }

        void Update()
        {
            if (_moveProvider != null && _footstepSource != null && _footstepClip != null)
            {
                var input = _moveProvider.leftHandMoveInput;
                if (input != null && input.TryReadValue(out var value) && value.magnitude > 0.1f)
                {
                    _stepTimer += Time.deltaTime;
                    if (_stepTimer >= _stepInterval)
                    {
                        _footstepSource.PlayOneShot(_footstepClip);
                        _stepTimer = 0f;
                    }
                }
                else
                {
                    _stepTimer = _stepInterval;
                }
            }
        }
    }
}
