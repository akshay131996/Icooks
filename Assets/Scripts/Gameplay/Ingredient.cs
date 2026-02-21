using UnityEngine;

namespace ChefJourney.Gameplay
{
    /// <summary>
    /// ScriptableObject representing an individual ingredient.
    /// Create via Assets → Create → Chef's Journey → Gameplay → Ingredient.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewIngredient",
        menuName = "Chef's Journey/Gameplay/Ingredient",
        order = 2
    )]
    public class Ingredient : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the ingredient")]
        public string ingredientName;

        [Tooltip("Category for sorting / filtering")]
        public IngredientCategory category;

        [Tooltip("Icon sprite for UI display")]
        public Sprite icon;

        [Header("Properties")]
        [Tooltip("Base cost in the shop")]
        public int shopCost = 10;

        [Tooltip("Is this ingredient unlocked by default?")]
        public bool unlockedByDefault = true;

        [Tooltip("Level at which this ingredient first appears")]
        public int availableFromLevel = 1;

        [Header("Visuals")]
        [Tooltip("Sprite to display on the kitchen counter / conveyor")]
        public Sprite worldSprite;

        [Tooltip("Color tint for effects")]
        public Color tintColor = Color.white;
    }

    public enum IngredientCategory
    {
        Vegetable,
        Fruit,
        Meat,
        Seafood,
        Dairy,
        Grain,
        Spice,
        Sauce,
        Special
    }
}
