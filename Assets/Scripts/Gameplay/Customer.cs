using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Visual customer that sits at a table/counter, displays an order bubble,
    /// and reacts to order fulfillment or timeout.
    /// </summary>
    public class Customer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer characterSprite;
        [SerializeField] private SpriteRenderer orderBubbleSprite;
        [SerializeField] private SpriteRenderer orderIconSprite;
        [SerializeField] private SpriteRenderer timerFillSprite;
        [SerializeField] private Transform deliveryPoint;

        [Header("Patience Visuals")]
        [SerializeField] private Color happyColor = Color.green;
        [SerializeField] private Color warnColor  = Color.yellow;
        [SerializeField] private Color angryColor = Color.red;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobAmount = 0.05f;

        // ─── Runtime ───────────────────────────────────────────
        private Order _currentOrder;
        private bool _isActive;
        private Vector3 _startPos;

        private static readonly int AnimHappy = Animator.StringToHash("Happy");
        private static readonly int AnimAngry = Animator.StringToHash("Angry");
        private static readonly int AnimIdle  = Animator.StringToHash("Idle");

        public Order CurrentOrder => _currentOrder;
        public bool IsActive => _isActive;
        public Transform DeliveryPoint => deliveryPoint != null ? deliveryPoint : transform;

        private void Awake()
        {
            _startPos = transform.position;
            SetVisible(false);
        }

        private void Update()
        {
            if (!_isActive || _currentOrder == null) return;

            // Idle bobbing animation
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = _startPos + Vector3.up * bob;

            // Update patience timer visual
            UpdatePatienceDisplay();
        }

        // ─── Public API ────────────────────────────────────────

        /// <summary>
        /// Assign an order to this customer and show them.
        /// </summary>
        public void AssignOrder(Order order)
        {
            _currentOrder = order;
            _isActive = true;
            SetVisible(true);

            // Show recipe icon in order bubble
            if (orderIconSprite != null && order.recipe.icon != null)
                orderIconSprite.sprite = order.recipe.icon;

            if (animator != null)
                animator.SetTrigger(AnimIdle);

            Debug.Log($"[Customer] Order assigned: {order.recipe.recipeName}");
        }

        /// <summary>
        /// Deliver the correct dish to this customer.
        /// </summary>
        public void DeliverOrder()
        {
            if (!_isActive) return;

            if (animator != null)
                animator.SetTrigger(AnimHappy);

            Debug.Log($"[Customer] Happy! Order delivered: {_currentOrder.recipe.recipeName}");

            // Play celebration, then depart
            Invoke(nameof(Depart), 1.5f);
        }

        /// <summary>
        /// Customer leaves angrily (order timed out).
        /// </summary>
        public void FailOrder()
        {
            if (!_isActive) return;

            if (animator != null)
                animator.SetTrigger(AnimAngry);

            Debug.Log($"[Customer] Angry! Order timed out: {_currentOrder.recipe.recipeName}");

            Invoke(nameof(Depart), 1f);
        }

        private void Depart()
        {
            _isActive = false;
            _currentOrder = null;
            SetVisible(false);
        }

        // ─── Visuals ───────────────────────────────────────────
        private void SetVisible(bool visible)
        {
            if (characterSprite != null) characterSprite.enabled = visible;
            if (orderBubbleSprite != null) orderBubbleSprite.enabled = visible;
            if (orderIconSprite != null) orderIconSprite.enabled = visible;
            if (timerFillSprite != null) timerFillSprite.enabled = visible;
        }

        private void UpdatePatienceDisplay()
        {
            if (_currentOrder == null || timerFillSprite == null) return;

            float ratio = _currentOrder.timeRemaining / _currentOrder.totalTime;

            // Scale the timer fill
            timerFillSprite.transform.localScale = new Vector3(ratio, 1f, 1f);

            // Color based on patience
            if (ratio > 0.5f)
                timerFillSprite.color = happyColor;
            else if (ratio > 0.25f)
                timerFillSprite.color = warnColor;
            else
                timerFillSprite.color = angryColor;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            var dp = deliveryPoint != null ? deliveryPoint.position : transform.position;
            Gizmos.DrawWireSphere(dp, 0.5f);
        }
    }
}
