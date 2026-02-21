using System.Collections.Generic;
using UnityEngine;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Loads manga chapter data from JSON files in Resources/Data/Manga/.
    /// Call LoadAllArcs() at startup to populate MangaManager with chapters.
    /// </summary>
    public static class MangaDataLoader
    {
        // ──────────────────────────────────────────────
        // JSON Data Structures (match the JSON schema)
        // ──────────────────────────────────────────────

        [System.Serializable]
        private class ArcData
        {
            public int arcId;
            public string arcName;
            public string setting;
            public string tone;
            public List<ChapterJsonData> chapters;
        }

        [System.Serializable]
        private class ChapterJsonData
        {
            public int chapterNumber;
            public int globalIndex;
            public string title;
            public string subtitle;
            public int unlockAtLevel;
            public int coinReward;
            public int gemReward;
            public string previewDescription;
            public List<PageJsonData> pages;
        }

        [System.Serializable]
        private class PageJsonData
        {
            public string layout;
            public List<PanelJsonData> panels;
        }

        [System.Serializable]
        private class PanelJsonData
        {
            public string panelId;
            public string narratorText;
            public string dialogueText;
            public string speakerName;
            public string characterExpression;
            public bool masterSmileVisible;
            public string panelSize;
            public string transition;
            public float transitionDuration;
            public string sfxKey;
            public string musicKey;
        }

        // ──────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Load all 5 arc JSON files from Resources and convert to
        /// MangaChapter ScriptableObjects at runtime.
        /// </summary>
        public static List<MangaChapter> LoadAllArcs()
        {
            var allChapters = new List<MangaChapter>();

            for (int arcId = 1; arcId <= 5; arcId++)
            {
                string path = $"Data/Manga/MangaArc{arcId}_Chapters";
                TextAsset jsonAsset = Resources.Load<TextAsset>(path);

                if (jsonAsset == null)
                {
                    Debug.LogWarning($"[MangaDataLoader] Could not find JSON at Resources/{path}");
                    continue;
                }

                ArcData arcData = JsonUtility.FromJson<ArcData>(jsonAsset.text);
                if (arcData == null || arcData.chapters == null)
                {
                    Debug.LogWarning($"[MangaDataLoader] Failed to parse arc {arcId} JSON.");
                    continue;
                }

                foreach (var chapterJson in arcData.chapters)
                {
                    MangaChapter chapter = ConvertToChapter(arcData, chapterJson);
                    if (chapter != null)
                        allChapters.Add(chapter);
                }

                Debug.Log($"[MangaDataLoader] Loaded Arc {arcId}: '{arcData.arcName}' " +
                          $"({arcData.chapters.Count} chapters)");
            }

            Debug.Log($"[MangaDataLoader] Total chapters loaded: {allChapters.Count}");
            return allChapters;
        }

        // ──────────────────────────────────────────────
        // Conversion
        // ──────────────────────────────────────────────

        private static MangaChapter ConvertToChapter(ArcData arc, ChapterJsonData json)
        {
            // Create runtime ScriptableObject instance (not saved to disk)
            MangaChapter chapter = ScriptableObject.CreateInstance<MangaChapter>();

            chapter.arcId = arc.arcId;
            chapter.chapterNumber = json.chapterNumber;
            chapter.globalChapterIndex = json.globalIndex;
            chapter.chapterTitle = json.title;
            chapter.subtitle = json.subtitle;
            chapter.unlockAtLevel = json.unlockAtLevel;
            chapter.coinReward = json.coinReward;
            chapter.gemReward = json.gemReward;
            chapter.previewDescription = json.previewDescription;
            chapter.settingName = arc.setting;
            chapter.tone = ParseTone(arc.tone);

            // Convert pages
            chapter.pages = new List<MangaPage>();
            if (json.pages != null)
            {
                foreach (var pageJson in json.pages)
                {
                    chapter.pages.Add(ConvertToPage(pageJson));
                }
            }

            // Set chapter name for debugging
            chapter.name = $"Arc{arc.arcId}_Ch{json.chapterNumber}_{json.title}";

            return chapter;
        }

        private static MangaPage ConvertToPage(PageJsonData json)
        {
            var page = new MangaPage
            {
                layout = ParseLayout(json.layout),
                panels = new List<MangaPanel>()
            };

            if (json.panels != null)
            {
                foreach (var panelJson in json.panels)
                {
                    page.panels.Add(ConvertToPanel(panelJson));
                }
            }

            return page;
        }

        private static MangaPanel ConvertToPanel(PanelJsonData json)
        {
            return new MangaPanel
            {
                panelId = json.panelId ?? "",
                narratorText = json.narratorText ?? "",
                dialogueText = json.dialogueText ?? "",
                speakerName = json.speakerName ?? "",
                characterExpression = json.characterExpression ?? "",
                masterSmileVisible = json.masterSmileVisible,
                panelSize = ParsePanelSize(json.panelSize),
                transition = ParseTransition(json.transition),
                transitionDuration = json.transitionDuration > 0 ? json.transitionDuration : 0.4f,
                sfxKey = json.sfxKey ?? "",
                musicKey = json.musicKey ?? ""
            };
        }

        // ──────────────────────────────────────────────
        // Enum Parsers
        // ──────────────────────────────────────────────

        private static ChapterTone ParseTone(string tone)
        {
            if (string.IsNullOrEmpty(tone)) return ChapterTone.Warm;

            string lower = tone.ToLower();
            if (lower.Contains("comedic") || lower.Contains("comedy")) return ChapterTone.Comedic;
            if (lower.Contains("adventure") || lower.Contains("adventurous")) return ChapterTone.Adventurous;
            if (lower.Contains("bitter")) return ChapterTone.Bittersweet;
            if (lower.Contains("dark")) return ChapterTone.Dark;
            if (lower.Contains("intense")) return ChapterTone.Intense;
            if (lower.Contains("emotion")) return ChapterTone.Emotional;
            if (lower.Contains("myster")) return ChapterTone.Mysterious;
            return ChapterTone.Warm;
        }

        private static PageLayout ParseLayout(string layout)
        {
            if (string.IsNullOrEmpty(layout)) return PageLayout.Vertical;

            return layout.ToLower() switch
            {
                "horizontal" => PageLayout.Horizontal,
                "grid" => PageLayout.Grid,
                "splash" => PageLayout.Splash,
                "cinematic" => PageLayout.Cinematic,
                _ => PageLayout.Vertical
            };
        }

        private static PanelSize ParsePanelSize(string size)
        {
            if (string.IsNullOrEmpty(size)) return PanelSize.Medium;

            return size.ToLower() switch
            {
                "small" => PanelSize.Small,
                "large" => PanelSize.Large,
                "fullpage" => PanelSize.FullPage,
                _ => PanelSize.Medium
            };
        }

        private static PanelTransition ParseTransition(string transition)
        {
            if (string.IsNullOrEmpty(transition)) return PanelTransition.Fade;

            return transition.ToLower() switch
            {
                "none" => PanelTransition.None,
                "slideleft" => PanelTransition.SlideLeft,
                "slideright" => PanelTransition.SlideRight,
                "slideup" => PanelTransition.SlideUp,
                "zoomin" => PanelTransition.ZoomIn,
                "dramatic" => PanelTransition.Dramatic,
                _ => PanelTransition.Fade
            };
        }
    }
}
