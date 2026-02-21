using UnityEngine;

namespace ChefJourney.Core
{
    /// <summary>
    /// Handles player input and chef character movement.
    /// Supports both touch (mobile) and mouse/keyboard input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float pickupRange = 1.2f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // ─── Runtime ───────────────────────────────────────────
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private bool _isCarrying;

        // Animation hashes (cached for performance)
        private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
        private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
        private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int AnimIsCarrying = Animator.StringToHash("IsCarrying");

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // Keyboard / gamepad input fallback
            _moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        /// <summary>
        /// Call from touch controls / on-screen joystick.
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input.normalized;
        }

        /// <summary>
        /// Try to pick up the nearest ingredient within range.
        /// </summary>
        public void TryPickup()
        {
            if (_isCarrying) return;

            var hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Ingredient"))
                {
                    hit.transform.SetParent(transform);
                    hit.transform.localPosition = Vector3.up * 0.8f;
                    _isCarrying = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Drop the currently carried ingredient at a station.
        /// </summary>
        public void TryDrop()
        {
            if (!_isCarrying) return;

            foreach (Transform child in transform)
            {
                if (child.CompareTag("Ingredient"))
                {
                    child.SetParent(null);
                    _isCarrying = false;
                    break;
                }
            }
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
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
