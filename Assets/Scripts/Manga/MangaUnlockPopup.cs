using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Post-level popup notification when a manga chapter is unlocked.
    /// Shows a peek preview of the new chapter with "Read Now" / "Later" buttons.
    /// </summary>
    public class MangaUnlockPopup : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Inspector
        // ──────────────────────────────────────────────

        [Header("Root")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private CanvasGroup popupCanvasGroup;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI chapterNameText;
        [SerializeField] private TextMeshProUGUI arcNameText;
        [SerializeField] private TextMeshProUGUI previewText;
        [SerializeField] private Image thumbnailPreview;
        [SerializeField] private Image mangaPagePeek;

        [Header("Reward Display")]
        [SerializeField] private GameObject rewardContainer;
        [SerializeField] private TextMeshProUGUI coinRewardText;
        [SerializeField] private TextMeshProUGUI gemRewardText;

        [Header("Buttons")]
        [SerializeField] private Button readNowButton;
        [SerializeField] private Button laterButton;

        [Header("Animation")]
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private float slideInDuration = 0.6f;
        [SerializeField] private float peekAnimationDelay = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip unlockChime;
        [SerializeField] private AudioClip pageRustleSound;

        [Header("References")]
        [SerializeField] private MangaReaderUI mangaReader;
        [SerializeField] private MangaGalleryUI mangaGallery;

        // ──────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────

        private MangaChapter _pendingChapter;
        private Coroutine _animationCoroutine;

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
            if (popupRoot != null)
                popupRoot.SetActive(false);

            if (readNowButton != null)
                readNowButton.onClick.AddListener(OnReadNow);

            if (laterButton != null)
                laterButton.onClick.AddListener(OnLater);
        }

        private void Start()
        {
            // Listen for chapter unlock events after all Awakes are done
            if (MangaManager.Instance != null)
                MangaManager.Instance.OnChapterUnlocked += ShowUnlockPopup;
        }

        private void OnDestroy()
        {
            if (readNowButton != null)
                readNowButton.onClick.RemoveListener(OnReadNow);
            if (laterButton != null)
                laterButton.onClick.RemoveListener(OnLater);
                
            if (MangaManager.Instance != null)
                MangaManager.Instance.OnChapterUnlocked -= ShowUnlockPopup;
        }

        // ──────────────────────────────────────────────
        // Show Popup
        // ──────────────────────────────────────────────

        /// <summary>
        /// Display the unlock notification for a newly unlocked chapter.
        /// Called automatically via MangaManager.OnChapterUnlocked event.
        /// </summary>
        public void ShowUnlockPopup(MangaChapter chapter)
        {
            if (chapter == null) return;

            _pendingChapter = chapter;

            // Set content
            if (titleText != null)
                titleText.text = "A New Tale Unlocked!";

            if (chapterNameText != null)
                chapterNameText.text = chapter.chapterTitle;

            if (arcNameText != null)
            {
                int arcIndex = Mathf.Clamp(chapter.arcId - 1, 0, ArcNames.Length - 1);
                arcNameText.text = $"Arc {chapter.arcId}: {ArcNames[arcIndex]}";
            }

            if (previewText != null)
                previewText.text = !string.IsNullOrEmpty(chapter.previewDescription)
                    ? chapter.previewDescription
                    : "A new chapter in the tale of the Smiling Master awaits...";

            // Thumbnail
            if (thumbnailPreview != null)
            {
                if (chapter.galleryThumbnail != null)
                {
                    thumbnailPreview.sprite = chapter.galleryThumbnail;
                    thumbnailPreview.gameObject.SetActive(true);
                }
                else
                {
                    thumbnailPreview.gameObject.SetActive(false);
                }
            }

            // Reward preview
            if (rewardContainer != null)
                rewardContainer.SetActive(chapter.coinReward > 0 || chapter.gemReward > 0);

            if (coinRewardText != null)
                coinRewardText.text = chapter.coinReward > 0 ? $"+{chapter.coinReward} Coins" : "";

            if (gemRewardText != null)
                gemRewardText.text = chapter.gemReward > 0 ? $"+{chapter.gemReward} Gems" : "";

            // Show with animation
            if (popupRoot != null)
                popupRoot.SetActive(true);

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(PlayOpenAnimation());

            Debug.Log($"[MangaUnlockPopup] Showing unlock popup for: '{chapter.chapterTitle}'");
        }

        // ──────────────────────────────────────────────
        // Button Handlers
        // ──────────────────────────────────────────────

        private void OnReadNow()
        {
            if (_pendingChapter == null) return;

            MangaChapter chapterToRead = _pendingChapter;
            DismissPopup();

            // Open the manga reader
            if (mangaReader != null)
            {
                mangaReader.OpenChapter(chapterToRead);
            }
            else
            {
                Debug.LogWarning("[MangaUnlockPopup] MangaReaderUI reference not set!");
            }
        }

        private void OnLater()
        {
            DismissPopup();
        }

        private void DismissPopup()
        {
            _pendingChapter = null;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _animationCoroutine = StartCoroutine(PlayCloseAnimation());
        }

        // ──────────────────────────────────────────────
        // Animations
        // ──────────────────────────────────────────────

        private IEnumerator PlayOpenAnimation()
        {
            // Play unlock chime
            if (sfxSource != null && unlockChime != null)
                sfxSource.PlayOneShot(unlockChime);

            // Fade in
            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 0f;
                float t = 0f;
                while (t < slideInDuration)
                {
                    t += Time.deltaTime;
                    popupCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t / slideInDuration);
                    yield return null;
                }
                popupCanvasGroup.alpha = 1f;
            }

            // Page peek animation (manga page slides up slightly)
            yield return new WaitForSeconds(peekAnimationDelay);

            if (mangaPagePeek != null)
            {
                // Play page rustle sound
                if (sfxSource != null && pageRustleSound != null)
                    sfxSource.PlayOneShot(pageRustleSound);

                // Animate the peek panel sliding up
                RectTransform peekRect = mangaPagePeek.GetComponent<RectTransform>();
                if (peekRect != null)
                {
                    Vector2 startPos = peekRect.anchoredPosition;
                    Vector2 endPos = startPos + new Vector2(0, 30f);
                    float t = 0f;
                    while (t < 0.4f)
                    {
                        t += Time.deltaTime;
                        peekRect.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t / 0.4f));
                        yield return null;
                    }
                }
            }

            _animationCoroutine = null;
        }

        private IEnumerator PlayCloseAnimation()
        {
            if (popupCanvasGroup != null)
            {
                float t = 0f;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    popupCanvasGroup.alpha = 1f - (t / 0.3f);
                    yield return null;
                }
                popupCanvasGroup.alpha = 0f;
            }

            if (popupRoot != null)
                popupRoot.SetActive(false);

            _animationCoroutine = null;
        }
    }
}
