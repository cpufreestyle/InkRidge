using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using TMPro;
using InkRidge.Core;
using InkRidge.Movement;

namespace InkRidge.UI
{
    /// <summary>
    /// In-VR settings panel. Toggled via controller menu button.
    /// World-space canvas attached to XR Origin (follows player facing).
    ///
    /// The toggle used to be `Input.GetKeyDown(KeyCode.M)`. This project runs
    /// with activeInputHandler=2 (Input System only), so legacy Input never
    /// fires — the panel was unreachable on a headset with no keyboard.
    ///
    /// NOTE: opening the panel is only half the job. Interacting with the
    /// sliders needs an XR Ray Interactor on each controller plus a
    /// TrackedDeviceGraphicRaycaster on this canvas; as of this change no
    /// scene contains either. See docs/FEATURE_PROPOSAL.md.
    /// </summary>
    public class XRMenu : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas _menuCanvas;
        [SerializeField] private float _distance = 2f;

        [Header("Controls")]
        [SerializeField] private Slider _moveSpeedSlider;
        [SerializeField] private Slider _turnAngleSlider;
        [SerializeField] private Toggle _vignetteToggle;
        [SerializeField] private Toggle _seatedToggle;
        [SerializeField] private Toggle _hapticsToggle;
        [SerializeField] private TextMeshProUGUI _moveSpeedLabel;
        [SerializeField] private TextMeshProUGUI _turnAngleLabel;

        private bool _isVisible;
        private bool _menuButtonWasPressed;

        void Start()
        {
            // Every control is optional: scenes built before this component was
            // wired have none of them assigned, and a null-deref here would
            // take the whole scene down on load.
            if (_moveSpeedSlider != null)
            {
                _moveSpeedSlider.value = ComfortSettings.MoveSpeed;
                _moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
            }

            if (_turnAngleSlider != null)
            {
                _turnAngleSlider.value = ComfortSettings.TurnAngle;
                _turnAngleSlider.onValueChanged.AddListener(OnTurnAngleChanged);
            }

            if (_vignetteToggle != null)
            {
                _vignetteToggle.isOn = ComfortSettings.VignetteEnabled;
                _vignetteToggle.onValueChanged.AddListener(OnVignetteChanged);
            }

            if (_seatedToggle != null)
            {
                _seatedToggle.isOn = ComfortSettings.SeatedMode;
                _seatedToggle.onValueChanged.AddListener(OnSeatedChanged);
            }

            if (_hapticsToggle != null)
            {
                _hapticsToggle.isOn = ComfortSettings.HapticsEnabled;
                _hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
            }

            UpdateLabels();
            SetVisible(false);
        }

        void Update()
        {
            // Edge-detect rather than poll isPressed, or holding the button
            // would toggle the panel every frame.
            bool pressed = IsMenuPressed();
            if (pressed && !_menuButtonWasPressed)
                ToggleMenu();
            _menuButtonWasPressed = pressed;
        }

        /// <summary>Left controller's menu button, or B/Y on either controller.</summary>
        private static bool IsMenuPressed()
        {
            return Pressed(XRController.leftHand) || Pressed(XRController.rightHand);
        }

        private static bool Pressed(XRController controller)
        {
            if (controller == null) return false;
            // XRController (Input System) exposes no typed button properties —
            // Quest controllers surface buttons as child controls named after
            // the XR usage. Query dynamically.
            return IsButtonPressed(controller, "menuButton") ||
                   IsButtonPressed(controller, "secondaryButton");
        }

        private static bool IsButtonPressed(XRController controller, string controlPath)
        {
            var button = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>(controlPath);
            return button != null && button.isPressed;
        }

        public void ToggleMenu()
        {
            SetVisible(!_isVisible);
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            _menuCanvas.gameObject.SetActive(visible);

            if (visible)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    transform.position = cam.transform.position + cam.transform.forward * _distance;
                    transform.LookAt(cam.transform);
                    transform.Rotate(0, 180, 0);
                }
            }
        }

        private void OnMoveSpeedChanged(float value)
        {
            ComfortSettings.MoveSpeed = value;
            UpdateLabels();
        }

        private void OnTurnAngleChanged(float value)
        {
            ComfortSettings.TurnAngle = value;
            UpdateLabels();
        }

        private void OnVignetteChanged(bool value)
        {
            ComfortSettings.VignetteEnabled = value;
        }

        private void OnSeatedChanged(bool value)
        {
            ComfortSettings.SeatedMode = value;
            var loco = FindObjectOfType<LocomotionController>();
            if (loco != null) loco.ApplySettings();
        }

        private void OnHapticsChanged(bool value)
        {
            ComfortSettings.HapticsEnabled = value;
        }

        private void UpdateLabels()
        {
            if (_moveSpeedLabel != null)
                _moveSpeedLabel.text = $"{ComfortSettings.MoveSpeed:F1} m/s";
            if (_turnAngleLabel != null)
                _turnAngleLabel.text = $"{ComfortSettings.TurnAngle:F0}°";
        }
    }
}
