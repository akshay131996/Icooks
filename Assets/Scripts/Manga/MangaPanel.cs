using System;
using UnityEngine;

namespace ChefJourney.Manga
{
    /// <summary>
    /// Represents a single panel within a manga chapter page.
    /// Serializable so it can be embedded in MangaChapter ScriptableObjects
    /// and deserialized from JSON data files.
    /// </summary>
    [Serializable]
    public class MangaPanel
    {
        [Tooltip("Unique panel identifier (e.g. arc1_ch1_p1)")]
        public string panelId;

        [Tooltip("Reference to the panel artwork sprite")]
        public Sprite panelImage;

        [Tooltip("Addressable key for lazy-loading the panel artwork")]
        public string panelImageKey;

        [Header("Text Content")]
        [TextArea(2, 5)]
        [Tooltip("Narrator box text (top or bottom of panel)")]
        public string narratorText;

        [TextArea(2, 5)]
        [Tooltip("Character dialogue in speech bubble")]
        public string dialogueText;

        [Tooltip("Name of the speaking character (empty = narrator only)")]
        public string speakerName;

        [Header("Character Expression")]
        [Tooltip("Expression tag for the speaking character (e.g. smile, shocked, sad)")]
        public string characterExpression;

        [Tooltip("If true, the Master's trademark smile is visible in this panel")]
        public bool masterSmileVisible;

        [Header("Layout & Presentation")]
        [Tooltip("Panel size hint relative to the page")]
        public PanelSize panelSize = PanelSize.Medium;

        [Tooltip("Horizontal position on the page")]
        public PanelPosition horizontalPosition = PanelPosition.Center;

        [Tooltip("Transition effect when this panel appears")]
        public PanelTransition transition = PanelTransition.Fade;

        [Tooltip("Duration of the transition in seconds")]
        [Range(0.1f, 2f)]
        public float transitionDuration = 0.4f;

        [Header("Audio")]
        [Tooltip("Optional SFX to play when this panel appears")]
        public string sfxKey;

        [Tooltip("Optional ambient/music change for dramatic panels")]
        public string musicKey;
    }

    /// <summary>Panel size relative to the page grid.</summary>
    public enum PanelSize
    {
        Small,      // Quarter page
        Medium,     // Half page
        Large,      // Three-quarter page
        FullPage    // Splash page (dramatic moments)
    }

    /// <summary>Horizontal alignment within the page.</summary>
    public enum PanelPosition
    {
        Left,
        Center,
        Right,
        FullWidth
    }

    /// <summary>Transition animation when a panel is revealed.</summary>
    public enum PanelTransition
    {
        None,       // Instant
        Fade,       // Fade in
        SlideLeft,  // Slide from right
        SlideRight, // Slide from left
        SlideUp,    // Slide from bottom
        ZoomIn,     // Zoom from center
        Dramatic    // Slow fade + screen shake (for big reveals)
    }
}
