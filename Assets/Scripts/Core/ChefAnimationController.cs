using UnityEngine;

namespace ChefJourney.Core
{
    /// <summary>
    /// Drives chef character animations and expression changes.
    /// Supports state-driven sprite switching for: idle, walk, cook, plate.
    /// Manga-style expression overlays for reactions.
    /// </summary>
    public class ChefAnimationController : MonoBehaviour
    {
        [Header("Sprite References")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer expressionOverlay;
        [SerializeField] private SpriteRenderer hatRenderer;

        [Header("Animation Sprites (placeholder until sprite sheets)")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite walkSprite1;
        [SerializeField] private Sprite walkSprite2;
        [SerializeField] private Sprite cookSprite;
        [SerializeField] private Sprite plateSprite;

        [Header("Expression Sprites (manga overlays)")]
        [SerializeField] private Sprite focusedExpression;
        [SerializeField] private Sprite happyExpression;
        [SerializeField] private Sprite stressedExpression;  // sweat drops
        [SerializeField] private Sprite excitedExpression;   // sparkle eyes

        [Header("Animation Settings")]
        [SerializeField] private float walkAnimSpeed = 0.2f;
        [SerializeField] private float bobHeight = 0.08f;
        [SerializeField] private float bobSpeed = 6f;
        [SerializeField] private float squashStretchAmount = 0.05f;

        [Header("Visual FX")]
        [SerializeField] private GameObject speedLinesPrefab;
        [SerializeField] private GameObject impactStarsPrefab;

        // ─── State ────────────────────────────────────────────
        public enum ChefState { Idle, Walking, Cooking, Plating, Celebrating }
        public enum Expression { Neutral, Focused, Happy, Stressed, Excited }

        private ChefState _currentState = ChefState.Idle;
        private Expression _currentExpression = Expression.Neutral;
        private float _animTimer;
        private bool _walkFrame;
        private Vector3 _baseScale;
        private Vector3 _basePos;
        private bool _facingRight = true;

        public ChefState CurrentState => _currentState;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _basePos = transform.localPosition;

            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _animTimer += Time.deltaTime;

            switch (_currentState)
            {
                case ChefState.Idle:
                    AnimateIdle();
                    break;
                case ChefState.Walking:
                    AnimateWalk();
                    break;
                case ChefState.Cooking:
                    AnimateCook();
                    break;
                case ChefState.Plating:
                    AnimatePlate();
                    break;
                case ChefState.Celebrating:
                    AnimateCelebrate();
                    break;
            }
        }

        // ─── Public API ────────────────────────────────────────

        public void SetState(ChefState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _animTimer = 0f;
        }

        public void SetExpression(Expression expr, float duration = 0f)
        {
            _currentExpression = expr;

            if (expressionOverlay != null)
            {
                expressionOverlay.sprite = expr switch
                {
                    Expression.Focused => focusedExpression,
                    Expression.Happy => happyExpression,
                    Expression.Stressed => stressedExpression,
                    Expression.Excited => excitedExpression,
                    _ => null,
                };
                expressionOverlay.enabled = expressionOverlay.sprite != null;
            }

            if (duration > 0)
                Invoke(nameof(ClearExpression), duration);
        }

        public void SetFacingDirection(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            _facingRight = horizontalInput > 0;
            var scale = transform.localScale;
            scale.x = _facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // ─── Animations ───────────────────────────────────────

        private void AnimateIdle()
        {
            // Subtle breathing bob
            float bob = Mathf.Sin(_animTimer * bobSpeed * 0.5f) * bobHeight * 0.5f;
            transform.localPosition = _basePos + Vector3.up * bob;

            // Gentle squash-stretch (breathing)
            float squash = 1f + Mathf.Sin(_animTimer * bobSpeed * 0.5f) * squashStretchAmount * 0.3f;
            transform.localScale = new Vector3(
                _baseScale.x * (2f - squash) * (_facingRight ? 1 : -1),
                _baseScale.y * squash,
                _baseScale.z
            );

            if (bodyRenderer != null && idleSprite != null)
                bodyRenderer.sprite = idleSprite;
        }

        private void AnimateWalk()
        {
            // Walk bob — more energetic
            float bob = Mathf.Abs(Mathf.Sin(_animTimer * bobSpeed)) * bobHeight;
            transform.localPosition = _basePos + Vector3.up * bob;

            // Squash on landing
            float squash = 1f + Mathf.Abs(Mathf.Sin(_animTimer * bobSpeed)) * squashStretchAmount;
            transform.localScale = new Vector3(
                _baseScale.x * squash * (_facingRight ? 1 : -1),
                _baseScale.y * (2f - squash),
                _baseScale.z
            );

            // Frame swap
            if (_animTimer >= walkAnimSpeed)
            {
                _animTimer = 0f;
                _walkFrame = !_walkFrame;
                if (bodyRenderer != null)
                {
                    if (_walkFrame && walkSprite1 != null)
                        bodyRenderer.sprite = walkSprite1;
                    else if (walkSprite2 != null)
                        bodyRenderer.sprite = walkSprite2;
                }
            }
        }

        private void AnimateCook()
        {
            // Rhythmic chopping/stirring motion
            float chop = Mathf.Sin(_animTimer * 8f) * 0.03f;
            transform.localPosition = _basePos + Vector3.up * chop;

            if (bodyRenderer != null && cookSprite != null)
                bodyRenderer.sprite = cookSprite;
        }

        private void AnimatePlate()
        {
            if (bodyRenderer != null && plateSprite != null)
                bodyRenderer.sprite = plateSprite;
        }

        private void AnimateCelebrate()
        {
            // Jump + spin celebration
            float jump = Mathf.Abs(Mathf.Sin(_animTimer * 4f)) * bobHeight * 3f;
            transform.localPosition = _basePos + Vector3.up * jump;

            SetExpression(Expression.Excited);

            if (_animTimer > 1.5f)
            {
                SetState(ChefState.Idle);
                ClearExpression();
            }
        }

        private void ClearExpression()
        {
            _currentExpression = Expression.Neutral;
            if (expressionOverlay != null)
                expressionOverlay.enabled = false;
        }
    }
}
