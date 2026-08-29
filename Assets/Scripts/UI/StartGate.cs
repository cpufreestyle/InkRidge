using UnityEngine;
using UnityEngine.InputSystem;
using InkRidge.Core;

namespace InkRidge.UI
{
    /// <summary>
    /// Start-scene gate. The 00_Start scene previously had no interactable:
    /// StartGame() existed but nothing called it, leaving the player staring
    /// at a stone stele with no way to enter the game.
    /// This component shows an in-VR prompt above the stele and starts the
    /// game via gaze confirmation (look toward the stele for 3 seconds) or
    /// any controller button press, with an idle auto-start fallback.
    /// Canvas is built at runtime to match the project's runtime-generated-UI
    /// approach (no editor-authored prefab).
    /// </summary>
    public class StartGate : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Transform _gazeTarget;   // the stone stele
        [SerializeField] private float _gazeHoldSeconds = 3f;
        [SerializeField] private float _gazeAngle = 25f;  // generous: stele sits below eye line
        [SerializeField] private float _idleAutoStartSeconds = 12f;
        [SerializeField] private float _promptDistance = 3.2f;
        [SerializeField] private float _promptHeight = 2.3f;

        private Camera _mainCamera;
        private Canvas _canvas;
        private UnityEngine.UI.Text _text;
        private float _gazeTimer;
        private float _idleTimer;
        private bool _started;

        void Start()
        {
            _mainCamera = Camera.main;
            if (_gazeTarget == null && _mainCamera != null)
            {
                // Fall back to whatever this component sits on.
                _gazeTarget = transform;
            }
            BuildPrompt();
        }

        void Update()
        {
            if (_started || _mainCamera == null) return;

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleAutoStartSeconds)
            {
                BeginGame();
                return;
            }

            bool gazing = false;
            if (_gazeTarget != null)
            {
                // Aim at the top of the stele — its center is below eye level,
                // so looking slightly down at the readable part counts.
                var targetPos = _gazeTarget.position + Vector3.up * (_gazeTarget.localScale.y * 0.5f);
                var toTarget = targetPos - _mainCamera.transform.position;
                gazing = Vector3.Angle(_mainCamera.transform.forward, toTarget) < _gazeAngle;
            }

            // Input System only project (activeInputHandler=2): legacy
            // Input.GetKeyDown never fires. Poll common VR controller buttons.
            bool buttonPressed = false;
            if (Gamepad.current != null)
            {
                var gp = Gamepad.current;
                buttonPressed =
                    gp.buttonSouth.wasPressedThisFrame ||
                    gp.buttonNorth.wasPressedThisFrame ||
                    gp.buttonEast.wasPressedThisFrame ||
                    gp.buttonWest.wasPressedThisFrame ||
                    gp.startButton.wasPressedThisFrame ||
                    gp.selectButton.wasPressedThisFrame ||
                    gp.rightTrigger.wasPressedThisFrame ||
                    gp.leftTrigger.wasPressedThisFrame;
            }

            if (buttonPressed)
            {
                BeginGame();
                return;
            }

            if (gazing)
            {
                _gazeTimer += Time.deltaTime;
                UpdatePrompt(_gazeTimer / _gazeHoldSeconds);
                if (_gazeTimer >= _gazeHoldSeconds)
                    BeginGame();
            }
            else if (_gazeTimer > 0f)
            {
                _gazeTimer = Mathf.Max(0f, _gazeTimer - Time.deltaTime * 2f);
                UpdatePrompt(_gazeTimer / _gazeHoldSeconds);
            }
        }
        void BeginGame()
        {
            if (_started) return;
            _started = true;
            UpdatePrompt(1f);
            if (_text != null) _text.text = "Begin ...";
            GameManager.Instance?.StartGame();
        }

        void BuildPrompt()
        {
            if (_mainCamera == null) return;

            var canvasObj = new GameObject("StartPromptCanvas");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;

            // Face the spawn point at comfortable reading distance.
            canvasObj.transform.position = new Vector3(0f, _promptHeight, _promptDistance);
            canvasObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 120f);

            // Background panel
            var bg = new GameObject("BG", typeof(UnityEngine.UI.Image));
            bg.transform.SetParent(canvasObj.transform, false);
            var bgImg = bg.GetComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.08f, 0.07f, 0.06f, 0.85f);
            bg.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

            // Title
            _text = CreateText("Title", rt.sizeDelta - new Vector2(20f, 20f));
            // LegacyRuntime.ttf has no CJK glyphs; fall back to English text.
            _text.text = "INK RIDGE\nLook at the stele or press any button\n(auto-start in a moment)";
            _text.fontSize = 22;

            // Progress bar (fades in while gazing)
            var barBg = new GameObject("BarBG", typeof(UnityEngine.UI.Image));
            barBg.transform.SetParent(canvasObj.transform, false);
            var barBgRt = barBg.GetComponent<RectTransform>();
            barBgRt.anchorMin = new Vector2(0.5f, 0f);
            barBgRt.anchorMax = new Vector2(0.5f, 0f);
            barBgRt.pivot = new Vector2(0.5f, 0f);
            barBgRt.anchoredPosition = new Vector2(0f, 10f);
            barBgRt.sizeDelta = new Vector2(240f, 6f);
            barBg.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 0.25f);

            var barFill = new GameObject("BarFill", typeof(UnityEngine.UI.Image));
            barFill.transform.SetParent(barBg.transform, false);
            _barFill = barFill.GetComponent<UnityEngine.UI.Image>();
            _barFill.color = new Color(0.92f, 0.90f, 0.85f);
            var barFillRt = barFill.GetComponent<RectTransform>();
            barFillRt.anchorMin = new Vector2(0f, 0f);
            barFillRt.anchorMax = new Vector2(0f, 1f);
            barFillRt.pivot = new Vector2(0f, 0.5f);
            barFillRt.sizeDelta = new Vector2(0f, 6f);
            UpdatePrompt(0f);
        }

        private UnityEngine.UI.Image _barFill;

        private UnityEngine.UI.Text CreateText(string name, Vector2 size)
        {
            var obj = new GameObject(name, typeof(UnityEngine.UI.Text));
            obj.transform.SetParent(_canvas.transform, false);
            var t = obj.GetComponent<UnityEngine.UI.Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 28;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.94f, 0.91f, 0.86f);
            obj.GetComponent<RectTransform>().sizeDelta = size;
            return t;
        }

        private void UpdatePrompt(float progress01)
        {
            if (_barFill != null)
            {
                _barFill.fillAmount = 0f;
                var rt = _barFill.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(240f * Mathf.Clamp01(progress01), 6f);
            }
        }
    }
}
