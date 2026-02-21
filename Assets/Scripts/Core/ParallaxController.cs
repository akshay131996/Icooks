using UnityEngine;

namespace ChefJourney.Core
{
    /// <summary>
    /// Multi-layer parallax scrolling for side-view depth.
    /// Each child layer scrolls at a different speed relative to the camera,
    /// creating a convincing 3D depth effect in 2D.
    /// </summary>
    public class ParallaxController : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public string name;
            public Transform layerTransform;
            [Range(0f, 1f)] public float parallaxFactor; // 0 = static, 1 = moves with camera
            public bool infiniteScrollX;
            public bool infiniteScrollY;
            [HideInInspector] public Vector3 startPos;
            [HideInInspector] public float spriteWidth;
            [HideInInspector] public float spriteHeight;
        }

        [Header("References")]
        [SerializeField] private Camera cam;

        [Header("Layers")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Subtle Drift")]
        [SerializeField] private bool enableDriftAnimation = true;
        [SerializeField] private float driftSpeed = 0.1f;
        [SerializeField] private float driftAmount = 0.5f;

        private Vector3 _lastCamPos;
        private float _driftTimer;

        private void Start()
        {
            if (cam == null) cam = Camera.main;
            _lastCamPos = cam.transform.position;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;
                layer.startPos = layer.layerTransform.position;

                var sr = layer.layerTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    layer.spriteWidth = sr.bounds.size.x;
                    layer.spriteHeight = sr.bounds.size.y;
                }
            }
        }

        private void LateUpdate()
        {
            if (cam == null) return;

            Vector3 camDelta = cam.transform.position - _lastCamPos;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;

                float moveX = camDelta.x * layer.parallaxFactor;
                float moveY = camDelta.y * layer.parallaxFactor;

                Vector3 newPos = layer.layerTransform.position;
                newPos.x += moveX;
                newPos.y += moveY;

                layer.layerTransform.position = newPos;

                // Infinite scrolling — wrap layer when it moves past bounds
                if (layer.infiniteScrollX && layer.spriteWidth > 0)
                {
                    float relativeX = cam.transform.position.x * (1 - layer.parallaxFactor);
                    float dist = cam.transform.position.x - layer.layerTransform.position.x;
                    if (Mathf.Abs(dist) >= layer.spriteWidth)
                    {
                        layer.layerTransform.position = new Vector3(
                            layer.layerTransform.position.x + layer.spriteWidth * Mathf.Sign(dist),
                            layer.layerTransform.position.y,
                            layer.layerTransform.position.z
                        );
                    }
                }
            }

            _lastCamPos = cam.transform.position;

            // Subtle ambient camera drift for atmosphere
            if (enableDriftAnimation)
            {
                _driftTimer += Time.deltaTime * driftSpeed;
                float driftX = Mathf.Sin(_driftTimer) * driftAmount * 0.01f;
                float driftY = Mathf.Cos(_driftTimer * 0.7f) * driftAmount * 0.005f;
                cam.transform.position += new Vector3(driftX, driftY, 0);
            }
        }

        /// <summary>
        /// Programmatically create parallax layers if not set in the inspector.
        /// Called by KitchenSetup during scene bootstrap.
        /// </summary>
        public void SetupLayers(ParallaxLayer[] newLayers)
        {
            layers = newLayers;
            if (cam == null) cam = Camera.main;
            _lastCamPos = cam.transform.position;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;
                layer.startPos = layer.layerTransform.position;

                var sr = layer.layerTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    layer.spriteWidth = sr.bounds.size.x;
                    layer.spriteHeight = sr.bounds.size.y;
                }
            }
        }
    }
}
