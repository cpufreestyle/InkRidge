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

        void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            if (_moveProvider != null)
            {
                _moveProvider.moveSpeed = ComfortSettings.MoveSpeed;
                _moveProvider.useGravity = true;
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
    }
}
