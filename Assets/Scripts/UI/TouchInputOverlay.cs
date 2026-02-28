using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ChefJourney.UI
{
    /// <summary>
    /// Touch input overlay with a virtual joystick (left side) and
    /// an action button (right side). Sends input to PlayerController.
    /// Only shows on mobile / touch-capable devices.
    /// </summary>
    public class TouchInputOverlay : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float joystickRadius = 100f;
        [SerializeField] private Color baseColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color stickColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color buttonColor = new Color(0.3f, 0.8f, 0.4f, 0.7f);

        // ─── Runtime ───────────────────────────────────────────
        private Canvas _canvas;
        private RectTransform _joystickBase;
        private RectTransform _joystickStick;
        private bool _isJoystickActive;
        private Vector2 _inputVector;
        private Core.PlayerController _player;

        private void Awake()
        {
            BuildCanvas();
            BuildJoystick();
            BuildActionButton();
        }

        private void Start()
        {
            _player = Object.FindFirstObjectByType<Core.PlayerController>();
        }

        private void Update()
        {
            // Only send touch input when the joystick is actively being dragged
            if (_player != null && _isJoystickActive)
            {
                _player.SetMoveInput(_inputVector);
            }
        }

        // ─── UI Building ──────────────────────────────────────

        private void BuildCanvas()
        {
            var canvasObj = new GameObject("TouchOverlay_Canvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 110; // above HUD

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private void BuildJoystick()
        {
            // Joystick base (outer ring)
            var baseObj = new GameObject("JoystickBase");
            baseObj.transform.SetParent(_canvas.transform, false);
            var baseImg = baseObj.AddComponent<Image>();
            baseImg.color = baseColor;

            _joystickBase = baseObj.GetComponent<RectTransform>();
            _joystickBase.anchorMin = new Vector2(0f, 0f);
            _joystickBase.anchorMax = new Vector2(0f, 0f);
            _joystickBase.pivot = new Vector2(0.5f, 0.5f);
            _joystickBase.anchoredPosition = new Vector2(200, 200);
            _joystickBase.sizeDelta = new Vector2(joystickRadius * 2, joystickRadius * 2);

            // Joystick stick (inner circle)
            var stickObj = new GameObject("JoystickStick");
            stickObj.transform.SetParent(baseObj.transform, false);
            var stickImg = stickObj.AddComponent<Image>();
            stickImg.color = stickColor;

            _joystickStick = stickObj.GetComponent<RectTransform>();
            _joystickStick.sizeDelta = new Vector2(joystickRadius * 0.8f, joystickRadius * 0.8f);
            _joystickStick.anchoredPosition = Vector2.zero;

            // Add drag handler to the base
            var dragger = baseObj.AddComponent<JoystickDragger>();
            dragger.Init(this, _joystickBase, _joystickStick, joystickRadius);
        }

        private void BuildActionButton()
        {
            var btnObj = new GameObject("ActionButton");
            btnObj.transform.SetParent(_canvas.transform, false);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = buttonColor;

            var rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-150, 200);
            rt.sizeDelta = new Vector2(120, 120);

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var label = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            label.text = "ACT";
            label.fontSize = 28;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.white;
            var labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImg;
            button.onClick.AddListener(() =>
            {
                if (_player != null)
                    _player.DoContextualAction();
            });
        }

        /// <summary>
        /// Called by JoystickDragger to update the input vector.
        /// </summary>
        public void SetJoystickInput(Vector2 input)
        {
            _inputVector = input;
        }

        /// <summary>
        /// Called by JoystickDragger to signal whether the joystick is being used.
        /// </summary>
        public void SetJoystickActive(bool active)
        {
            _isJoystickActive = active;
            if (!active)
            {
                _inputVector = Vector2.zero;
                // Clear touch input on the player when joystick released
                if (_player != null)
                    _player.SetMoveInput(Vector2.zero);
            }
        }
    }

    /// <summary>
    /// Handles drag events on the virtual joystick base.
    /// </summary>
    public class JoystickDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private TouchInputOverlay _overlay;
        private RectTransform _base;
        private RectTransform _stick;
        private float _radius;

        public void Init(TouchInputOverlay overlay, RectTransform baseRt, RectTransform stickRt, float radius)
        {
            _overlay = overlay;
            _base = baseRt;
            _stick = stickRt;
            _radius = radius;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _overlay.SetJoystickActive(true);
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _base, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // Normalize to -1..1
                Vector2 input = localPoint / _radius;
                input = Vector2.ClampMagnitude(input, 1f);

                // Move stick visual
                _stick.anchoredPosition = input * _radius * 0.5f;

                // Send to overlay
                _overlay.SetJoystickInput(input);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _stick.anchoredPosition = Vector2.zero;
            _overlay.SetJoystickInput(Vector2.zero);
            _overlay.SetJoystickActive(false);
        }
    }
}
