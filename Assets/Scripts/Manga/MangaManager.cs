using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Singleton manager that tracks manga chapter unlock state, listens for
    /// level completion events, awards first-read rewards, and persists
    /// progress via PlayerPrefs (upgradeable to SaveSystem later).
    /// </summary>
    public class MangaManager : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Singleton
        // ──────────────────────────────────────────────

        public static MangaManager Instance { get; private set; }

        // ──────────────────────────────────────────────
        // Inspector
        // ──────────────────────────────────────────────

        [Header("Chapter Database")]
        [Tooltip("All manga chapters in order. Populate via inspector or load from Resources.")]
        [SerializeField] private List<MangaChapter> allChapters = new List<MangaChapter>();

        // ──────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────

        /// <summary>Fired when a chapter is newly unlocked. Passes the chapter data.</summary>
        public event Action<MangaChapter> OnChapterUnlocked;

        /// <summary>Fired when a chapter is read for the first time. Passes the chapter and rewards.</summary>
        public event Action<MangaChapter, int, int> OnChapterFirstRead; // chapter, coins, gems

        /// <summary>Fired when any chapter read state changes.</summary>
        public event Action OnMangaProgressChanged;

        // ──────────────────────────────────────────────
        // Runtime State
        // ──────────────────────────────────────────────

        private HashSet<string> _unlockedChapterIds = new HashSet<string>();
        private HashSet<string> _readChapterIds = new HashSet<string>();

        private const string SAVE_KEY_UNLOCKED = "manga_unlocked";
        private const string SAVE_KEY_READ = "manga_read";

        // ──────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ──────────────────────────────────────────────
        // Public API — Queries
        // ──────────────────────────────────────────────

        /// <summary>Get all chapters for a specific arc.</summary>
        public List<MangaChapter> GetChaptersForArc(int arcId)
        {
            return allChapters
                .Where(c => c.arcId == arcId)
                .OrderBy(c => c.chapterNumber)
                .ToList();
        }

        /// <summary>Get all chapters across all arcs.</summary>
        public List<MangaChapter> GetAllChapters() => new List<MangaChapter>(allChapters);

        /// <summary>Check if a specific chapter is unlocked.</summary>
        public bool IsChapterUnlocked(string chapterId)
        {
            return _unlockedChapterIds.Contains(chapterId);
        }

        /// <summary>Check if a specific chapter is unlocked by arc and chapter number.</summary>
        public bool IsChapterUnlocked(int arcId, int chapterNumber)
        {
            return IsChapterUnlocked($"arc{arcId}_ch{chapterNumber}");
        }

        /// <summary>Check if a chapter has been read at least once.</summary>
        public bool IsChapterRead(string chapterId)
        {
            return _readChapterIds.Contains(chapterId);
        }

        /// <summary>Get the next locked chapter the player can work toward.</summary>
        public MangaChapter GetNextLockedChapter()
        {
            return allChapters
                .OrderBy(c => c.globalChapterIndex)
                .FirstOrDefault(c => !IsChapterUnlocked(c.ChapterId));
        }

        /// <summary>Get unlock progress for an arc (0.0 to 1.0).</summary>
        public float GetArcProgress(int arcId)
        {
            var arcChapters = GetChaptersForArc(arcId);
            if (arcChapters.Count == 0) return 0f;

            int unlocked = arcChapters.Count(c => IsChapterUnlocked(c.ChapterId));
            return (float)unlocked / arcChapters.Count;
        }

        /// <summary>Total number of unlocked chapters.</summary>
        public int TotalUnlockedCount => _unlockedChapterIds.Count;

        /// <summary>Total number of chapters.</summary>
        public int TotalChapterCount => allChapters.Count;

        // ──────────────────────────────────────────────
        // Public API — Actions
        // ──────────────────────────────────────────────

        /// <summary>
        /// Call this when a level is completed. Checks if any manga chapters
        /// should be unlocked for that level number.
        /// </summary>
        public void OnLevelCompleted(int levelNumber)
        {
            foreach (var chapter in allChapters)
            {
                if (chapter.unlockAtLevel == levelNumber && !IsChapterUnlocked(chapter.ChapterId))
                {
                    UnlockChapter(chapter);
                }
            }
        }

        /// <summary>
        /// Mark a chapter as read. Awards first-read rewards if not previously read.
        /// Returns true if this was the first read.
        /// </summary>
        public bool MarkChapterRead(MangaChapter chapter)
        {
            if (chapter == null) return false;

            string id = chapter.ChapterId;
            if (_readChapterIds.Contains(id)) return false;

            _readChapterIds.Add(id);
            SaveProgress();

            // Award first-read rewards
            // Integration point: CurrencyManager.Instance.AddCoins(chapter.coinReward);
            // Integration point: CurrencyManager.Instance.AddGems(chapter.gemReward);

            OnChapterFirstRead?.Invoke(chapter, chapter.coinReward, chapter.gemReward);
            OnMangaProgressChanged?.Invoke();

            Debug.Log($"[MangaManager] Chapter '{chapter.chapterTitle}' read for the first time! " +
                      $"Rewards: {chapter.coinReward} coins, {chapter.gemReward} gems.");

            return true;
        }

        /// <summary>
        /// Force-unlock a chapter (e.g., from a cheat menu or IAP).
        /// </summary>
        public void ForceUnlockChapter(int arcId, int chapterNumber)
        {
            var chapter = allChapters.FirstOrDefault(
                c => c.arcId == arcId && c.chapterNumber == chapterNumber
            );

            if (chapter != null && !IsChapterUnlocked(chapter.ChapterId))
            {
                UnlockChapter(chapter);
            }
        }

        // ──────────────────────────────────────────────
        // Internal
        // ──────────────────────────────────────────────

        private void UnlockChapter(MangaChapter chapter)
        {
            _unlockedChapterIds.Add(chapter.ChapterId);
            SaveProgress();

            OnChapterUnlocked?.Invoke(chapter);
            OnMangaProgressChanged?.Invoke();

            Debug.Log($"[MangaManager] ★ Chapter unlocked: '{chapter.chapterTitle}' " +
                      $"(Arc {chapter.arcId}, Chapter {chapter.chapterNumber})");
        }

        // ──────────────────────────────────────────────
        // Persistence (PlayerPrefs — upgrade to SaveSystem later)
        // ──────────────────────────────────────────────

        private void SaveProgress()
        {
            string unlocked = string.Join(",", _unlockedChapterIds);
            string read = string.Join(",", _readChapterIds);

            PlayerPrefs.SetString(SAVE_KEY_UNLOCKED, unlocked);
            PlayerPrefs.SetString(SAVE_KEY_READ, read);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _unlockedChapterIds.Clear();
            _readChapterIds.Clear();

            string unlocked = PlayerPrefs.GetString(SAVE_KEY_UNLOCKED, "");
            string read = PlayerPrefs.GetString(SAVE_KEY_READ, "");

            if (!string.IsNullOrEmpty(unlocked))
            {
                foreach (string id in unlocked.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        _unlockedChapterIds.Add(id);
                }
            }

            if (!string.IsNullOrEmpty(read))
            {
                foreach (string id in read.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        _readChapterIds.Add(id);
                }
            }

            Debug.Log($"[MangaManager] Loaded progress: {_unlockedChapterIds.Count} unlocked, " +
                      $"{_readChapterIds.Count} read.");
        }

        /// <summary>
        /// Reset all manga progress (for debug/testing).
        /// </summary>
        [ContextMenu("Reset Manga Progress")]
        public void ResetProgress()
        {
            _unlockedChapterIds.Clear();
            _readChapterIds.Clear();
            PlayerPrefs.DeleteKey(SAVE_KEY_UNLOCKED);
            PlayerPrefs.DeleteKey(SAVE_KEY_READ);
            PlayerPrefs.Save();
            OnMangaProgressChanged?.Invoke();
            Debug.Log("[MangaManager] All manga progress reset.");
        }
    }
}
