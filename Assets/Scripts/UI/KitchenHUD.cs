using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.UI
{
    /// <summary>
    /// Lightweight in-scene HUD that creates a Canvas at runtime with
    /// score display, timer, active orders panel, and pause button.
    /// Subscribes to GameManager and OrderManager events automatically.
    /// </summary>
    public class KitchenHUD : MonoBehaviour
    {
        // ─── Runtime References ────────────────────────────────
        private Canvas _canvas;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _ordersText;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _hintText;
        private Button _pauseButton;

        private float _levelTimer;
        private bool _isTimerRunning;

        private void Awake()
        {
            BuildCanvas();
            BuildScoreDisplay();
            BuildTimerDisplay();
            BuildLevelDisplay();
            BuildOrdersPanel();
            BuildPauseButton();
            BuildHintText();
        }

        private void Start()
        {
            // Subscribe to events
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged.AddListener(OnScoreChanged);
                Core.GameManager.Instance.OnStateChanged.AddListener(OnStateChanged);
            }
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged.RemoveListener(OnScoreChanged);
                Core.GameManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
            }
        }

        private void Update()
        {
            if (!_isTimerRunning) return;

            _levelTimer += Time.deltaTime;

            if (_timerText != null)
            {
                int mins = Mathf.FloorToInt(_levelTimer / 60f);
                int secs = Mathf.FloorToInt(_levelTimer % 60f);
                _timerText.text = $"{mins:00}:{secs:00}";
            }

            // Update active orders display
            UpdateOrdersDisplay();
        }

        // ─── Event Handlers ───────────────────────────────────

        private void OnScoreChanged(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        private void OnStateChanged(Core.GameManager.GameState state)
        {
            _isTimerRunning = state == Core.GameManager.GameState.Playing;

            if (state == Core.GameManager.GameState.Playing)
            {
                _levelTimer = 0f;
                if (_levelText != null)
                    _levelText.text = $"Level {Core.GameManager.Instance.CurrentLevel}";
            }
        }

        private void UpdateOrdersDisplay()
        {
            if (_ordersText == null) return;

            var om = Object.FindFirstObjectByType<Gameplay.OrderManager>();
            if (om == null || om.ActiveOrders.Count == 0)
            {
                _ordersText.text = "No orders";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Orders:</b>");
            foreach (var order in om.ActiveOrders)
            {
                float ratio = order.timeRemaining / order.totalTime;
                string color = ratio > 0.5f ? "#4CAF50" : (ratio > 0.25f ? "#FF9800" : "#F44336");
                int secs = Mathf.CeilToInt(order.timeRemaining);
                sb.AppendLine($"<color={color}>● {order.recipe.recipeName} ({secs}s)</color>");
            }
            _ordersText.text = sb.ToString();
        }

        // ─── UI Building ──────────────────────────────────────

        private void BuildCanvas()
        {
            var canvasObj = new GameObject("KitchenHUD_Canvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void BuildScoreDisplay()
        {
            _scoreText = CreateText("ScoreText", "Score: 0",
                new Vector2(20, -20), new Vector2(300, 50),
                TextAlignmentOptions.TopLeft);
            _scoreText.fontSize = 32;
            _scoreText.color = new Color(1f, 0.85f, 0.2f); // gold
        }

        private void BuildTimerDisplay()
        {
            _timerText = CreateText("TimerText", "00:00",
                new Vector2(0, -20), new Vector2(200, 50),
                TextAlignmentOptions.Top);
            _timerText.fontSize = 36;
            _timerText.color = Color.white;
            // Center horizontally
            var rt = _timerText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -20);
        }

        private void BuildLevelDisplay()
        {
            _levelText = CreateText("LevelText", "Level 1",
                new Vector2(20, -70), new Vector2(250, 40),
                TextAlignmentOptions.TopLeft);
            _levelText.fontSize = 24;
            _levelText.color = new Color(0.7f, 0.85f, 1f);
        }

        private void BuildOrdersPanel()
        {
            _ordersText = CreateText("OrdersText", "No orders",
                new Vector2(-20, -20), new Vector2(350, 200),
                TextAlignmentOptions.TopRight);
            _ordersText.fontSize = 22;
            _ordersText.color = Color.white;
            _ordersText.richText = true;

            // Anchor to top-right
            var rt = _ordersText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -20);
        }

        private void BuildPauseButton()
        {
            var btnObj = new GameObject("PauseButton");
            btnObj.transform.SetParent(_canvas.transform, false);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

            var rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -180);
            rt.sizeDelta = new Vector2(80, 40);

            var label = CreateTextChild(btnObj, "PauseLabel", "❚❚",
                Vector2.zero, new Vector2(80, 40), TextAlignmentOptions.Center);
            label.fontSize = 22;

            _pauseButton = btnObj.AddComponent<Button>();
            _pauseButton.targetGraphic = btnImg;
            _pauseButton.onClick.AddListener(() =>
            {
                if (Core.GameManager.Instance != null)
                {
                    if (Core.GameManager.Instance.CurrentState == Core.GameManager.GameState.Playing)
                        Core.GameManager.Instance.PauseGame();
                    else if (Core.GameManager.Instance.CurrentState == Core.GameManager.GameState.Paused)
                        Core.GameManager.Instance.ResumeGame();
                }
            });
        }

        private void BuildHintText()
        {
            _hintText = CreateText("HintText", "WASD to move | SPACE to interact",
                new Vector2(0, 20), new Vector2(500, 40),
                TextAlignmentOptions.Bottom);
            _hintText.fontSize = 20;
            _hintText.color = new Color(1f, 1f, 1f, 0.6f);
            var rt = _hintText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 20);
        }

        // ─── Helpers ──────────────────────────────────────────

        private TextMeshProUGUI CreateText(string name, string text,
            Vector2 position, Vector2 size, TextAlignmentOptions align)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(_canvas.transform, false);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = align;
            tmp.fontSize = 28;

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            return tmp;
        }

        private TextMeshProUGUI CreateTextChild(GameObject parent, string name, string text,
            Vector2 position, Vector2 size, TextAlignmentOptions align)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = align;

            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            return tmp;
        }
    }
}
