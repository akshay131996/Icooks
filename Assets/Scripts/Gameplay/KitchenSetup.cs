using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Scene bootstrapper that programmatically creates a complete kitchen level.
    /// Attach this to an empty GameObject in the KitchenLevel scene.
    /// It sets up: floor, walls, cooking stations, ingredient spawners,
    /// customer seats, the player chef, and all manager singletons.
    /// </summary>
    public class KitchenSetup : MonoBehaviour
    {
        [Header("Prefabs (optional — will create defaults if null)")]
        [SerializeField] private GameObject chefPrefab;
        [SerializeField] private GameObject cookingStationPrefab;
        [SerializeField] private GameObject ingredientSpawnerPrefab;
        [SerializeField] private GameObject customerPrefab;

        [Header("Kitchen Layout — Indian Street Stall Theme")]
        [SerializeField] private Vector2 kitchenSize = new Vector2(16f, 10f);
        [SerializeField] private Color floorColor = new Color(0.76f, 0.58f, 0.38f);  // dusty terracotta
        [SerializeField] private Color wallColor  = new Color(0.55f, 0.35f, 0.18f);  // dark wood / bamboo
        [SerializeField] private Color counterColor = new Color(0.45f, 0.40f, 0.38f); // oxidized steel tawa

        [Header("Station Positions")]
        [SerializeField] private Vector2[] stationPositions = new Vector2[]
        {
            new Vector2(-4f,  2f),  // Chopping board
            new Vector2(-1f,  2f),  // Stove (fry)
            new Vector2( 2f,  2f),  // Oven (bake)
            new Vector2( 5f,  2f),  // Plating
        };

        [SerializeField] private CookingStep.StepType[] stationTypes = new CookingStep.StepType[]
        {
            CookingStep.StepType.Chop,
            CookingStep.StepType.Fry,
            CookingStep.StepType.Bake,
            CookingStep.StepType.Plate,
        };

        [SerializeField] private string[] stationNames = new string[]
        {
            "Cutting Board",
            "Stove",
            "Oven",
            "Plating Counter",
        };

        [Header("Supply Station Positions")]
        [SerializeField] private Vector2[] supplyPositions = new Vector2[]
        {
            new Vector2(-5f, -2f),
            new Vector2(-2f, -2f),
            new Vector2( 1f, -2f),
        };

        [Header("Customer Positions")]
        [SerializeField] private Vector2[] customerPositions = new Vector2[]
        {
            new Vector2(-3f, -4.5f),
            new Vector2( 0f, -4.5f),
            new Vector2( 3f, -4.5f),
        };

        private void Start()
        {
            CreateKitchen();
            CreateManagers();
            CreateChef();
            CreateStations();
            CreateSupplyStations();
            CreateCustomerSeats();
            CreateCamera();

            Debug.Log("[KitchenSetup] Kitchen level initialized!");
        }

        // ─── Kitchen Environment ───────────────────────────────
        private void CreateKitchen()
        {
            // Floor
            var floor = CreateSprite("Floor", Vector3.zero, kitchenSize, floorColor, -10);

            // Counter / prep area (top half)
            var counter = CreateSprite("Counter",
                new Vector3(0, kitchenSize.y * 0.25f - 0.5f, 0),
                new Vector2(kitchenSize.x - 1f, 0.3f),
                counterColor, -5);

            // Serving counter (bottom divider)
            var servingCounter = CreateSprite("ServingCounter",
                new Vector3(0, -kitchenSize.y * 0.15f, 0),
                new Vector2(kitchenSize.x - 1f, 0.25f),
                new Color(0.5f, 0.3f, 0.2f), -5);

            // Walls (boundaries)
            CreateWall("WallTop",    new Vector3(0, kitchenSize.y / 2, 0),  new Vector2(kitchenSize.x + 1, 0.5f));
            CreateWall("WallBottom", new Vector3(0, -kitchenSize.y / 2, 0), new Vector2(kitchenSize.x + 1, 0.5f));
            CreateWall("WallLeft",   new Vector3(-kitchenSize.x / 2, 0, 0), new Vector2(0.5f, kitchenSize.y + 1));
            CreateWall("WallRight",  new Vector3(kitchenSize.x / 2, 0, 0),  new Vector2(0.5f, kitchenSize.y + 1));
        }

        private void CreateManagers()
        {
            // Ensure singletons exist
            if (Core.GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<Core.GameManager>();
            }

            if (Economy.CurrencyManager.Instance == null)
            {
                var cm = new GameObject("CurrencyManager");
                cm.AddComponent<Economy.CurrencyManager>();
            }

            if (FindFirstObjectByType<OrderManager>() == null)
            {
                var om = new GameObject("OrderManager");
                om.AddComponent<OrderManager>();
            }
        }

        private void CreateChef()
        {
            GameObject chef;
            if (chefPrefab != null)
            {
                chef = Instantiate(chefPrefab, Vector3.zero, Quaternion.identity);
            }
            else
            {
                chef = new GameObject("Chef");
                chef.transform.position = Vector3.zero;

                // Visual
                var sr = chef.AddComponent<SpriteRenderer>();
                sr.color = Color.white;
                sr.sortingOrder = 10;

                // Create a simple chef hat indicator
                var hat = new GameObject("ChefHat");
                hat.transform.SetParent(chef.transform);
                hat.transform.localPosition = new Vector3(0, 0.6f, 0);
                var hatSr = hat.AddComponent<SpriteRenderer>();
                hatSr.color = Color.white;
                hatSr.sortingOrder = 11;

                // Physics
                var rb = chef.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.freezeRotation = true;

                var col = chef.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);

                // Player controller
                chef.AddComponent<Core.PlayerController>();
            }
            chef.name = "Chef";
            chef.tag = "Player";
        }

        private void CreateStations()
        {
            var parent = new GameObject("CookingStations");

            for (int i = 0; i < stationPositions.Length; i++)
            {
                GameObject station;
                if (cookingStationPrefab != null)
                {
                    station = Instantiate(cookingStationPrefab, stationPositions[i], Quaternion.identity, parent.transform);
                }
                else
                {
                    station = new GameObject(stationNames[i]);
                    station.transform.position = (Vector3)stationPositions[i];
                    station.transform.SetParent(parent.transform);

                    // Visual
                    var sr = station.AddComponent<SpriteRenderer>();
                    sr.color = GetStationColor(stationTypes[i]);
                    sr.sortingOrder = 2;

                    // Label
                    var label = new GameObject("Label");
                    label.transform.SetParent(station.transform);
                    label.transform.localPosition = new Vector3(0, -0.8f, 0);

                    // Collision
                    var col = station.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1.8f, 1.2f);
                    col.isTrigger = true;

                    // Ingredient slot
                    var slot = new GameObject("IngredientSlot");
                    slot.transform.SetParent(station.transform);
                    slot.transform.localPosition = new Vector3(0, 0.3f, 0);

                    // Cooking station component
                    var cs = station.AddComponent<CookingStation>();
                }
            }
        }

        private void CreateSupplyStations()
        {
            var parent = new GameObject("SupplyStations");

            for (int i = 0; i < supplyPositions.Length; i++)
            {
                var supply = new GameObject($"Supply_{i}");
                supply.transform.position = (Vector3)supplyPositions[i];
                supply.transform.SetParent(parent.transform);

                // Visual
                var sr = supply.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.4f, 0.7f, 0.4f);
                sr.sortingOrder = 2;

                // Collision
                var col = supply.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.5f, 1f);
                col.isTrigger = true;

                // Spawner
                supply.AddComponent<IngredientSpawner>();
            }
        }

        private void CreateCustomerSeats()
        {
            var parent = new GameObject("CustomerSeats");

            for (int i = 0; i < customerPositions.Length; i++)
            {
                var seat = new GameObject($"Customer_{i}");
                seat.transform.position = (Vector3)customerPositions[i];
                seat.transform.SetParent(parent.transform);

                // Seat visual
                var seatSprite = CreateSprite($"Seat_{i}",
                    (Vector3)customerPositions[i] - Vector3.up * 0.3f,
                    new Vector2(1f, 0.5f),
                    new Color(0.6f, 0.4f, 0.25f), 1);
                seatSprite.transform.SetParent(parent.transform);

                // Order bubble
                var bubble = new GameObject("OrderBubble");
                bubble.transform.SetParent(seat.transform);
                bubble.transform.localPosition = new Vector3(0.5f, 0.8f, 0);
                var bubbleSr = bubble.AddComponent<SpriteRenderer>();
                bubbleSr.color = new Color(1f, 1f, 1f, 0.8f);
                bubbleSr.sortingOrder = 15;

                seat.AddComponent<Customer>();
            }
        }

        private void CreateCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = kitchenSize.y / 2 + 1f;
            cam.backgroundColor = new Color(0.15f, 0.12f, 0.1f);
        }

        // ─── Helpers ───────────────────────────────────────────
        private GameObject CreateSprite(string name, Vector3 pos, Vector2 size, Color color, int sortOrder)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = sortOrder;
            obj.transform.localScale = new Vector3(size.x, size.y, 1);
            return obj;
        }

        private void CreateWall(string name, Vector3 pos, Vector2 size)
        {
            var wall = CreateSprite(name, pos, size, wallColor, -8);
            var col = wall.AddComponent<BoxCollider2D>();
            // Collider needs to be unit-size since the scale handles visual size
            col.size = Vector2.one;
        }

        private Color GetStationColor(CookingStep.StepType type)
        {
            return type switch
            {
                CookingStep.StepType.Chop => new Color(0.85f, 0.75f, 0.55f), // wood
                CookingStep.StepType.Fry  => new Color(0.3f, 0.3f, 0.35f),   // cast iron
                CookingStep.StepType.Boil => new Color(0.7f, 0.7f, 0.75f),   // steel pot
                CookingStep.StepType.Bake => new Color(0.45f, 0.35f, 0.3f),  // oven
                CookingStep.StepType.Mix  => new Color(0.8f, 0.8f, 0.85f),   // bowl
                CookingStep.StepType.Plate => new Color(0.95f, 0.95f, 0.97f), // white plate
                CookingStep.StepType.Garnish => new Color(0.4f, 0.7f, 0.4f), // herb green
                _ => Color.gray
            };
        }
    }
}
