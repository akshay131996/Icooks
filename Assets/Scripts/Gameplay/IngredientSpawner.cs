using System.Collections.Generic;
using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Spawns ingredient objects at supply station positions.
    /// Ingredients respawn after being picked up.
    /// </summary>
    public class IngredientSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Ingredient ingredientData;
        [SerializeField] private GameObject ingredientPrefab;
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private int maxSpawned = 3;

        [Header("Spawn Area")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius = 0.3f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer stationIcon;
        [SerializeField] private SpriteRenderer labelRenderer;

        // ─── Runtime ───────────────────────────────────────────
        private readonly List<GameObject> _spawnedIngredients = new List<GameObject>();
        private float _respawnTimer;
        private int _nextSpawnIndex;

        public Ingredient IngredientData => ingredientData;

        private void Start()
        {
            // Set station icon to ingredient sprite
            if (stationIcon != null && ingredientData != null && ingredientData.worldSprite != null)
                stationIcon.sprite = ingredientData.worldSprite;

            // Initial spawn
            for (int i = 0; i < Mathf.Min(maxSpawned, GetSpawnPointCount()); i++)
            {
                SpawnIngredient();
            }
        }

        private void Update()
        {
            // Clean up destroyed references
            _spawnedIngredients.RemoveAll(item => item == null);

            // Respawn if below max
            if (_spawnedIngredients.Count < maxSpawned)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f)
                {
                    SpawnIngredient();
                    _respawnTimer = respawnDelay;
                }
            }
        }

        private void SpawnIngredient()
        {
            Vector3 spawnPos = GetNextSpawnPosition();

            GameObject ingredient;
            if (ingredientPrefab != null)
            {
                ingredient = Instantiate(ingredientPrefab, spawnPos, Quaternion.identity, transform);
            }
            else
            {
                // Create a simple ingredient object if no prefab is assigned
                ingredient = CreateDefaultIngredient(spawnPos);
            }

            ingredient.tag = "Ingredient";
            ingredient.name = ingredientData != null ? ingredientData.ingredientName : "Ingredient";
            _spawnedIngredients.Add(ingredient);
        }

        private GameObject CreateDefaultIngredient(Vector3 position)
        {
            var obj = new GameObject("Ingredient");
            obj.transform.position = position;
            obj.transform.SetParent(transform);

            // Sprite
            var sr = obj.AddComponent<SpriteRenderer>();
            if (ingredientData != null && ingredientData.worldSprite != null)
                sr.sprite = ingredientData.worldSprite;
            else if (ingredientData != null && ingredientData.icon != null)
                sr.sprite = ingredientData.icon;
            sr.sortingOrder = 5;

            if (ingredientData != null)
                sr.color = ingredientData.tintColor;

            // Collision for pickup detection
            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = true;

            // Physics
            var rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            return obj;
        }

        private Vector3 GetNextSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var pos = spawnPoints[_nextSpawnIndex % spawnPoints.Length].position;
                _nextSpawnIndex++;
                return pos;
            }

            // Random position within spawn radius
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(offset.x, offset.y, 0);
        }

        private int GetSpawnPointCount()
        {
            return (spawnPoints != null && spawnPoints.Length > 0) ? spawnPoints.Length : maxSpawned;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            if (spawnPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                        Gizmos.DrawSphere(point.position, 0.15f);
                }
            }
        }
    }
}
