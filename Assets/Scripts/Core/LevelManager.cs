using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChefJourney.Core
{
    /// <summary>
    /// Manages level loading, difficulty progression, and scene transitions.
    /// Each kitchen/restaurant is a separate Unity scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Config")]
        [SerializeField] private string[] levelSceneNames;
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("Difficulty")]
        [SerializeField] private float baseDifficultyMultiplier = 1f;
        [SerializeField] private float difficultyIncrement = 0.15f;

        public float CurrentDifficulty =>
            baseDifficultyMultiplier + (GameManager.Instance.CurrentLevel - 1) * difficultyIncrement;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelCompleted.AddListener(OnLevelCompleted);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelCompleted.RemoveListener(OnLevelCompleted);
        }

        // ─── Scene Loading ─────────────────────────────────────
        public void LoadLevel(int levelNumber)
        {
            if (levelSceneNames == null || levelSceneNames.Length == 0)
            {
                Debug.LogWarning("[LevelManager] No level scenes configured!");
                return;
            }

            int index = Mathf.Clamp(levelNumber - 1, 0, levelSceneNames.Length - 1);
            string sceneName = levelSceneNames[index];

            Debug.Log($"[LevelManager] Loading level {levelNumber}: {sceneName} (difficulty: {CurrentDifficulty:F2})");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }

        public void ReloadCurrentLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadNextLevel()
        {
            LoadLevel(GameManager.Instance.CurrentLevel);
        }

        // ─── Bonus Levels ──────────────────────────────────────
        public void LoadBonusLevel(string bonusSceneName)
        {
            Debug.Log($"[LevelManager] Loading bonus level: {bonusSceneName}");
            SceneManager.LoadScene(bonusSceneName);
        }

        /// <summary>
        /// Check if a bonus level is available based on player progress.
        /// Bonus levels unlock every 5 levels.
        /// </summary>
        public bool IsBonusLevelAvailable(int currentLevel)
        {
            return currentLevel > 0 && currentLevel % 5 == 0;
        }

        // ─── Callbacks ─────────────────────────────────────────
        private void OnLevelCompleted(int completedLevel)
        {
            // Notify manga system about level completion
            if (ChefJourney.Manga.MangaManager.Instance != null)
                ChefJourney.Manga.MangaManager.Instance.OnLevelCompleted(completedLevel);

            Debug.Log($"[LevelManager] Level {completedLevel} completed! Next difficulty: {CurrentDifficulty:F2}");
        }

        /// <summary>
        /// Get the total number of available levels.
        /// </summary>
        public int TotalLevels => levelSceneNames?.Length ?? 0;
    }
}
