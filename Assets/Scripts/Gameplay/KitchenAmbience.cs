using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// Manages ambient visual effects for the kitchen scene:
    /// steam, fire, spice dust particles, and fairy light flickering.
    /// Attach to a root GameObject in the kitchen scene.
    /// </summary>
    public class KitchenAmbience : MonoBehaviour
    {
        [Header("Particle Prefabs (will create defaults if null)")]
        [SerializeField] private ParticleSystem steamPrefab;
        [SerializeField] private ParticleSystem firePrefab;
        [SerializeField] private ParticleSystem sparksPrefab;
        [SerializeField] private ParticleSystem dustPrefab;

        [Header("Spawn Positions")]
        [SerializeField] private Transform[] steamPoints;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private Transform dustArea;

        [Header("Fairy Lights")]
        [SerializeField] private SpriteRenderer[] fairyLights;
        [SerializeField] private Color[] lightColors = new Color[]
        {
            new Color(1f, 0.42f, 0.21f),    // orange
            new Color(1f, 0.84f, 0f),        // gold
            new Color(1f, 0.27f, 0.27f),     // red
            new Color(0.27f, 0.87f, 0.27f),  // green
            new Color(1f, 0.65f, 0f),        // amber
            new Color(1f, 0.42f, 0.62f),     // pink
        };
        [SerializeField] private float flickerSpeed = 3f;
        [SerializeField] private float flickerIntensity = 0.3f;

        private float[] _flickerOffsets;

        private void Start()
        {
            SetupParticles();
            InitializeFairyLights();
        }

        private void Update()
        {
            AnimateFairyLights();
        }

        // ─── Particles ─────────────────────────────────────────

        private void SetupParticles()
        {
            // Steam at cooking stations
            if (steamPoints != null)
            {
                foreach (var point in steamPoints)
                {
                    if (point == null) continue;
                    var steam = CreateSteamSystem(point.position);
                    steam.transform.SetParent(point);
                }
            }

            // Fire under stoves
            if (firePoints != null)
            {
                foreach (var point in firePoints)
                {
                    if (point == null) continue;
                    var fire = CreateFireSystem(point.position);
                    fire.transform.SetParent(point);
                }
            }

            // Ambient spice dust in the air
            if (dustArea != null)
            {
                var dust = CreateDustSystem(dustArea.position);
                dust.transform.SetParent(dustArea);
            }
            else
            {
                // Create dust across the whole scene
                var dust = CreateDustSystem(Vector3.zero);
                dust.transform.SetParent(transform);
            }
        }

        private ParticleSystem CreateSteamSystem(Vector3 position)
        {
            if (steamPrefab != null)
                return Instantiate(steamPrefab, position, Quaternion.identity);

            var go = new GameObject("Steam");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startColor = new Color(1f, 1f, 1f, 0.15f);
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 0.5f, 1, 1.5f));

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(0.05f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 20;

            return ps;
        }

        private ParticleSystem CreateFireSystem(Vector3 position)
        {
            if (firePrefab != null)
                return Instantiate(firePrefab, position, Quaternion.identity);

            var go = new GameObject("Fire");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 25f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.1f), 0.5f),
                    new GradientColorKey(new Color(0.5f, 0.1f, 0.05f), 1f)
                },
                new[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0.5f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 19;

            return ps;
        }

        private ParticleSystem CreateDustSystem(Vector3 position)
        {
            if (dustPrefab != null)
                return Instantiate(dustPrefab, position, Quaternion.identity);

            var go = new GameObject("SpiceDust");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 8f, 0f);

            // Warm golden dust color
            var startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.85f, 0.4f, 0.15f),
                new Color(0.9f, 0.7f, 0.3f, 0.1f)
            );
            main.startColor = startColor;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.85f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.85f, 0.5f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.12f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = grad;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 25;

            return ps;
        }

        // ─── Fairy Lights ──────────────────────────────────────

        private void InitializeFairyLights()
        {
            if (fairyLights == null || fairyLights.Length == 0) return;

            _flickerOffsets = new float[fairyLights.Length];
            for (int i = 0; i < fairyLights.Length; i++)
            {
                _flickerOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
                if (fairyLights[i] != null && i < lightColors.Length)
                    fairyLights[i].color = lightColors[i % lightColors.Length];
            }
        }

        private void AnimateFairyLights()
        {
            if (fairyLights == null || _flickerOffsets == null) return;

            for (int i = 0; i < fairyLights.Length; i++)
            {
                if (fairyLights[i] == null) continue;

                float flicker = 1f - flickerIntensity + Mathf.Sin(Time.time * flickerSpeed + _flickerOffsets[i]) * flickerIntensity;
                Color baseColor = lightColors[i % lightColors.Length];
                fairyLights[i].color = new Color(
                    baseColor.r * flicker,
                    baseColor.g * flicker,
                    baseColor.b * flicker,
                    baseColor.a
                );
            }
        }

        // ─── Public API ────────────────────────────────────────

        /// <summary>
        /// Spawn a burst of sizzle sparks at a position (e.g. when cooking starts).
        /// </summary>
        public void PlaySizzleSparks(Vector3 position)
        {
            if (sparksPrefab != null)
            {
                var sparks = Instantiate(sparksPrefab, position, Quaternion.identity);
                Destroy(sparks.gameObject, 2f);
                return;
            }

            var go = new GameObject("Sizzle");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new Color(1f, 0.8f, 0.2f);
            main.maxParticles = 30;
            main.duration = 0.3f;
            main.loop = false;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 22;

            Destroy(go, 2f);
        }

        /// <summary>
        /// Set fairy light references programmatically.
        /// </summary>
        public void SetFairyLights(SpriteRenderer[] lights)
        {
            fairyLights = lights;
            InitializeFairyLights();
        }

        /// <summary>
        /// Set steam/fire spawn points programmatically.
        /// </summary>
        public void SetEffectPoints(Transform[] steam, Transform[] fire)
        {
            steamPoints = steam;
            firePoints = fire;
        }
    }
}
