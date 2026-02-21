using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChefJourney.UI
{
    /// <summary>
    /// Chalkboard-style UI for displaying orders, timers, scores, and combo text.
    /// Hand-written aesthetic with rope/incense timers and manga impact text.
    /// </summary>
    public class ChalkboardUI : MonoBehaviour
    {
        [Header("Board References")]
        [SerializeField] private RectTransform orderBoardPanel;
        [SerializeField] private RectTransform scoreBoardPanel;
        [SerializeField] private RectTransform comboPanel;

        [Header("Order Display")]
        [SerializeField] private GameObject orderTicketPrefab;
        [SerializeField] private Transform orderContainer;
        [SerializeField] private int maxVisibleOrders = 4;

        [Header("Score Display")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text levelText;

        [Header("Chalkboard Colors")]
        [SerializeField] private Color boardColor = new Color(0.18f, 0.22f, 0.18f);
        [SerializeField] private Color chalkWhite = new Color(0.92f, 0.9f, 0.85f);
        [SerializeField] private Color chalkYellow = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private Color chalkRed = new Color(0.9f, 0.35f, 0.3f);
        [SerializeField] private Color chalkGreen = new Color(0.5f, 0.85f, 0.5f);

        [Header("Combo Impact")]
        [SerializeField] private float comboPopScale = 1.5f;
        [SerializeField] private float comboPopDuration = 0.3f;

        [Header("Timer Style")]
        [SerializeField] private Color timerFullColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private Color timerWarnColor = new Color(1f, 0.5f, 0.2f);
        [SerializeField] private Color timerCriticalColor = new Color(0.9f, 0.25f, 0.2f);

        // ─── Runtime ───────────────────────────────────────────
        private int _currentScore;
        private int _comboCount;
        private int _maxCombo;
        private readonly List<OrderTicket> _activeTickets = new List<OrderTicket>();

        private struct OrderTicket
        {
            public GameObject gameObject;
            public Image timerFill;
            public Text recipeName;
            public Text timerText;
            public Gameplay.Order order;
        }

        private void Awake()
        {
            if (comboPanel != null)
                comboPanel.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateOrderTimers();
        }

        // ─── Public API ────────────────────────────────────────

        /// <summary>
        /// Add a new order ticket to the chalkboard.
        /// </summary>
        public void AddOrder(Gameplay.Order order)
        {
            if (_activeTickets.Count >= maxVisibleOrders) return;

            GameObject ticketObj;
            if (orderTicketPrefab != null && orderContainer != null)
            {
                ticketObj = Instantiate(orderTicketPrefab, orderContainer);
            }
            else
            {
                ticketObj = CreateDefaultTicket();
            }

            var ticket = new OrderTicket
            {
                gameObject = ticketObj,
                timerFill = ticketObj.transform.Find("TimerFill")?.GetComponent<Image>(),
                recipeName = ticketObj.transform.Find("RecipeName")?.GetComponent<Text>(),
                timerText = ticketObj.transform.Find("TimerText")?.GetComponent<Text>(),
                order = order
            };

            if (ticket.recipeName != null)
                ticket.recipeName.text = order.recipe.recipeName;

            _activeTickets.Add(ticket);
        }

        /// <summary>
        /// Remove a completed order ticket with a chalk-strike animation.
        /// </summary>
        public void CompleteOrder(Gameplay.Order order)
        {
            for (int i = _activeTickets.Count - 1; i >= 0; i--)
            {
                if (_activeTickets[i].order == order)
                {
                    StartCoroutine(StrikethroughAndRemove(_activeTickets[i]));
                    _activeTickets.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Update the score with a chalk-drawing animation.
        /// </summary>
        public void SetScore(int newScore)
        {
            _currentScore = newScore;
            if (scoreText != null)
                scoreText.text = $"₹ {_currentScore:N0}";
        }

        /// <summary>
        /// Show a manga-style combo popup.
        /// </summary>
        public void ShowCombo(int combo)
        {
            _comboCount = combo;
            if (combo > _maxCombo) _maxCombo = combo;

            if (comboPanel != null && comboText != null)
            {
                comboPanel.gameObject.SetActive(true);

                string comboMsg = combo switch
                {
                    >= 10 => $"★ LEGENDARY ×{combo}! ★",
                    >= 7  => $"🔥 BLAZING ×{combo}!",
                    >= 5  => $"⚡ AMAZING ×{combo}!",
                    >= 3  => $"✦ COMBO ×{combo}!",
                    _ => $"Nice! ×{combo}"
                };

                comboText.text = comboMsg;
                comboText.color = combo >= 5 ? chalkYellow : chalkWhite;

                StartCoroutine(ComboPopAnimation());
            }
        }

        /// <summary>
        /// Show a fail message when an order times out.
        /// </summary>
        public void ShowOrderFailed(string recipeName)
        {
            if (comboPanel != null && comboText != null)
            {
                comboPanel.gameObject.SetActive(true);
                comboText.text = $"Order Lost: {recipeName}";
                comboText.color = chalkRed;
                StartCoroutine(ComboPopAnimation());
            }

            // Reset combo
            _comboCount = 0;
        }

        public void SetLevel(int level, string locationName)
        {
            if (levelText != null)
                levelText.text = $"Day {level} — {locationName}";
        }

        // ─── Timer Updates ─────────────────────────────────────

        private void UpdateOrderTimers()
        {
            foreach (var ticket in _activeTickets)
            {
                if (ticket.order == null) continue;

                float ratio = ticket.order.timeRemaining / ticket.order.totalTime;

                // Update timer fill (rope burning / incense shortening)
                if (ticket.timerFill != null)
                {
                    ticket.timerFill.fillAmount = ratio;
                    ticket.timerFill.color = ratio > 0.5f ? timerFullColor :
                                             ratio > 0.25f ? timerWarnColor :
                                             timerCriticalColor;
                }

                // Update timer text
                if (ticket.timerText != null)
                {
                    int seconds = Mathf.CeilToInt(ticket.order.timeRemaining);
                    ticket.timerText.text = $"{seconds}s";
                    ticket.timerText.color = ratio > 0.25f ? chalkWhite : chalkRed;
                }
            }
        }

        // ─── Animations ───────────────────────────────────────

        private IEnumerator StrikethroughAndRemove(OrderTicket ticket)
        {
            // Flash green for success
            if (ticket.recipeName != null)
                ticket.recipeName.color = chalkGreen;

            yield return new WaitForSeconds(0.5f);

            // Scale down and fade out
            float elapsed = 0f;
            float duration = 0.3f;
            var canvasGroup = ticket.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = ticket.gameObject.AddComponent<CanvasGroup>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = 1f - t;
                ticket.gameObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, t);
                yield return null;
            }

            Destroy(ticket.gameObject);
        }

        private IEnumerator ComboPopAnimation()
        {
            if (comboPanel == null) yield break;

            // Pop in
            comboPanel.localScale = Vector3.one * 0.3f;
            float elapsed = 0f;

            while (elapsed < comboPopDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / comboPopDuration;
                // Overshoot elastic curve
                float scale = 1f + (comboPopScale - 1f) * (1f - Mathf.Pow(1f - t, 3f));
                if (t > 0.6f) scale = Mathf.Lerp(comboPopScale, 1f, (t - 0.6f) / 0.4f);
                comboPanel.localScale = Vector3.one * scale;
                yield return null;
            }

            comboPanel.localScale = Vector3.one;

            // Hold then fade
            yield return new WaitForSeconds(1.5f);

            elapsed = 0f;
            var cg = comboPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = comboPanel.gameObject.AddComponent<CanvasGroup>();

            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - elapsed / 0.3f;
                yield return null;
            }

            comboPanel.gameObject.SetActive(false);
            cg.alpha = 1f;
        }

        // ─── Default Ticket Creation ───────────────────────────

        private GameObject CreateDefaultTicket()
        {
            var ticket = new GameObject("OrderTicket");
            if (orderContainer != null)
                ticket.transform.SetParent(orderContainer, false);

            var rt = ticket.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 60);

            var bg = ticket.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.15f, 0.9f);

            // Recipe name
            var nameObj = new GameObject("RecipeName");
            nameObj.transform.SetParent(ticket.transform, false);
            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.5f);
            nameRT.anchorMax = new Vector2(0.7f, 0.95f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            var nameText = nameObj.AddComponent<Text>();
            nameText.color = chalkWhite;
            nameText.fontSize = 14;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Timer text
            var timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(ticket.transform, false);
            var timerRT = timerObj.AddComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0.75f, 0.5f);
            timerRT.anchorMax = new Vector2(0.95f, 0.95f);
            timerRT.offsetMin = Vector2.zero;
            timerRT.offsetMax = Vector2.zero;
            var timerText = timerObj.AddComponent<Text>();
            timerText.color = chalkYellow;
            timerText.fontSize = 13;
            timerText.alignment = TextAnchor.MiddleRight;
            timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Timer fill bar
            var fillObj = new GameObject("TimerFill");
            fillObj.transform.SetParent(ticket.transform, false);
            var fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0.05f, 0.1f);
            fillRT.anchorMax = new Vector2(0.95f, 0.35f);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = timerFullColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            return ticket;
        }
    }
}
