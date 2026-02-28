using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.Testing
{
    /// <summary>
    /// A runtime UI builder that bypasses broken prefabs and creates
    /// a guaranteed-visible Canvas with TextMeshPro text for debugging.
    /// </summary>
    public class RuntimeMangaTester : MonoBehaviour
    {
        private GameObject _canvasObj;
        private TextMeshProUGUI _statusText;

        private void Start()
        {
            CreateDebugUI();
            
            // Subscribe here instead of OnEnable to avoid Awake() race conditions
            if (Manga.MangaManager.Instance != null)
            {
                Manga.MangaManager.Instance.OnChapterUnlocked += HandleChapterUnlocked;
            }
        }
        
        private void OnDestroy()
        {
            if (Manga.MangaManager.Instance != null)
            {
                Manga.MangaManager.Instance.OnChapterUnlocked -= HandleChapterUnlocked;
            }
        }

        private void CreateDebugUI()
        {
            // Create Canvas
            _canvasObj = new GameObject("DebugRuntimeCanvas");
            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure it's on top of everything
            _canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasObj.AddComponent<GraphicRaycaster>();

            // Create Background
            GameObject bgObj = new GameObject("DebugBackground");
            bgObj.transform.SetParent(_canvasObj.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.sizeDelta = Vector2.zero;

            // Create Text Box
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(_canvasObj.transform, false);
            _statusText = textObj.AddComponent<TextMeshProUGUI>();
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            
            _statusText.text = "Debug Canvas Ready!\nWaiting for Manga Events...";
            _statusText.color = Color.white;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.fontSize = 48;
            
            textRect.anchorMin = new Vector2(0.1f, 0.1f);
            textRect.anchorMax = new Vector2(0.9f, 0.9f);
            textRect.sizeDelta = Vector2.zero;

            // Create Close Button
            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(_canvasObj.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.red;
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.1f);
            btnRect.anchorMax = new Vector2(0.5f, 0.1f);
            btnRect.sizeDelta = new Vector2(200, 80);
            btnRect.anchoredPosition = new Vector2(0, 50);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => _canvasObj.SetActive(false));

            GameObject btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Close";
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 24;
            btnText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            btnText.GetComponent<RectTransform>().anchorMax = Vector2.one;
            btnText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            _canvasObj.SetActive(false); // Hide until event fires
        }

        private void HandleChapterUnlocked(Manga.MangaChapter chapter)
        {
            if (_canvasObj != null && _statusText != null)
            {
                _canvasObj.SetActive(true);
                _statusText.text = $"EVENT FIRED!\n\nUnlocked Chapter:\n{chapter.chapterTitle}\n\nArc {chapter.arcId} - Level {chapter.unlockAtLevel}";
            }
            else
            {
                Debug.LogError("[RuntimeMangaTester] Canvas or text is null!");
            }
        }
    }
}
