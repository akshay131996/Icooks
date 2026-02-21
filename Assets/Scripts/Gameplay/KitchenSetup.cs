using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Premium scene bootstrapper: builds a side-view Indian street stall
    /// with parallax depth layers, particle effects, fairy lights,
    /// sorting layers, and all gameplay objects.
    /// </summary>
    public class KitchenSetup : MonoBehaviour
    {
        [Header("Prefabs (uses procedural defaults if null)")]
        [SerializeField] private GameObject chefPrefab;
        [SerializeField] private GameObject cookingStationPrefab;
        [SerializeField] private GameObject ingredientSpawnerPrefab;
        [SerializeField] private GameObject customerPrefab;

        // ─── Color Palette ─────────────────────────────────────
        [Header("Indian Street Stall — Color Palette")]
        [SerializeField] private Color skyTop     = new Color(0.05f, 0.02f, 0.12f);   // deep night
        [SerializeField] private Color skyBottom   = new Color(0.15f, 0.06f, 0.22f);   // purple horizon
        [SerializeField] private Color distantCity = new Color(0.12f, 0.08f, 0.15f);   // silhouette
        [SerializeField] private Color groundColor = new Color(0.35f, 0.25f, 0.18f);   // dusty road
        [SerializeField] private Color cartWood    = new Color(0.42f, 0.26f, 0.13f);   // cart wood
        [SerializeField] private Color cartWoodDark = new Color(0.30f, 0.18f, 0.08f);  // cart shadow
        [SerializeField] private Color stallRoof   = new Color(0.75f, 0.2f, 0.15f);    // red awning
        [SerializeField] private Color stallRoofStripe = new Color(0.95f, 0.9f, 0.75f);// cream stripe
        [SerializeField] private Color tawaColor   = new Color(0.2f, 0.2f, 0.22f);     // cast iron
        [SerializeField] private Color marigold    = new Color(1f, 0.75f, 0f);         // marigold gold
        [SerializeField] private Color turmeric    = new Color(0.91f, 0.64f, 0.09f);   // turmeric amber
        [SerializeField] private Color tandoori    = new Color(0.75f, 0.22f, 0.17f);   // tandoori red
        [SerializeField] private Color mintGreen   = new Color(0.1f, 0.73f, 0.61f);    // mint chutney

        // ─── Layout ────────────────────────────────────────────
        [Header("Side-View Layout")]
        [SerializeField] private float sceneWidth = 18f;
        [SerializeField] private float stallHeight = 4.5f;
        [SerializeField] private float counterY = -0.5f;
        [SerializeField] private float groundY = -3.5f;

        [Header("Station Config")]
        [SerializeField] private string[] stationNames = { "Tawa", "Chulha", "Prep Board", "Plating" };
        [SerializeField] private CookingStep.StepType[] stationTypes =
        {
            CookingStep.StepType.Fry,
            CookingStep.StepType.Boil,
            CookingStep.StepType.Chop,
            CookingStep.StepType.Plate,
        };

        // ─── Runtime refs ──────────────────────────────────────
        private KitchenAmbience _ambience;
        private readonly List<Transform> _steamPoints = new List<Transform>();
        private readonly List<Transform> _firePoints = new List<Transform>();
        private readonly List<SpriteRenderer> _fairyLightRenderers = new List<SpriteRenderer>();

        private void Start()
        {
            CreateSortingLayers();
            CreateParallaxBackground();
            CreateStreetGround();
            CreateStallStructure();
            CreateCookingStations();
            CreateSupplyStations();
            CreateCustomerArea();
            CreateChef();
            CreateAmbience();
            CreateManagers();
            CreateCamera();
            CreateUI();

            Debug.Log("[KitchenSetup] ✦ Indian Street Stall kitchen loaded! ✦");
        }

        // ═══════════════════════════════════════════════════════
        // PARALLAX BACKGROUND — 4 depth layers
        // ═══════════════════════════════════════════════════════
        private void CreateParallaxBackground()
        {
            var bgRoot = new GameObject("ParallaxBackground");
            var parallax = bgRoot.AddComponent<Core.ParallaxController>();
            var layers = new List<Core.ParallaxController.ParallaxLayer>();

            // Layer 1: Night sky gradient (barely moves)
            var sky = CreateGradientQuad("Sky", 22f, 14f,
                skyTop, skyBottom, -100, bgRoot.transform);
            sky.transform.position = new Vector3(0, 2f, 0);
            layers.Add(new Core.ParallaxController.ParallaxLayer
            {
                name = "Sky", layerTransform = sky.transform,
                parallaxFactor = 0.02f
            });

            // Layer 2: Stars / moon
            CreateStarField(sky.transform);

            // Layer 3: Distant city silhouette
            var city = CreateCitySilhouette(bgRoot.transform);
            layers.Add(new Core.ParallaxController.ParallaxLayer
            {
                name = "City", layerTransform = city.transform,
                parallaxFactor = 0.1f, infiniteScrollX = true
            });

            // Layer 4: Mid-ground buildings / street life
            var midground = CreateMidgroundBuildings(bgRoot.transform);
            layers.Add(new Core.ParallaxController.ParallaxLayer
            {
                name = "MidGround", layerTransform = midground.transform,
                parallaxFactor = 0.3f
            });

            parallax.SetupLayers(layers.ToArray());
        }

        private void CreateStarField(Transform parent)
        {
            var stars = new GameObject("Stars");
            stars.transform.SetParent(parent);
            stars.transform.localPosition = Vector3.zero;

            System.Random rng = new System.Random(42);
            for (int i = 0; i < 40; i++)
            {
                var star = new GameObject($"Star_{i}");
                star.transform.SetParent(stars.transform);
                star.transform.localPosition = new Vector3(
                    (float)(rng.NextDouble() * 20 - 10),
                    (float)(rng.NextDouble() * 6 + 1),
                    0
                );
                var sr = star.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.95f, 0.8f, (float)(rng.NextDouble() * 0.5 + 0.2));
                sr.sortingOrder = -99;
                star.transform.localScale = Vector3.one * (float)(rng.NextDouble() * 0.06 + 0.02);
            }

            // Moon
            var moon = CreateSprite("Moon", new Vector3(6f, 4f, 0), new Vector2(1.2f, 1.2f),
                new Color(1f, 0.95f, 0.8f, 0.6f), -98);
            moon.transform.SetParent(parent);
        }

        private GameObject CreateCitySilhouette(Transform parent)
        {
            var city = new GameObject("CitySilhouette");
            city.transform.SetParent(parent);
            city.transform.position = new Vector3(0, 0.5f, 0);

            // Series of building shapes at varying heights
            float[] heights = { 3f, 4.5f, 2.5f, 5f, 3.5f, 2f, 4f, 3f, 5.5f, 2.8f, 3.8f, 4.2f };
            float[] widths  = { 1.5f, 1f, 1.8f, 0.8f, 1.3f, 2f, 1.1f, 1.6f, 0.9f, 1.4f, 1.2f, 1f };
            float x = -9f;

            for (int i = 0; i < heights.Length; i++)
            {
                var building = CreateSprite($"Building_{i}",
                    new Vector3(x, heights[i] * 0.5f - 1f, 0),
                    new Vector2(widths[i], heights[i]),
                    new Color(distantCity.r + Random.Range(-0.02f, 0.02f),
                              distantCity.g + Random.Range(-0.02f, 0.02f),
                              distantCity.b + Random.Range(-0.02f, 0.02f)),
                    -90);
                building.transform.SetParent(city.transform);

                // Tiny lit windows
                int windowCount = Random.Range(2, 6);
                for (int w = 0; w < windowCount; w++)
                {
                    var win = CreateSprite($"Win_{i}_{w}",
                        building.transform.position + new Vector3(
                            Random.Range(-widths[i] * 0.3f, widths[i] * 0.3f),
                            Random.Range(-heights[i] * 0.3f, heights[i] * 0.3f), 0),
                        new Vector2(0.08f, 0.1f),
                        new Color(1f, 0.9f, 0.5f, Random.Range(0.3f, 0.7f)),
                        -89);
                    win.transform.SetParent(building.transform);
                }

                x += widths[i] + Random.Range(0.1f, 0.4f);
            }

            return city;
        }

        private GameObject CreateMidgroundBuildings(Transform parent)
        {
            var mid = new GameObject("MidgroundStreet");
            mid.transform.SetParent(parent);
            mid.transform.position = new Vector3(0, -1f, 0);

            // Distant shop fronts
            Color shopColor = new Color(0.25f, 0.16f, 0.1f);
            float x = -8f;
            for (int i = 0; i < 6; i++)
            {
                float w = Random.Range(1.5f, 2.5f);
                float h = Random.Range(2f, 3.5f);
                var shop = CreateSprite($"Shop_{i}",
                    new Vector3(x, h * 0.5f - 0.5f, 0),
                    new Vector2(w, h),
                    new Color(shopColor.r + Random.Range(-0.05f, 0.05f),
                              shopColor.g + Random.Range(-0.03f, 0.03f),
                              shopColor.b + Random.Range(-0.03f, 0.03f)),
                    -50);
                shop.transform.SetParent(mid.transform);

                // Shop light glow
                var glow = CreateSprite($"ShopGlow_{i}",
                    new Vector3(x, -0.3f, 0),
                    new Vector2(w * 0.6f, 0.5f),
                    new Color(1f, 0.7f, 0.3f, 0.15f),
                    -49);
                glow.transform.SetParent(mid.transform);

                x += w + Random.Range(0.3f, 0.8f);
            }

            return mid;
        }

        // ═══════════════════════════════════════════════════════
        // GROUND & STREET
        // ═══════════════════════════════════════════════════════
        private void CreateStreetGround()
        {
            var ground = new GameObject("Ground");

            // Main road
            var road = CreateSprite("Road",
                new Vector3(0, groundY, 0),
                new Vector2(sceneWidth + 4, 3f),
                groundColor, -20);
            road.transform.SetParent(ground.transform);

            // Road texture variation
            var roadDark = CreateSprite("RoadShadow",
                new Vector3(0, groundY - 0.3f, 0),
                new Vector2(sceneWidth + 4, 1f),
                new Color(groundColor.r * 0.7f, groundColor.g * 0.7f, groundColor.b * 0.7f), -19);
            roadDark.transform.SetParent(ground.transform);

            // Sidewalk / edge
            var sidewalk = CreateSprite("Sidewalk",
                new Vector3(0, groundY + 1.2f, 0),
                new Vector2(sceneWidth + 4, 0.4f),
                new Color(0.55f, 0.45f, 0.35f), -18);
            sidewalk.transform.SetParent(ground.transform);

            // Collision for player
            var col = road.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        // ═══════════════════════════════════════════════════════
        // STALL STRUCTURE — Cart, awning, decorations
        // ═══════════════════════════════════════════════════════
        private void CreateStallStructure()
        {
            var stall = new GameObject("StreetStall");

            // Cart body
            var cart = CreateSprite("CartBody",
                new Vector3(0, counterY, 0),
                new Vector2(12f, 2.5f),
                cartWood, -5);
            cart.transform.SetParent(stall.transform);

            // Cart front face (darker)
            var cartFront = CreateSprite("CartFront",
                new Vector3(0, counterY - 0.8f, 0),
                new Vector2(12f, 0.8f),
                cartWoodDark, -4);
            cartFront.transform.SetParent(stall.transform);

            // Counter top (steel surface)
            var counter = CreateSprite("CounterTop",
                new Vector3(0, counterY + 1.1f, 0),
                new Vector2(12.2f, 0.15f),
                new Color(0.5f, 0.48f, 0.45f), -3);
            counter.transform.SetParent(stall.transform);

            // Cart wheels
            for (int i = 0; i < 3; i++)
            {
                float wx = -4f + i * 4f;
                var wheel = CreateSprite($"Wheel_{i}",
                    new Vector3(wx, counterY - 1.8f, 0),
                    new Vector2(0.8f, 0.8f),
                    new Color(0.3f, 0.3f, 0.3f), -3);
                wheel.transform.SetParent(stall.transform);

                var wheelInner = CreateSprite($"WheelHub_{i}",
                    new Vector3(wx, counterY - 1.8f, 0),
                    new Vector2(0.3f, 0.3f),
                    new Color(0.5f, 0.5f, 0.5f), -2);
                wheelInner.transform.SetParent(stall.transform);
            }

            // Awning (red & cream stripes)
            CreateAwning(stall.transform);

            // Awning poles
            var poleL = CreateSprite("PoleLeft",
                new Vector3(-6f, counterY + 2f, 0),
                new Vector2(0.12f, 4f),
                new Color(0.55f, 0.35f, 0.2f), -6);
            poleL.transform.SetParent(stall.transform);

            var poleR = CreateSprite("PoleRight",
                new Vector3(6f, counterY + 2f, 0),
                new Vector2(0.12f, 4f),
                new Color(0.55f, 0.35f, 0.2f), -6);
            poleR.transform.SetParent(stall.transform);

            // Marigold toran (garland across the top)
            CreateMarigoldToran(stall.transform);

            // Fairy lights string
            CreateFairyLights(stall.transform);

            // Hand-painted menu sign
            CreateMenuSign(stall.transform);

            // Masala jars on counter
            CreateMasalaJars(stall.transform);

            // Lemon garland
            CreateLemonGarland(stall.transform);
        }

        private void CreateAwning(Transform parent)
        {
            var awning = new GameObject("Awning");
            awning.transform.SetParent(parent);
            float awningY = counterY + stallHeight - 0.5f;

            int stripes = 14;
            float totalWidth = 13f;
            float stripeWidth = totalWidth / stripes;

            for (int i = 0; i < stripes; i++)
            {
                float x = -totalWidth / 2 + stripeWidth * i + stripeWidth / 2;
                var stripe = CreateSprite($"Stripe_{i}",
                    new Vector3(x, awningY, 0),
                    new Vector2(stripeWidth + 0.02f, 1f),
                    i % 2 == 0 ? stallRoof : stallRoofStripe, -7);
                stripe.transform.SetParent(awning.transform);
            }

            // Awning bottom scallop edge (triangular fringe)
            for (int i = 0; i < 20; i++)
            {
                float x = -totalWidth / 2 + (totalWidth / 20) * i + (totalWidth / 40);
                var fringe = CreateSprite($"Fringe_{i}",
                    new Vector3(x, awningY - 0.55f, 0),
                    new Vector2(0.3f, 0.25f),
                    i % 2 == 0 ? stallRoof : stallRoofStripe, -7);
                fringe.transform.SetParent(awning.transform);
            }
        }

        private void CreateMarigoldToran(Transform parent)
        {
            var toran = new GameObject("MarigoldToran");
            toran.transform.SetParent(parent);
            float toranY = counterY + stallHeight - 1.3f;

            // Garland beads
            for (int i = 0; i < 30; i++)
            {
                float t = i / 29f;
                float x = Mathf.Lerp(-5.5f, 5.5f, t);
                float droop = Mathf.Sin(t * Mathf.PI) * 0.4f;
                float smallWave = Mathf.Sin(t * Mathf.PI * 6) * 0.08f;

                var bead = CreateSprite($"Marigold_{i}",
                    new Vector3(x, toranY - droop - smallWave, 0),
                    new Vector2(0.2f, 0.2f),
                    i % 3 == 0 ? marigold : new Color(1f, 0.55f, 0f),
                    -1);
                bead.transform.SetParent(toran.transform);
            }
        }

        private void CreateFairyLights(Transform parent)
        {
            var lights = new GameObject("FairyLights");
            lights.transform.SetParent(parent);
            float lightY = counterY + stallHeight - 1.6f;

            Color[] colors = {
                new Color(1f, 0.42f, 0.21f),
                new Color(1f, 0.84f, 0f),
                new Color(1f, 0.27f, 0.27f),
                new Color(0.27f, 0.87f, 0.27f),
                new Color(1f, 0.65f, 0f),
                new Color(1f, 0.42f, 0.62f),
            };

            // String/wire
            var wire = CreateSprite("Wire",
                new Vector3(0, lightY + 0.1f, 0),
                new Vector2(11f, 0.03f),
                new Color(0.3f, 0.3f, 0.3f), 0);
            wire.transform.SetParent(lights.transform);

            for (int i = 0; i < 16; i++)
            {
                float t = i / 15f;
                float x = Mathf.Lerp(-5f, 5f, t);
                float droop = Mathf.Sin(t * Mathf.PI) * 0.15f;

                // Bulb
                var bulb = CreateSprite($"Bulb_{i}",
                    new Vector3(x, lightY - droop, 0),
                    new Vector2(0.15f, 0.2f),
                    colors[i % colors.Length], 1);
                bulb.transform.SetParent(lights.transform);
                _fairyLightRenderers.Add(bulb.GetComponent<SpriteRenderer>());

                // Glow around bulb
                var glow = CreateSprite($"BulbGlow_{i}",
                    new Vector3(x, lightY - droop, 0),
                    new Vector2(0.6f, 0.6f),
                    new Color(colors[i % colors.Length].r,
                              colors[i % colors.Length].g,
                              colors[i % colors.Length].b, 0.12f),
                    0);
                glow.transform.SetParent(lights.transform);
            }
        }

        private void CreateMenuSign(Transform parent)
        {
            var sign = new GameObject("MenuSign");
            sign.transform.SetParent(parent);
            sign.transform.position = new Vector3(-5.5f, counterY + 2.5f, 0);

            // Board
            var board = CreateSprite("Board",
                sign.transform.position,
                new Vector2(2f, 1.5f),
                new Color(0.18f, 0.22f, 0.18f), 2);
            board.transform.SetParent(sign.transform);

            // Board frame
            var frame = CreateSprite("Frame",
                sign.transform.position,
                new Vector2(2.15f, 1.65f),
                cartWood, 1);
            frame.transform.SetParent(sign.transform);
        }

        private void CreateMasalaJars(Transform parent)
        {
            var jars = new GameObject("MasalaJars");
            jars.transform.SetParent(parent);

            Color[] spiceColors = {
                tandoori,                           // chili powder
                turmeric,                           // haldi
                new Color(0.6f, 0.45f, 0.2f),      // garam masala
                new Color(0.15f, 0.5f, 0.15f),      // green chutney
                new Color(0.75f, 0.55f, 0.3f),      // cumin
            };

            for (int i = 0; i < spiceColors.Length; i++)
            {
                float x = 4f + i * 0.5f;
                var jar = CreateSprite($"Jar_{i}",
                    new Vector3(x, counterY + 1.5f, 0),
                    new Vector2(0.35f, 0.5f),
                    spiceColors[i], 5);
                jar.transform.SetParent(jars.transform);

                // Jar rim
                var rim = CreateSprite($"JarRim_{i}",
                    new Vector3(x, counterY + 1.78f, 0),
                    new Vector2(0.38f, 0.08f),
                    new Color(spiceColors[i].r * 0.7f, spiceColors[i].g * 0.7f, spiceColors[i].b * 0.7f), 6);
                rim.transform.SetParent(jars.transform);
            }
        }

        private void CreateLemonGarland(Transform parent)
        {
            var garland = new GameObject("LemonGarland");
            garland.transform.SetParent(parent);

            for (int i = 0; i < 12; i++)
            {
                float x = -5f + i * 0.85f;
                var lemon = CreateSprite($"Lemon_{i}",
                    new Vector3(x, counterY + 0.6f, 0),
                    new Vector2(0.2f, 0.25f),
                    i % 3 == 0 ? new Color(0.2f, 0.7f, 0.15f) : new Color(0.95f, 0.9f, 0.2f),
                    4);
                lemon.transform.SetParent(garland.transform);
            }
        }

        // ═══════════════════════════════════════════════════════
        // COOKING STATIONS — On the counter
        // ═══════════════════════════════════════════════════════
        private void CreateCookingStations()
        {
            var parent = new GameObject("CookingStations");
            float[] xPositions = { -3.5f, -1f, 1.5f, 4f };

            for (int i = 0; i < stationNames.Length && i < xPositions.Length; i++)
            {
                var station = new GameObject(stationNames[i]);
                station.transform.position = new Vector3(xPositions[i], counterY + 1.5f, 0);
                station.transform.SetParent(parent.transform);

                // Station visual (tawa, pot, board, plate)
                var sr = station.AddComponent<SpriteRenderer>();
                sr.color = GetStationColor(stationTypes[i]);
                sr.sortingOrder = 8;

                // Station base/shadow
                var shadow = CreateSprite($"{stationNames[i]}_shadow",
                    new Vector3(xPositions[i], counterY + 1.25f, 0),
                    new Vector2(1.6f, 0.15f),
                    new Color(0, 0, 0, 0.2f), 7);
                shadow.transform.SetParent(station.transform);

                // Interaction collider
                var col = station.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.8f, 1.5f);
                col.isTrigger = true;

                // Ingredient slot
                var slot = new GameObject("IngredientSlot");
                slot.transform.SetParent(station.transform);
                slot.transform.localPosition = new Vector3(0, 0.5f, 0);

                station.AddComponent<CookingStation>();

                // Steam point for ambience
                var steamPoint = new GameObject("SteamPoint");
                steamPoint.transform.SetParent(station.transform);
                steamPoint.transform.localPosition = new Vector3(0, 0.6f, 0);
                _steamPoints.Add(steamPoint.transform);

                // Fire point (only for stove/chulha)
                if (stationTypes[i] == CookingStep.StepType.Fry ||
                    stationTypes[i] == CookingStep.StepType.Boil)
                {
                    var firePoint = new GameObject("FirePoint");
                    firePoint.transform.SetParent(station.transform);
                    firePoint.transform.localPosition = new Vector3(0, -0.3f, 0);
                    _firePoints.Add(firePoint.transform);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        // SUPPLY STATIONS — Ingredient crates below counter
        // ═══════════════════════════════════════════════════════
        private void CreateSupplyStations()
        {
            var parent = new GameObject("SupplyStations");
            float[] xPositions = { -4.5f, -2f, 0.5f };
            Color crateColor = new Color(0.45f, 0.32f, 0.18f);

            for (int i = 0; i < xPositions.Length; i++)
            {
                var supply = new GameObject($"Supply_{i}");
                supply.transform.position = new Vector3(xPositions[i], counterY - 0.5f, 0);
                supply.transform.SetParent(parent.transform);

                // Wooden crate
                var crate = CreateSprite($"Crate_{i}",
                    supply.transform.position,
                    new Vector2(1.2f, 0.9f),
                    new Color(crateColor.r + Random.Range(-0.05f, 0.05f),
                              crateColor.g + Random.Range(-0.03f, 0.03f),
                              crateColor.b + Random.Range(-0.03f, 0.03f)),
                    3);
                crate.transform.SetParent(supply.transform);

                // Crate rim
                var rim = CreateSprite($"CrateRim_{i}",
                    supply.transform.position + Vector3.up * 0.4f,
                    new Vector2(1.25f, 0.1f),
                    new Color(crateColor.r * 0.8f, crateColor.g * 0.8f, crateColor.b * 0.8f),
                    4);
                rim.transform.SetParent(supply.transform);

                supply.AddComponent<BoxCollider2D>().isTrigger = true;
                supply.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1.2f);
                supply.AddComponent<IngredientSpawner>();
            }
        }

        // ═══════════════════════════════════════════════════════
        // CUSTOMER AREA — Stools in front of stall
        // ═══════════════════════════════════════════════════════
        private void CreateCustomerArea()
        {
            var parent = new GameObject("CustomerArea");
            float[] xPositions = { -3f, 0f, 3f };

            for (int i = 0; i < xPositions.Length; i++)
            {
                var seat = new GameObject($"Customer_{i}");
                seat.transform.position = new Vector3(xPositions[i], groundY + 1.5f, 0);
                seat.transform.SetParent(parent.transform);

                // Plastic stool (typical Indian street stall)
                var stool = CreateSprite($"Stool_{i}",
                    new Vector3(xPositions[i], groundY + 0.8f, 0),
                    new Vector2(0.6f, 0.5f),
                    new Color(0.2f, 0.4f, 0.7f), 3);
                stool.transform.SetParent(seat.transform);

                // Order bubble
                var bubble = new GameObject("OrderBubble");
                bubble.transform.SetParent(seat.transform);
                bubble.transform.localPosition = new Vector3(0.6f, 1.5f, 0);
                var bubbleSr = bubble.AddComponent<SpriteRenderer>();
                bubbleSr.color = new Color(1f, 1f, 1f, 0.85f);
                bubbleSr.sortingOrder = 15;

                seat.AddComponent<Customer>();
            }
        }

        // ═══════════════════════════════════════════════════════
        // CHEF CHARACTER
        // ═══════════════════════════════════════════════════════
        private void CreateChef()
        {
            GameObject chef;
            if (chefPrefab != null)
            {
                chef = Instantiate(chefPrefab, new Vector3(0, counterY + 2f, 0), Quaternion.identity);
            }
            else
            {
                chef = new GameObject("Chef");
                chef.transform.position = new Vector3(0, counterY + 2f, 0);

                // Body
                var body = chef.AddComponent<SpriteRenderer>();
                body.color = new Color(0.95f, 0.85f, 0.7f); // skin tone
                body.sortingOrder = 12;

                // Kurta-apron (child sprite)
                var kurta = new GameObject("Kurta");
                kurta.transform.SetParent(chef.transform);
                kurta.transform.localPosition = new Vector3(0, -0.15f, 0);
                var kurtaSr = kurta.AddComponent<SpriteRenderer>();
                kurtaSr.color = new Color(0.95f, 0.95f, 0.98f); // white kurta
                kurtaSr.sortingOrder = 13;

                // Bandana (instead of toque)
                var bandana = new GameObject("Bandana");
                bandana.transform.SetParent(chef.transform);
                bandana.transform.localPosition = new Vector3(0, 0.55f, 0);
                var bandanaSr = bandana.AddComponent<SpriteRenderer>();
                bandanaSr.color = tandoori;
                bandanaSr.sortingOrder = 14;

                // Expression overlay
                var expression = new GameObject("Expression");
                expression.transform.SetParent(chef.transform);
                expression.transform.localPosition = new Vector3(0, 0.3f, 0);
                var exprSr = expression.AddComponent<SpriteRenderer>();
                exprSr.enabled = false;
                exprSr.sortingOrder = 15;

                // Physics
                var rb = chef.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.freezeRotation = true;

                var col = chef.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.7f, 1f);

                chef.AddComponent<Core.PlayerController>();

                // Animation controller
                var animCtrl = chef.AddComponent<Core.ChefAnimationController>();
            }
            chef.name = "Chef";
            chef.tag = "Player";
        }

        // ═══════════════════════════════════════════════════════
        // AMBIENCE — Particles & fairy lights
        // ═══════════════════════════════════════════════════════
        private void CreateAmbience()
        {
            var ambienceObj = new GameObject("KitchenAmbience");
            _ambience = ambienceObj.AddComponent<KitchenAmbience>();
            _ambience.SetEffectPoints(_steamPoints.ToArray(), _firePoints.ToArray());
            _ambience.SetFairyLights(_fairyLightRenderers.ToArray());
        }

        // ═══════════════════════════════════════════════════════
        // MANAGERS
        // ═══════════════════════════════════════════════════════
        private void CreateManagers()
        {
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

        // ═══════════════════════════════════════════════════════
        // CAMERA — Side-view framing
        // ═══════════════════════════════════════════════════════
        private void CreateCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.transform.position = new Vector3(0, 0.5f, -10);
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.08f, 0.04f, 0.1f);
        }

        // ═══════════════════════════════════════════════════════
        // UI CANVAS
        // ═══════════════════════════════════════════════════════
        private void CreateUI()
        {
            var canvasObj = new GameObject("GameCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<ChalkboardUI>();
        }

        // ═══════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════
        private void CreateSortingLayers()
        {
            Debug.Log("[KitchenSetup] Note: Manually add sorting layers in Edit > Project Settings > Tags & Layers:");
            Debug.Log("  Background (-100), Midground (-50), Stall (-5), Gameplay (0), Characters (10), FX (20), UI (30)");
        }

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

        private GameObject CreateGradientQuad(string name, float width, float height,
            Color topColor, Color bottomColor, int sortOrder, Transform parent)
        {
            // Unity SpriteRenderer doesn't natively support gradients,
            // so we approximate with 2 overlapping sprites
            var container = new GameObject(name);
            container.transform.SetParent(parent);

            var top = CreateSprite($"{name}_Top",
                new Vector3(0, height * 0.25f, 0),
                new Vector2(width, height * 0.5f),
                topColor, sortOrder);
            top.transform.SetParent(container.transform);

            var bottom = CreateSprite($"{name}_Bottom",
                new Vector3(0, -height * 0.25f, 0),
                new Vector2(width, height * 0.5f),
                bottomColor, sortOrder);
            bottom.transform.SetParent(container.transform);

            return container;
        }

        private Color GetStationColor(CookingStep.StepType type)
        {
            return type switch
            {
                CookingStep.StepType.Chop => new Color(0.7f, 0.6f, 0.4f),    // wooden board
                CookingStep.StepType.Fry  => tawaColor,                        // cast iron tawa
                CookingStep.StepType.Boil => new Color(0.55f, 0.55f, 0.6f),   // steel pot
                CookingStep.StepType.Bake => new Color(0.4f, 0.28f, 0.18f),   // clay tandoor
                CookingStep.StepType.Mix  => new Color(0.65f, 0.65f, 0.7f),   // steel bowl
                CookingStep.StepType.Plate => new Color(0.9f, 0.88f, 0.85f),  // steel thali
                CookingStep.StepType.Garnish => new Color(0.35f, 0.6f, 0.25f),// herb
                _ => Color.gray
            };
        }
    }
}
