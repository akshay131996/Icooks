using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Manages customer orders — spawning, tracking, timing, and completion.
    /// Works with the Recipe system to create dynamic gameplay.
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [Header("Level Config")]
        [SerializeField] private Core.Data.LevelConfigSO currentLevel;

        [Header("Runtime Pacing (Overridden by LevelConfig)")]
        [SerializeField] private List<Recipe> availableRecipes = new List<Recipe>();
        [SerializeField] private int maxActiveOrders = 3;
        [SerializeField] private float spawnInterval = 15f;
        [SerializeField] private float minSpawnInterval = 5f;
        [SerializeField] private float spawnIntervalReduction = 0.5f;

        [Header("Legacy / Fallbacks")]
        [SerializeField] private float orderTimeout = 60f;

        // ─── Runtime ───────────────────────────────────────────
        private readonly List<Order> _activeOrders = new List<Order>();
        private float _spawnTimer;

        // ─── Events ────────────────────────────────────────────
        [HideInInspector] public UnityEvent<Order> OnOrderSpawned;
        [HideInInspector] public UnityEvent<Order> OnOrderCompleted;
        [HideInInspector] public UnityEvent<Order> OnOrderFailed;

        public IReadOnlyList<Order> ActiveOrders => _activeOrders;

        private void Start()
        {
            ApplyLevelConfig();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            if (Core.GameManager.Instance == null) return;
            if (Core.GameManager.Instance.CurrentState != Core.GameManager.GameState.Playing) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && _activeOrders.Count < maxActiveOrders)
            {
                SpawnOrder();
                _spawnTimer = Mathf.Max(spawnInterval, minSpawnInterval);
            }

            // Update timers for active orders
            for (int i = _activeOrders.Count - 1; i >= 0; i--)
            {
                _activeOrders[i].timeRemaining -= Time.deltaTime;
                if (_activeOrders[i].timeRemaining <= 0f)
                {
                    FailOrder(_activeOrders[i]);
                }
            }
        }

        public void SpawnOrder()
        {
            if (availableRecipes.Count == 0) return;

            var recipe = availableRecipes[Random.Range(0, availableRecipes.Count)];
            var order = new Order
            {
                recipe = recipe,
                timeRemaining = recipe.timeLimit > 0 ? recipe.timeLimit : orderTimeout,
                totalTime = recipe.timeLimit > 0 ? recipe.timeLimit : orderTimeout,
            };

            _activeOrders.Add(order);
            OnOrderSpawned?.Invoke(order);
            Debug.Log($"[OrderManager] New order: {recipe.recipeName}");
        }

        public void CompleteOrder(Order order)
        {
            if (!_activeOrders.Contains(order)) return;

            // Calculate score based on time remaining
            float timeRatio = order.timeRemaining / order.totalTime;
            int points = order.recipe.basePoints;
            if (timeRatio > 0.5f)
                points += order.recipe.perfectBonus;

            Core.GameManager.Instance?.AddScore(points);
            _activeOrders.Remove(order);
            OnOrderCompleted?.Invoke(order);
            Debug.Log($"[OrderManager] Completed: {order.recipe.recipeName} (+{points})");
        }

        private void FailOrder(Order order)
        {
            _activeOrders.Remove(order);
            OnOrderFailed?.Invoke(order);
            Debug.Log($"[OrderManager] Failed: {order.recipe.recipeName}");
        }

        /// <summary>
        /// Increase difficulty (call when level increases).
        /// </summary>
        public void IncreaseDifficulty()
        {
            spawnInterval = Mathf.Max(spawnInterval - spawnIntervalReduction, minSpawnInterval);
            maxActiveOrders++;
        }

        public void ClearAllOrders() => _activeOrders.Clear();

        public void ApplyLevelConfig()
        {
            if (currentLevel != null)
            {
                availableRecipes = new List<Recipe>(currentLevel.availableMenu);
                maxActiveOrders = currentLevel.maxSimultaneousOrders;
                spawnInterval = currentLevel.orderSpawnInterval;
                _spawnTimer = currentLevel.initialSpawnDelay;

                // Sync GameManager duration if applicable
                if (Core.GameManager.Instance != null && currentLevel.shiftDuration > 0)
                {
                    Core.GameManager.Instance.StartLevel(currentLevel.levelNumber);
                    // AddScore 0 just to trigger UI updates if money/score were set
                }

                Debug.Log($"[OrderManager] Loaded Level Config: {currentLevel.levelName}");
            }
        }
    }

    /// <summary>
    /// Runtime data for an active customer order.
    /// </summary>
    [System.Serializable]
    public class Order
    {
        public Recipe recipe;
        public float timeRemaining;
        public float totalTime;
        public bool isUrgent => timeRemaining / totalTime < 0.25f;
    }
}
