using System.Collections.Generic;
using UnityEngine;

namespace ChefJourney.Manga
{
    /// <summary>
    /// ScriptableObject representing a single manga chapter.
    /// Created via Assets → Create → Chef's Journey → Manga → Chapter.
    /// Each chapter belongs to an arc and contains ordered pages of panels.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewMangaChapter",
        menuName = "Chef's Journey/Manga/Chapter",
        order = 1
    )]
    public class MangaChapter : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Arc this chapter belongs to (1–5)")]
        [Range(1, 5)]
        public int arcId = 1;

        [Tooltip("Chapter number within the arc (1–5)")]
        [Range(1, 5)]
        public int chapterNumber = 1;

        [Tooltip("Global chapter index across all arcs (1–25)")]
        public int globalChapterIndex;

        [Tooltip("Display title shown in the gallery and reader")]
        public string chapterTitle;

        [Tooltip("Short subtitle / tagline for the gallery thumbnail")]
        [TextArea(1, 2)]
        public string subtitle;

        [Header("Unlock Conditions")]
        [Tooltip("Level number the player must complete to unlock this chapter")]
        public int unlockAtLevel;

        [Tooltip("Alternative unlock: number of recipe fragments required (0 = level-only)")]
        public int fragmentsRequired;

        [Header("Content")]
        [Tooltip("Ordered list of pages. Each page is a list of panels.")]
        public List<MangaPage> pages = new List<MangaPage>();

        [Header("Presentation")]
        [Tooltip("Thumbnail sprite for the gallery grid")]
        public Sprite galleryThumbnail;

        [Tooltip("Addressable key for lazy-loading the thumbnail")]
        public string thumbnailKey;

        [Tooltip("Background color/theme tint for this chapter's reader")]
        public Color chapterTint = Color.white;

        [Header("Rewards")]
        [Tooltip("Coins awarded on first read")]
        public int coinReward = 50;

        [Tooltip("Gems awarded on first read")]
        public int gemReward = 5;

        [Header("Metadata")]
        [Tooltip("Destination/setting shown in the reader header")]
        public string settingName;

        [Tooltip("Tone tag for ambient music selection")]
        public ChapterTone tone = ChapterTone.Warm;

        [TextArea(2, 4)]
        [Tooltip("Brief story description shown before reading (spoiler-free)")]
        public string previewDescription;

        /// <summary>
        /// Computed global chapter ID for save data keying.
        /// </summary>
        public string ChapterId => $"arc{arcId}_ch{chapterNumber}";

        /// <summary>
        /// Total number of panels across all pages.
        /// </summary>
        public int TotalPanelCount
        {
            get
            {
                int count = 0;
                foreach (var page in pages)
                {
                    if (page != null && page.panels != null)
                        count += page.panels.Count;
                }
                return count;
            }
        }
    }

    /// <summary>
    /// A single page within a chapter, containing one or more panels.
    /// </summary>
    [System.Serializable]
    public class MangaPage
    {
        [Tooltip("Ordered list of panels on this page")]
        public List<MangaPanel> panels = new List<MangaPanel>();

        [Tooltip("Page layout style")]
        public PageLayout layout = PageLayout.Vertical;

        [Tooltip("Optional full-page background image behind all panels")]
        public Sprite pageBackground;

        [Tooltip("Addressable key for lazy-loading the background")]
        public string backgroundKey;
    }

    /// <summary>How panels are arranged on a page.</summary>
    public enum PageLayout
    {
        Vertical,       // Panels stacked top to bottom (classic manga)
        Horizontal,     // Panels side by side
        Grid,           // 2×2 or custom grid
        Splash,         // Single full-page panel
        Cinematic       // Wide panels with black bars (dramatic moments)
    }

    /// <summary>Tone of the chapter, used for music and color grading.</summary>
    public enum ChapterTone
    {
        Warm,           // Nostalgic, happy
        Comedic,        // Light, funny
        Adventurous,    // Excited, upbeat
        Bittersweet,    // Mixed emotions
        Dark,           // Hardship, struggle
        Intense,        // Competition, rivalry
        Emotional,      // Tearjerker
        Mysterious       // Enigmatic, uncertain
    }
}
