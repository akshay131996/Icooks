using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Gallery view for browsing all manga arcs and chapters.
    /// Displays chapter thumbnails in a grid, with lock states
    /// and progress tracking per arc.
    /// </summary>
    public class MangaGalleryUI : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Inspector
        // ──────────────────────────────────────────────

        [Header("Root")]
        [SerializeField] private GameObject galleryRoot;

        [Header("Arc Tabs")]
        [SerializeField] private Transform arcTabContainer;
        [SerializeField] private GameObject arcTabPrefab;

        [Header("Chapter Grid")]
        [SerializeField] private Transform chapterGridContainer;
        [SerializeField] private GameObject chapterCardPrefab;

        [Header("Arc Info")]
        [SerializeField] private TextMeshProUGUI arcTitleText;
        [SerializeField] private TextMeshProUGUI arcDescriptionText;
        [SerializeField] private Slider arcProgressBar;
        [SerializeField] private TextMeshProUGUI arcProgressText;

        [Header("Chapter Card Elements (inside prefab)")]
        [SerializeField] private Color unlockedCardColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color lockedCardColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color readCardColor = new Color(0.85f, 0.95f, 0.85f, 1f);

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        [Header("References")]
        [SerializeField] private MangaReaderUI mangaReader;

        // ──────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────

        private int _selectedArcId = 1;
        private List<GameObject> _arcTabInstances = new List<GameObject>();
        private List<GameObject> _chapterCardInstances = new List<GameObject>();

        // Arc metadata
        private static readonly string[] ArcNames = {
            "The Boy and the Smoke",
            "The Trails He Left Behind",
            "Storms at Sea",
            "Embers in the Snow",
            "The Smile Behind the Smoke"
        };

        private static readonly string[] ArcDescriptions = {
            "Southern India — A young boy meets a legendary, smiling chef.",
            "Across India — Following the trails and tales left behind.",
            "Southeast Asia — Storms reveal the master's darker past.",
            "Europe & Middle East — Competition, rivalry, and buried truths.",
            "Grand Finale — The smile behind the smoke is finally revealed."
        };

        private static readonly string[] ArcEmojis = { "🇮🇳", "🇮🇳", "🌏", "🌍", "🌟" };

        // ──────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (galleryRoot != null)
                galleryRoot.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            if (MangaManager.Instance != null)
                MangaManager.Instance.OnMangaProgressChanged += RefreshCurrentArc;
        }

        private void OnDisable()
        {
            if (MangaManager.Instance != null)
                MangaManager.Instance.OnMangaProgressChanged -= RefreshCurrentArc;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        // ──────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────

        /// <summary>Open the gallery, optionally jumping to a specific arc.</summary>
        public void Open(int startingArcId = 1)
        {
            if (galleryRoot != null)
                galleryRoot.SetActive(true);

            BuildArcTabs();
            SelectArc(Mathf.Clamp(startingArcId, 1, 5));
        }

        /// <summary>Close the gallery.</summary>
        public void Close()
        {
            if (galleryRoot != null)
                galleryRoot.SetActive(false);
        }

        /// <summary>Check if the gallery is open.</summary>
        public bool IsOpen => galleryRoot != null && galleryRoot.activeSelf;

        // ──────────────────────────────────────────────
        // Arc Tabs
        // ──────────────────────────────────────────────

        private void BuildArcTabs()
        {
            // Clear existing tabs
            foreach (var tab in _arcTabInstances)
            {
                if (tab != null) Destroy(tab);
            }
            _arcTabInstances.Clear();

            if (arcTabContainer == null || arcTabPrefab == null) return;

            for (int i = 1; i <= 5; i++)
            {
                int arcId = i; // Capture for closure
                GameObject tab = Instantiate(arcTabPrefab, arcTabContainer);

                // Setup tab text
                var tabText = tab.GetComponentInChildren<TextMeshProUGUI>();
                if (tabText != null)
                    tabText.text = $"{ArcEmojis[i - 1]} Arc {i}";

                // Setup tab button
                var tabButton = tab.GetComponent<Button>();
                if (tabButton != null)
                    tabButton.onClick.AddListener(() => SelectArc(arcId));

                // Show progress indicator
                float progress = MangaManager.Instance != null
                    ? MangaManager.Instance.GetArcProgress(arcId)
                    : 0f;

                // Visual indicator for completed arcs
                var tabImage = tab.GetComponent<Image>();
                if (tabImage != null && progress >= 1f)
                    tabImage.color = new Color(1f, 0.85f, 0.4f, 1f); // Gold for complete

                _arcTabInstances.Add(tab);
            }
        }

        private void SelectArc(int arcId)
        {
            _selectedArcId = arcId;

            // Update arc info header
            int index = Mathf.Clamp(arcId - 1, 0, ArcNames.Length - 1);

            if (arcTitleText != null)
                arcTitleText.text = $"Arc {arcId}: {ArcNames[index]}";

            if (arcDescriptionText != null)
                arcDescriptionText.text = ArcDescriptions[index];

            // Update progress
            UpdateArcProgress(arcId);

            // Highlight selected tab
            UpdateTabHighlights(arcId);

            // Build chapter cards
            BuildChapterGrid(arcId);
        }

        // ──────────────────────────────────────────────
        // Chapter Grid
        // ──────────────────────────────────────────────

        private void BuildChapterGrid(int arcId)
        {
            // Clear existing cards
            foreach (var card in _chapterCardInstances)
            {
                if (card != null) Destroy(card);
            }
            _chapterCardInstances.Clear();

            if (chapterGridContainer == null || chapterCardPrefab == null) return;

            var chapters = MangaManager.Instance?.GetChaptersForArc(arcId);
            if (chapters == null) return;

            foreach (var chapter in chapters)
            {
                GameObject card = Instantiate(chapterCardPrefab, chapterGridContainer);
                SetupChapterCard(card, chapter);
                _chapterCardInstances.Add(card);
            }
        }

        private void SetupChapterCard(GameObject card, MangaChapter chapter)
        {
            if (card == null || chapter == null) return;

            bool isUnlocked = MangaManager.Instance != null
                && MangaManager.Instance.IsChapterUnlocked(chapter.ChapterId);
            bool isRead = MangaManager.Instance != null
                && MangaManager.Instance.IsChapterRead(chapter.ChapterId);

            // Card background color
            var cardImage = card.GetComponent<Image>();
            if (cardImage != null)
            {
                if (isRead)
                    cardImage.color = readCardColor;
                else if (isUnlocked)
                    cardImage.color = unlockedCardColor;
                else
                    cardImage.color = lockedCardColor;
            }

            // Thumbnail
            var thumbnailImage = card.transform.Find("Thumbnail")?.GetComponent<Image>();
            if (thumbnailImage != null)
            {
                if (isUnlocked && chapter.galleryThumbnail != null)
                {
                    thumbnailImage.sprite = chapter.galleryThumbnail;
                    thumbnailImage.color = Color.white;
                }
                else
                {
                    // Show silhouette / locked state
                    thumbnailImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                }
            }

            // Title text
            var titleText = card.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = isUnlocked
                    ? chapter.chapterTitle
                    : $"Chapter {chapter.chapterNumber}";
            }

            // Status text
            var statusText = card.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                if (isRead)
                    statusText.text = "✓ Read";
                else if (isUnlocked)
                    statusText.text = "NEW — Tap to read!";
                else
                    statusText.text = $"🔒 Complete Level {chapter.unlockAtLevel}";
            }

            // Lock icon
            var lockIcon = card.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
                lockIcon.SetActive(!isUnlocked);

            // New badge
            var newBadge = card.transform.Find("NewBadge")?.gameObject;
            if (newBadge != null)
                newBadge.SetActive(isUnlocked && !isRead);

            // Click handler
            var button = card.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = isUnlocked;
                MangaChapter capturedChapter = chapter; // Closure capture
                button.onClick.AddListener(() => OnChapterCardClicked(capturedChapter));
            }
        }

        // ──────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────

        private void OnChapterCardClicked(MangaChapter chapter)
        {
            if (chapter == null || mangaReader == null) return;

            if (MangaManager.Instance != null && MangaManager.Instance.IsChapterUnlocked(chapter.ChapterId))
            {
                // Hide gallery and open reader
                if (galleryRoot != null)
                    galleryRoot.SetActive(false);

                mangaReader.OpenChapter(chapter);
            }
        }

        // ──────────────────────────────────────────────
        // UI Updates
        // ──────────────────────────────────────────────

        private void UpdateArcProgress(int arcId)
        {
            float progress = MangaManager.Instance != null
                ? MangaManager.Instance.GetArcProgress(arcId)
                : 0f;

            if (arcProgressBar != null)
                arcProgressBar.value = progress;

            if (arcProgressText != null)
            {
                var chapters = MangaManager.Instance?.GetChaptersForArc(arcId);
                int total = chapters?.Count ?? 5;
                int unlocked = Mathf.RoundToInt(progress * total);
                arcProgressText.text = $"{unlocked} / {total} chapters unlocked";
            }
        }

        private void UpdateTabHighlights(int selectedArcId)
        {
            for (int i = 0; i < _arcTabInstances.Count; i++)
            {
                var tab = _arcTabInstances[i];
                if (tab == null) continue;

                var outline = tab.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = (i + 1 == selectedArcId);

                // Scale selected tab slightly larger
                tab.transform.localScale = (i + 1 == selectedArcId)
                    ? Vector3.one * 1.1f
                    : Vector3.one;
            }
        }

        private void RefreshCurrentArc()
        {
            if (IsOpen)
                SelectArc(_selectedArcId);
        }
    }
}
