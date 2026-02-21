using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace ChefJourney.UI
{
    /// <summary>
    /// Central UI controller — manages HUD elements, menus, score display,
    /// pause screen, and navigation between UI panels.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI gemText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject recipeBookPanel;

        [Header("Level Complete")]
        [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
        [SerializeField] private Image[] starImages;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to game events
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
                Core.GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
            }
            if (Economy.CurrencyManager.Instance != null)
            {
                Economy.CurrencyManager.Instance.OnCoinsChanged.AddListener(UpdateCoins);
                Economy.CurrencyManager.Instance.OnGemsChanged.AddListener(UpdateGems);
            }

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged.RemoveListener(UpdateScore);
                Core.GameManager.Instance.OnStateChanged.RemoveListener(OnGameStateChanged);
            }
        }

        // ─── State Handling ────────────────────────────────────
        private void OnGameStateChanged(Core.GameManager.GameState state)
        {
            HideAllPanels();
            switch (state)
            {
                case Core.GameManager.GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case Core.GameManager.GameState.Playing:
                    ShowHUD();
                    break;
                case Core.GameManager.GameState.Paused:
                    ShowPause();
                    break;
                case Core.GameManager.GameState.LevelComplete:
                    ShowLevelComplete();
                    break;
                case Core.GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        // ─── Panel Visibility ──────────────────────────────────
        private void HideAllPanels()
        {
            hudPanel?.SetActive(false);
            mainMenuPanel?.SetActive(false);
            pausePanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            levelCompletePanel?.SetActive(false);
            shopPanel?.SetActive(false);
            recipeBookPanel?.SetActive(false);
        }

        public void ShowMainMenu() { HideAllPanels(); mainMenuPanel?.SetActive(true); }
        public void ShowHUD()      { HideAllPanels(); hudPanel?.SetActive(true); }
        public void ShowPause()    { hudPanel?.SetActive(true); pausePanel?.SetActive(true); }
        public void ShowGameOver() { HideAllPanels(); gameOverPanel?.SetActive(true); }
        public void ShowShop()     { HideAllPanels(); shopPanel?.SetActive(true); }
        public void ShowRecipeBook() { HideAllPanels(); recipeBookPanel?.SetActive(true); }

        public void ShowLevelComplete()
        {
            HideAllPanels();
            levelCompletePanel?.SetActive(true);
            if (levelCompleteScoreText != null && Core.GameManager.Instance != null)
                levelCompleteScoreText.text = $"Score: {Core.GameManager.Instance.Score}";
        }

        // ─── HUD Updates ──────────────────────────────────────
        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        public void UpdateLevel(int level)
        {
            if (levelText != null) levelText.text = $"Level {level}";
        }

        private void UpdateCoins(int coins)
        {
            if (coinText != null) coinText.text = coins.ToString();
        }

        private void UpdateGems(int gems)
        {
            if (gemText != null) gemText.text = gems.ToString();
        }

        public void UpdateTimer(float seconds)
        {
            if (timerText != null)
            {
                int mins = Mathf.FloorToInt(seconds / 60);
                int secs = Mathf.FloorToInt(seconds % 60);
                timerText.text = $"{mins:00}:{secs:00}";
            }
        }

        // ─── Button Callbacks ──────────────────────────────────
        public void OnPlayButton()    => Core.GameManager.Instance?.StartLevel(Core.GameManager.Instance.CurrentLevel);
        public void OnPauseButton()   => Core.GameManager.Instance?.PauseGame();
        public void OnResumeButton()  => Core.GameManager.Instance?.ResumeGame();
        public void OnMainMenuButton() => Core.GameManager.Instance?.SetState(Core.GameManager.GameState.MainMenu);
    }
}
