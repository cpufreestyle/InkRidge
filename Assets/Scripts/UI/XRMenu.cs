using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRidge.Core;
using InkRidge.Movement;

namespace InkRidge.UI
{
    /// <summary>
    /// In-VR settings panel. Toggled via controller menu button.
    /// World-space canvas attached to XR Origin (follows player facing).
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
        [SerializeField] private TextMeshProUGUI _moveSpeedLabel;
        [SerializeField] private TextMeshProUGUI _turnAngleLabel;

        private bool _isVisible;

        void Start()
        {
            _moveSpeedSlider.value = ComfortSettings.MoveSpeed;
            _turnAngleSlider.value = ComfortSettings.TurnAngle;
            _vignetteToggle.isOn = ComfortSettings.VignetteEnabled;
            _seatedToggle.isOn = ComfortSettings.SeatedMode;

            _moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
            _turnAngleSlider.onValueChanged.AddListener(OnTurnAngleChanged);
            _vignetteToggle.onValueChanged.AddListener(OnVignetteChanged);
            _seatedToggle.onValueChanged.AddListener(OnSeatedChanged);

            UpdateLabels();
            SetVisible(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMenu();
            }
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
            var loco = FindFirstObjectByType<LocomotionController>();
            if (loco != null) loco.ApplySettings();
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
