using UnityEngine;

namespace ChefJourney.Core
{
    /// <summary>
    /// Handles player input and chef character movement.
    /// Supports both touch (mobile) and mouse/keyboard input.
    /// Space key cycles through contextual actions:
    ///   - Near ingredient → pick up
    ///   - Near station (carrying) → place ingredient
    ///   - Near station (dish ready) → collect dish
    ///   - Near customer (carrying dish) → deliver order
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float interactionRange = 1.5f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // ─── Runtime ───────────────────────────────────────────
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private bool _isCarrying;
        private GameObject _carriedObject;
        private bool _carryingDish; // true if carrying a cooked dish (vs raw ingredient)

        // Animation hashes (cached for performance)
        private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
        private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
        private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int AnimIsCarrying = Animator.StringToHash("IsCarrying");

        public bool IsCarrying => _isCarrying;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // Keyboard / gamepad input
            _moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            // Space key — contextual action
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DoContextualAction();
            }

            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        // ─── Public API (for touch controls) ──────────────────

        /// <summary>
        /// Call from touch controls / on-screen joystick.
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input.normalized;
        }

        /// <summary>
        /// Trigger the contextual action from a UI button.
        /// </summary>
        public void DoContextualAction()
        {
            // Priority 1: deliver dish to customer
            if (_isCarrying && _carryingDish && TryDeliverToCustomer()) return;

            // Priority 2: place ingredient on station
            if (_isCarrying && !_carryingDish && TryPlaceOnStation()) return;

            // Priority 3: collect finished dish from station
            if (!_isCarrying && TryCollectFromStation()) return;

            // Priority 4: pick up raw ingredient
            if (!_isCarrying && TryPickup()) return;
        }

        // ─── Interaction Methods ──────────────────────────────

        /// <summary>
        /// Try to pick up the nearest ingredient within range.
        /// </summary>
        public bool TryPickup()
        {
            if (_isCarrying) return false;

            var hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);
            float closest = float.MaxValue;
            Collider2D bestHit = null;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Ingredient"))
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < closest)
                    {
                        closest = dist;
                        bestHit = hit;
                    }
                }
            }

            if (bestHit == null) return false;

            _carriedObject = bestHit.gameObject;
            _carriedObject.transform.SetParent(transform);
            _carriedObject.transform.localPosition = Vector3.up * 0.8f;
            _isCarrying = true;
            _carryingDish = false;

            // Disable physics while carried
            var rb = _carriedObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            var col = _carriedObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Debug.Log($"[Player] Picked up {_carriedObject.name}");
            return true;
        }

        /// <summary>
        /// Place the carried ingredient on the nearest CookingStation.
        /// </summary>
        public bool TryPlaceOnStation()
        {
            if (!_isCarrying || _carriedObject == null) return false;

            var station = FindNearest<Gameplay.CookingStation>();
            if (station == null) return false;

            if (station.TryPlaceIngredient(_carriedObject))
            {
                Debug.Log($"[Player] Placed ingredient on {station.StationName}");
                _carriedObject = null;
                _isCarrying = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Collect a finished dish from the nearest CookingStation.
        /// </summary>
        public bool TryCollectFromStation()
        {
            if (_isCarrying) return false;

            var station = FindNearest<Gameplay.CookingStation>();
            if (station == null || !station.IsReady) return false;

            var dish = station.TryCollectDish();
            if (dish != null)
            {
                _carriedObject = dish;
                _carriedObject.transform.SetParent(transform);
                _carriedObject.transform.localPosition = Vector3.up * 0.8f;
                _isCarrying = true;
                _carryingDish = true;

                // Disable physics while carried
                var rb = _carriedObject.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;
                var col = _carriedObject.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                Debug.Log($"[Player] Collected dish from {station.StationName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deliver the carried dish to the nearest active Customer.
        /// </summary>
        public bool TryDeliverToCustomer()
        {
            if (!_isCarrying || !_carryingDish) return false;

            var customer = FindNearest<Gameplay.Customer>();
            if (customer == null || !customer.IsActive) return false;

            // Deliver the order
            customer.DeliverOrder();

            // Complete the order in OrderManager
            var orderManager = Object.FindFirstObjectByType<Gameplay.OrderManager>();
            if (orderManager != null && customer.CurrentOrder != null)
            {
                orderManager.CompleteOrder(customer.CurrentOrder);
            }

            // Destroy the dish object
            if (_carriedObject != null)
                Destroy(_carriedObject);
            _carriedObject = null;
            _isCarrying = false;
            _carryingDish = false;

            Debug.Log("[Player] Delivered dish to customer!");
            return true;
        }

        // ─── Helpers ──────────────────────────────────────────

        /// <summary>
        /// Find the nearest component of type T within interaction range.
        /// </summary>
        private T FindNearest<T>() where T : Component
        {
            var candidates = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            T best = null;
            float bestDist = interactionRange;

            foreach (var c in candidates)
            {
                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = c;
                }
            }
            return best;
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            bool isMoving = _moveInput.sqrMagnitude > 0.01f;
            animator.SetBool(AnimIsMoving, isMoving);
            animator.SetBool(AnimIsCarrying, _isCarrying);

            if (isMoving)
            {
                animator.SetFloat(AnimMoveX, _moveInput.x);
                animator.SetFloat(AnimMoveY, _moveInput.y);

                // Flip sprite based on horizontal direction
                if (spriteRenderer != null && Mathf.Abs(_moveInput.x) > 0.1f)
                    spriteRenderer.flipX = _moveInput.x < 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
