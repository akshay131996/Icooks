using UnityEngine;
using UnityEngine.Events;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// A kitchen station where the player brings ingredients to cook.
    /// Each station supports specific cooking actions (chopping, frying, boiling, etc.)
    /// and validates against active orders.
    /// </summary>
    public class CookingStation : MonoBehaviour
    {
        [Header("Station Config")]
        [SerializeField] private string stationName = "Cutting Board";
        [SerializeField] private CookingStep.StepType stationType = CookingStep.StepType.Chop;
        [SerializeField] private float cookingDuration = 3f;
        [SerializeField] private Transform ingredientSlot;
        [SerializeField] private SpriteRenderer stationSprite;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject progressBarPrefab;
        [SerializeField] private SpriteRenderer highlightRing;
        [SerializeField] private ParticleSystem cookingParticles;
        [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color readyColor  = new Color(0.2f, 1f, 0.4f, 0.5f);

        // ─── Runtime ───────────────────────────────────────────
        private bool _hasIngredient;
        private bool _isCooking;
        private float _cookTimer;
        private GameObject _currentIngredient;
        private bool _isReady; // finished cooking

        // ─── Events ────────────────────────────────────────────
        [HideInInspector] public UnityEvent<CookingStation> OnCookingStarted;
        [HideInInspector] public UnityEvent<CookingStation> OnCookingComplete;
        [HideInInspector] public UnityEvent<CookingStation> OnDishCollected;

        public string StationName => stationName;
        public CookingStep.StepType StationType => stationType;
        public bool HasIngredient => _hasIngredient;
        public bool IsCooking => _isCooking;
        public bool IsReady => _isReady;
        public float CookProgress => _isCooking ? _cookTimer / cookingDuration : (_isReady ? 1f : 0f);

        private void Awake()
        {
            OnCookingStarted  ??= new UnityEvent<CookingStation>();
            OnCookingComplete ??= new UnityEvent<CookingStation>();
            OnDishCollected   ??= new UnityEvent<CookingStation>();

            if (highlightRing != null)
                highlightRing.enabled = false;
        }

        private void Update()
        {
            if (!_isCooking) return;

            _cookTimer += Time.deltaTime;

            if (_cookTimer >= cookingDuration)
            {
                CompleteCooking();
            }
        }

        // ─── Interaction ───────────────────────────────────────

        /// <summary>
        /// Place an ingredient on this station. Called by PlayerController.
        /// </summary>
        public bool TryPlaceIngredient(GameObject ingredient)
        {
            if (_hasIngredient || _isCooking || _isReady) return false;

            _currentIngredient = ingredient;
            _hasIngredient = true;

            // Parent ingredient to the slot
            ingredient.transform.SetParent(ingredientSlot != null ? ingredientSlot : transform);
            ingredient.transform.localPosition = Vector3.zero;

            // Disable ingredient physics
            var rb = ingredient.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            var col = ingredient.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            StartCooking();
            return true;
        }

        /// <summary>
        /// Collect the finished dish. Called by PlayerController.
        /// </summary>
        public GameObject TryCollectDish()
        {
            if (!_isReady || _currentIngredient == null) return null;

            var dish = _currentIngredient;
            _currentIngredient = null;
            _hasIngredient = false;
            _isReady = false;

            // Re-enable physics
            dish.transform.SetParent(null);
            var rb = dish.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
            var col = dish.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            if (highlightRing != null)
                highlightRing.enabled = false;

            OnDishCollected?.Invoke(this);
            return dish;
        }

        // ─── Cooking Logic ─────────────────────────────────────
        private void StartCooking()
        {
            _isCooking = true;
            _cookTimer = 0f;

            if (cookingParticles != null)
                cookingParticles.Play();

            if (highlightRing != null)
            {
                highlightRing.enabled = true;
                highlightRing.color = activeColor;
            }

            OnCookingStarted?.Invoke(this);
            Debug.Log($"[CookingStation] {stationName}: cooking started ({stationType})");
        }

        private void CompleteCooking()
        {
            _isCooking = false;
            _isReady = true;

            if (cookingParticles != null)
                cookingParticles.Stop();

            if (highlightRing != null)
                highlightRing.color = readyColor;

            OnCookingComplete?.Invoke(this);
            Debug.Log($"[CookingStation] {stationName}: cooking complete!");
        }

        /// <summary>
        /// Show/hide interaction highlight when player is nearby.
        /// </summary>
        public void SetPlayerNearby(bool nearby)
        {
            if (highlightRing == null || _isCooking || _isReady) return;
            highlightRing.enabled = nearby;
            if (nearby) highlightRing.color = new Color(1f, 1f, 1f, 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
            if (ingredientSlot != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(ingredientSlot.position, 0.2f);
            }
        }
    }
}
