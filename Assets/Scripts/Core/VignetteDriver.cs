using UnityEngine;

namespace InkRidge.Core
{
    /// <summary>
    /// Always-on companion to <see cref="VignetteEffect"/>, mounted on the same
    /// camera GameObject.
    ///
    /// Two jobs:
    /// 1. Detect that the player is moving. Nothing else in the project calls
    ///    VignetteEffect.SetMovementActive, so the comfort vignette never fired
    ///    even though ComfortSettings.VignetteEnabled and the XR menu toggle
    ///    both exist. Watching the camera's own world-position delta covers
    ///    thumbstick locomotion and physical room-scale walking alike, and needs
    ///    no references wired up in the scene.
    /// 2. Switch VignetteEffect off when it has nothing to draw. A Behaviour
    ///    cannot disable itself from its own Update (a disabled Behaviour gets
    ///    no Update), so this separate always-on component owns that decision.
    ///    While off, OnRenderImage never runs and the full-screen blit per eye
    ///    disappears from the frame entirely.
    /// </summary>
    [RequireComponent(typeof(VignetteEffect))]
    public class VignetteDriver : MonoBehaviour
    {
        [SerializeField, Tooltip("World units per second above which the player counts as moving.")]
        private float _moveThreshold = 0.15f;

        private VignetteEffect _effect;
        private Vector3 _lastPosition;
        private bool _hasLastPosition;

        void Awake()
        {
            _effect = GetComponent<VignetteEffect>();
            if (_effect == null)
            {
                Debug.LogWarning("[VignetteDriver] No VignetteEffect on this GameObject, disabling.");
                enabled = false;
            }
        }

        void Update()
        {
            var position = transform.position;
            float speed = 0f;
            if (_hasLastPosition)
                speed = Vector3.Distance(position, _lastPosition) / Mathf.Max(Time.deltaTime, 1e-5f);

            _lastPosition = position;
            _hasLastPosition = true;

            _effect.SetMovementActive(speed > _moveThreshold);

            // Note: this reads WantsRender *after* feeding it the new movement
            // state, so the fade-out keeps the effect alive until it settles.
            bool shouldRender = _effect.WantsRender;
            if (_effect.enabled != shouldRender)
                _effect.enabled = shouldRender;
        }
    }
}
