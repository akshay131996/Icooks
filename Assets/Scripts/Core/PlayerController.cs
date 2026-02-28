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
        [SerializeField] private ChefAnimationController animationController;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // ─── Runtime ───────────────────────────────────────────
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _touchInput;
        private bool _isCarrying;
        private GameObject _carriedObject;
        private bool _carryingDish; 

        public bool IsCarrying => _isCarrying;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (animationController == null)
                animationController = GetComponent<ChefAnimationController>();
        }

        private void Update()
        {
            // Keyboard input
            Vector2 keyboardInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            // Keyboard takes priority over touch; use touch only when keyboard is idle
            _moveInput = keyboardInput.sqrMagnitude > 0.01f ? keyboardInput : _touchInput;

            // Animation and direction
            if (animationController != null)
            {
                if (_moveInput.sqrMagnitude > 0.01f)
                {
                    animationController.SetState(ChefAnimationController.ChefState.Walking);
                    animationController.SetFacingDirection(_moveInput.x);
                }
                else
                {
                    animationController.SetState(ChefAnimationController.ChefState.Idle);
                }
            }

            // Space key — contextual action
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DoContextualAction();
            }
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
            _touchInput = input.normalized;
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



        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
