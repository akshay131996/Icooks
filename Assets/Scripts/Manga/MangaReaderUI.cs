using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Full-screen manga reading experience with page-turning,
    /// panel transitions, and text display. Attach to a dedicated Canvas.
    /// </summary>
    public class MangaReaderUI : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Inspector — UI References
        // ──────────────────────────────────────────────

        [Header("Containers")]
        [SerializeField] private GameObject readerRoot;
        [SerializeField] private RectTransform panelContainer;
        [SerializeField] private Image backgroundImage;

        [Header("Panel Display")]
        [SerializeField] private Image panelImageDisplay;
        [SerializeField] private TextMeshProUGUI narratorTextDisplay;
        [SerializeField] private TextMeshProUGUI dialogueTextDisplay;
        [SerializeField] private TextMeshProUGUI speakerNameDisplay;
        [SerializeField] private GameObject speechBubble;
        [SerializeField] private GameObject narratorBox;

        [Header("Navigation")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button nextChapterButton;
        [SerializeField] private TextMeshProUGUI pageCounter;

        [Header("Chapter Header")]
        [SerializeField] private TextMeshProUGUI chapterTitleText;
        [SerializeField] private TextMeshProUGUI arcTitleText;
        [SerializeField] private TextMeshProUGUI settingText;

        [Header("Transition Effects")]
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private Animator pageAnimator;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip pageFlipSound;
        [SerializeField] private AudioClip dramaticRevealSound;

        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.03f;
        [SerializeField] private bool useTypewriterEffect = true;

        // ──────────────────────────────────────────────
        // Runtime State
        // ──────────────────────────────────────────────

        private MangaChapter _currentChapter;
        private int _currentPageIndex;
        private int _currentPanelIndex;
        private List<MangaPanel> _currentPagePanels;
        private bool _isTransitioning;
        private Coroutine _typewriterCoroutine;

        // Arc display names
        private static readonly string[] ArcNames = {
            "The Boy and the Smoke",
            "The Trails He Left Behind",
            "Storms at Sea",
            "Embers in the Snow",
            "The Smile Behind the Smoke"
        };

        // ──────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (readerRoot != null)
                readerRoot.SetActive(false);

            // Wire up buttons
            if (nextButton != null) nextButton.onClick.AddListener(NextPanel);
            if (prevButton != null) prevButton.onClick.AddListener(PrevPanel);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (nextChapterButton != null) nextChapterButton.onClick.AddListener(GoToNextChapter);
        }

        private void OnDestroy()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(NextPanel);
            if (prevButton != null) prevButton.onClick.RemoveListener(PrevPanel);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (nextChapterButton != null) nextChapterButton.onClick.RemoveListener(GoToNextChapter);
        }

        // ──────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Open the reader and display a specific chapter from the beginning.
        /// </summary>
        public void OpenChapter(MangaChapter chapter)
        {
            if (chapter == null)
            {
                Debug.LogWarning("[MangaReaderUI] Attempted to open null chapter.");
                return;
            }

            _currentChapter = chapter;
            _currentPageIndex = 0;
            _currentPanelIndex = 0;

            // Mark as read (triggers first-read rewards via MangaManager)
            MangaManager.Instance?.MarkChapterRead(chapter);

            // Setup header
            UpdateHeader();

            // Show reader
            if (readerRoot != null)
                readerRoot.SetActive(true);

            // Apply chapter tint
            if (backgroundImage != null)
                backgroundImage.color = chapter.chapterTint;

            // Display first page
            DisplayCurrentPage();

            Debug.Log($"[MangaReaderUI] Opened chapter: '{chapter.chapterTitle}'");
        }

        /// <summary>Close the reader and return to the gallery or game.</summary>
        public void Close()
        {
            StopTypewriter();

            if (readerRoot != null)
                readerRoot.SetActive(false);

            _currentChapter = null;
        }

        /// <summary>Check if the reader is currently open.</summary>
        public bool IsOpen => readerRoot != null && readerRoot.activeSelf;

        // ──────────────────────────────────────────────
        // Navigation
        // ──────────────────────────────────────────────

        public void NextPanel()
        {
            if (_isTransitioning || _currentChapter == null) return;

            var page = GetCurrentPage();
            if (page == null) return;

            // Move to next panel on current page
            if (_currentPanelIndex < page.panels.Count - 1)
            {
                _currentPanelIndex++;
                StartCoroutine(TransitionToPanel(page.panels[_currentPanelIndex]));
            }
            // Move to next page
            else if (_currentPageIndex < _currentChapter.pages.Count - 1)
            {
                _currentPageIndex++;
                _currentPanelIndex = 0;
                PlayPageFlipSound();
                StartCoroutine(TransitionPage(() => DisplayCurrentPage()));
            }
            // End of chapter
            else
            {
                ShowEndOfChapter();
            }

            UpdateNavigationButtons();
        }

        public void PrevPanel()
        {
            if (_isTransitioning || _currentChapter == null) return;

            // Move to previous panel
            if (_currentPanelIndex > 0)
            {
                _currentPanelIndex--;
                var page = GetCurrentPage();
                if (page != null)
                    StartCoroutine(TransitionToPanel(page.panels[_currentPanelIndex]));
            }
            // Move to previous page
            else if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                var prevPage = GetCurrentPage();
                if (prevPage != null)
                    _currentPanelIndex = Mathf.Max(0, prevPage.panels.Count - 1);
                PlayPageFlipSound();
                StartCoroutine(TransitionPage(() => DisplayCurrentPage()));
            }

            UpdateNavigationButtons();
        }

        public void GoToNextChapter()
        {
            if (_currentChapter == null) return;

            // Find next chapter in the same arc or next arc
            var allChapters = MangaManager.Instance?.GetAllChapters();
            if (allChapters == null) return;

            int nextIndex = _currentChapter.globalChapterIndex; // 0-based or 1-based
            MangaChapter nextChapter = allChapters.Find(
                c => c.globalChapterIndex == nextIndex + 1
            );

            if (nextChapter != null && MangaManager.Instance.IsChapterUnlocked(nextChapter.ChapterId))
            {
                OpenChapter(nextChapter);
            }
        }

        // ──────────────────────────────────────────────
        // Display Logic
        // ──────────────────────────────────────────────

        private void DisplayCurrentPage()
        {
            var page = GetCurrentPage();
            if (page == null || page.panels.Count == 0) return;

            // Set page background
            if (backgroundImage != null && page.pageBackground != null)
                backgroundImage.sprite = page.pageBackground;

            // Display first panel of the page
            _currentPanelIndex = 0;
            DisplayPanel(page.panels[0]);
            UpdateNavigationButtons();
            UpdatePageCounter();
        }

        private void DisplayPanel(MangaPanel panel)
        {
            if (panel == null) return;

            StopTypewriter();

            // Panel image
            if (panelImageDisplay != null)
            {
                panelImageDisplay.sprite = panel.panelImage;
                panelImageDisplay.gameObject.SetActive(panel.panelImage != null);
            }

            // Narrator text
            if (narratorBox != null)
                narratorBox.SetActive(!string.IsNullOrEmpty(panel.narratorText));

            if (narratorTextDisplay != null)
            {
                if (useTypewriterEffect && !string.IsNullOrEmpty(panel.narratorText))
                    _typewriterCoroutine = StartCoroutine(TypewriterEffect(narratorTextDisplay, panel.narratorText));
                else
                    narratorTextDisplay.text = panel.narratorText ?? "";
            }

            // Dialogue
            if (speechBubble != null)
                speechBubble.SetActive(!string.IsNullOrEmpty(panel.dialogueText));

            if (dialogueTextDisplay != null)
            {
                if (useTypewriterEffect && !string.IsNullOrEmpty(panel.dialogueText))
                    _typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogueTextDisplay, panel.dialogueText));
                else
                    dialogueTextDisplay.text = panel.dialogueText ?? "";
            }

            // Speaker name
            if (speakerNameDisplay != null)
                speakerNameDisplay.text = panel.speakerName ?? "";

            // Play SFX
            if (!string.IsNullOrEmpty(panel.sfxKey))
                PlayPanelSfx(panel);
        }

        private void UpdateHeader()
        {
            if (_currentChapter == null) return;

            if (chapterTitleText != null)
                chapterTitleText.text = _currentChapter.chapterTitle;

            if (arcTitleText != null)
            {
                int arcIndex = Mathf.Clamp(_currentChapter.arcId - 1, 0, ArcNames.Length - 1);
                arcTitleText.text = $"Arc {_currentChapter.arcId}: {ArcNames[arcIndex]}";
            }

            if (settingText != null)
                settingText.text = _currentChapter.settingName ?? "";
        }

        private void UpdatePageCounter()
        {
            if (pageCounter != null && _currentChapter != null)
            {
                pageCounter.text = $"Page {_currentPageIndex + 1} / {_currentChapter.pages.Count}";
            }
        }

        private void UpdateNavigationButtons()
        {
            if (_currentChapter == null) return;

            var page = GetCurrentPage();
            bool isFirstPanel = _currentPageIndex == 0 && _currentPanelIndex == 0;
            bool isLastPanel = _currentPageIndex >= _currentChapter.pages.Count - 1
                && page != null
                && _currentPanelIndex >= page.panels.Count - 1;

            if (prevButton != null) prevButton.interactable = !isFirstPanel;
            if (nextButton != null) nextButton.gameObject.SetActive(!isLastPanel);
            if (nextChapterButton != null) nextChapterButton.gameObject.SetActive(isLastPanel);
        }

        private void ShowEndOfChapter()
        {
            // Check if next chapter is available
            if (nextChapterButton != null)
                nextChapterButton.gameObject.SetActive(true);
            if (nextButton != null)
                nextButton.gameObject.SetActive(false);
        }

        // ──────────────────────────────────────────────
        // Transitions & Effects
        // ──────────────────────────────────────────────

        private IEnumerator TransitionToPanel(MangaPanel panel)
        {
            _isTransitioning = true;

            float duration = panel.transitionDuration;

            switch (panel.transition)
            {
                case PanelTransition.Fade:
                    yield return StartCoroutine(FadeTransition(duration));
                    break;

                case PanelTransition.Dramatic:
                    yield return StartCoroutine(DramaticTransition(duration));
                    break;

                default:
                    yield return StartCoroutine(FadeTransition(duration * 0.5f));
                    break;
            }

            DisplayPanel(panel);
            _isTransitioning = false;
        }

        private IEnumerator TransitionPage(System.Action onMidTransition)
        {
            _isTransitioning = true;

            // Fade out
            if (fadeGroup != null)
            {
                float t = 0f;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    fadeGroup.alpha = 1f - (t / 0.3f);
                    yield return null;
                }
                fadeGroup.alpha = 0f;
            }

            onMidTransition?.Invoke();

            // Fade in
            if (fadeGroup != null)
            {
                float t = 0f;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    fadeGroup.alpha = t / 0.3f;
                    yield return null;
                }
                fadeGroup.alpha = 1f;
            }

            _isTransitioning = false;
        }

        private IEnumerator FadeTransition(float duration)
        {
            if (fadeGroup == null) yield break;

            float half = duration * 0.5f;
            float t = 0f;

            // Fade out
            while (t < half)
            {
                t += Time.deltaTime;
                fadeGroup.alpha = 1f - (t / half);
                yield return null;
            }

            // Fade in
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                fadeGroup.alpha = t / half;
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        private IEnumerator DramaticTransition(float duration)
        {
            // Play dramatic sound
            if (sfxSource != null && dramaticRevealSound != null)
                sfxSource.PlayOneShot(dramaticRevealSound);

            // Slow fade with slight scale punch
            yield return StartCoroutine(FadeTransition(duration * 1.5f));
        }

        private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText)
        {
            textComponent.text = "";
            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        private void StopTypewriter()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
        }

        // ──────────────────────────────────────────────
        // Audio
        // ──────────────────────────────────────────────

        private void PlayPageFlipSound()
        {
            if (sfxSource != null && pageFlipSound != null)
                sfxSource.PlayOneShot(pageFlipSound);
        }

        private void PlayPanelSfx(MangaPanel panel)
        {
            // In production: load clip from addressables using panel.sfxKey
            // For now, log the intended SFX
            Debug.Log($"[MangaReaderUI] SFX: {panel.sfxKey}");
        }

        // ──────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────

        private MangaPage GetCurrentPage()
        {
            if (_currentChapter == null || _currentChapter.pages == null) return null;
            if (_currentPageIndex < 0 || _currentPageIndex >= _currentChapter.pages.Count) return null;
            return _currentChapter.pages[_currentPageIndex];
        }

        // ──────────────────────────────────────────────
        // Touch / Swipe Support
        // ──────────────────────────────────────────────

        private Vector2 _touchStartPos;
        private const float SWIPE_THRESHOLD = 50f;

        private void Update()
        {
            if (!IsOpen || _isTransitioning) return;

            // Keyboard navigation
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space))
                NextPanel();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                PrevPanel();
            else if (Input.GetKeyDown(KeyCode.Escape))
                Close();

            // Touch swipe
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                    _touchStartPos = touch.position;
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 delta = touch.position - _touchStartPos;
                    if (Mathf.Abs(delta.x) > SWIPE_THRESHOLD)
                    {
                        if (delta.x < 0) NextPanel();  // Swipe left → next
                        else PrevPanel();               // Swipe right → prev
                    }
                }
            }
        }
    }
}
