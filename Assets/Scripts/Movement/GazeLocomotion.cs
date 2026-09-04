using UnityEngine;
using UnityEngine.XR;

namespace InkRidge.Movement
{
    /// <summary>
    /// Controller-free locomotion: gaze-assisted walking.
    ///
    /// Reads the HMD gaze direction; when the player pinches (either hand's
    /// pinch strength > 0.7, from the system hand-tracking data) or holds
    /// gaze on the ground, smoothly moves the XR Origin along the gaze
    /// direction projected onto the horizontal plane.
    ///
    /// Designed for players without controllers — pinch is the Quest system
    /// hand-tracking "index pinch" gesture, available without any package
    /// install (system hand tracking feeds pinch via Input Devices).
    /// Attach to the XROrigin GameObject.
    /// </summary>
    public class GazeLocomotion : MonoBehaviour
    {
        [Header("Gaze Walk")]
        [SerializeField] private float _gazeHoldSeconds = 1.5f;   // look-at-target hold before moving
        [SerializeField] private float _moveSpeed = 1.2f;         // m/s while walking
        [SerializeField] private float _maxWalkDistance = 8f;     // single walk segment cap

        [Header("Pinch Confirm")]
        [SerializeField] private float _pinchThreshold = 0.7f;    // pinch strength 0..1

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundMask = ~0;
        [SerializeField] private float _maxGazeDistance = 12f;

        private Transform _xrOrigin;
        private Camera _mainCamera;
        private float _gazeHold;
        private Vector3 _walkTarget;
        private bool _walking;

        void Start()
        {
            // This component sits on the XROrigin GO (same as LocomotionController).
            _xrOrigin = transform;
            _mainCamera = Camera.main;
        }

        void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            // ── Walking state: glide toward target ──
            if (_walking)
            {
                Vector3 flat = new Vector3(_walkTarget.x - _xrOrigin.position.x, 0f,
                                           _walkTarget.z - _xrOrigin.position.z);
                if (flat.magnitude < 0.15f)
                {
                    _walking = false;
                    return;
                }
                float step = _moveSpeed * Time.deltaTime;
                if (step > flat.magnitude) step = flat.magnitude;
                _xrOrigin.position += flat.normalized * step;
                return;
            }

            // ── Gaze detection: gaze ray hits the ground within range ──
            Ray ray = new Ray(_mainCamera.transform.position,
                              _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit, _maxGazeDistance, _groundMask) &&
                Vector3.Dot(hit.normal, Vector3.up) > 0.5f)  // ground-ish surface only
            {
                _gazeHold += Time.deltaTime;

                // Walk when gaze is held long enough, or when pinching.
                if (_gazeHold >= _gazeHoldSeconds || IsPinching())
                {
                    StartWalk(hit.point);
                    _gazeHold = 0f;
                }
            }
            else
            {
                _gazeHold = Mathf.Max(0f, _gazeHold - Time.deltaTime * 2f);
            }
        }

        private bool IsPinching()
        {
            var left = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            var right = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            return IsPinchSimple(left) || IsPinchSimple(right);
        }

        private static bool IsPinchSimple(InputDevice device)
        {
            if (!device.isValid) return false;
            // Quest hand tracking: trigger is a float axis (0..1) — pinch/grip.
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float grip) && grip > 0.5f)
                return true;
            // Fallback: grip as float axis.
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float g) && g > 0.5f)
                return true;
            return false;
        }

        private void StartWalk(Vector3 targetPoint)
        {
            Vector3 flat = new Vector3(targetPoint.x - _xrOrigin.position.x, 0f,
                                       targetPoint.z - _xrOrigin.position.z);
            if (flat.magnitude > _maxWalkDistance)
                flat = flat.normalized * _maxWalkDistance;
            _walkTarget = _xrOrigin.position + flat;
            _walking = true;
        }
    }
}
