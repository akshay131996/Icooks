using System.Collections.Generic;
using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// ScriptableObject defining a recipe — what ingredients are needed,
    /// cooking steps, point values, and time limits.
    /// Create via Assets → Create → Chef's Journey → Gameplay → Recipe.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewRecipe",
        menuName = "Chef's Journey/Gameplay/Recipe",
        order = 1
    )]
    public class Recipe : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown in orders and recipe book")]
        public string recipeName;

        [Tooltip("Short description of the dish")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("Thumbnail sprite for the recipe book / order display")]
        public Sprite icon;

        [Header("Requirements")]
        [Tooltip("Ordered list of ingredients needed")]
        public List<Ingredient> ingredients = new List<Ingredient>();

        [Tooltip("Steps the player must perform (chop, boil, fry, etc.)")]
        public List<CookingStep> cookingSteps = new List<CookingStep>();

        [Header("Scoring")]
        [Tooltip("Base points for completing this recipe")]
        public int basePoints = 100;

        [Tooltip("Bonus points for perfect execution")]
        public int perfectBonus = 50;

        [Tooltip("Time limit in seconds (0 = no limit)")]
        public float timeLimit = 60f;

        [Header("Progression")]
        [Tooltip("Minimum level to encounter this recipe")]
        public int minLevel = 1;

        [Tooltip("Difficulty rating 1–5")]
        [Range(1, 5)]
        public int difficulty = 1;

        [Tooltip("Coins awarded on first completion")]
        public int coinReward = 25;

        [Header("Story")]
        [Tooltip("Hidden story snippet unlocked when recipe is mastered")]
        [TextArea(2, 5)]
        public string hiddenStory;

        [Tooltip("Has the player unlocked the hidden story?")]
        [HideInInspector]
        public bool storyUnlocked;
    }

    /// <summary>
    /// A single cooking step in a recipe (e.g., chop, boil, fry).
    /// </summary>
    [System.Serializable]
    public class CookingStep
    {
        public enum StepType { Chop, Boil, Fry, Bake, Mix, Plate, Garnish }

        public StepType type;
        [Tooltip("Target ingredient for this step (null = any)")]
        public Ingredient targetIngredient;
        [Tooltip("Duration in seconds the player must hold/interact")]
        public float duration = 2f;
        [Tooltip("UI instruction text")]
        public string instruction;
    }
}
