using UnityEngine;
using UnityEngine.Events;

namespace ChefJourney.Core
{
    /// <summary>
    /// Singleton game controller — manages game state, score, level flow,
    /// and acts as a central event hub for the Icooks chef game.
    /// Persists across scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ─── State ─────────────────────────────────────────────
        public enum GameState { MainMenu, Playing, Paused, LevelComplete, GameOver }

        [Header("State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        [Header("Score")]
        [SerializeField] private int _score;
        public int Score => _score;

        [Header("Level")]
        [SerializeField] private int _currentLevel = 1;
        public int CurrentLevel => _currentLevel;

        // ─── Events ────────────────────────────────────────────
        [HideInInspector] public UnityEvent<GameState> OnStateChanged;
        [HideInInspector] public UnityEvent<int> OnScoreChanged;
        [HideInInspector] public UnityEvent<int> OnLevelCompleted;

        // ────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            OnStateChanged ??= new UnityEvent<GameState>();
            OnScoreChanged ??= new UnityEvent<int>();
            OnLevelCompleted ??= new UnityEvent<int>();
        }

        // ─── Public API ────────────────────────────────────────
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
            Debug.Log($"[GameManager] State → {_currentState}");
        }

        public void AddScore(int points)
        {
            _score += points;
            OnScoreChanged?.Invoke(_score);
        }

        public void ResetScore() { _score = 0; OnScoreChanged?.Invoke(0); }

        public void CompleteLevel()
        {
            SetState(GameState.LevelComplete);
            OnLevelCompleted?.Invoke(_currentLevel);
            _currentLevel++;
        }

        public void StartLevel(int level)
        {
            _currentLevel = level;
            ResetScore();
            SetState(GameState.Playing);
        }

        public void PauseGame()  => SetState(GameState.Paused);
        public void ResumeGame() => SetState(GameState.Playing);
        public void GameOver()   => SetState(GameState.GameOver);
    }
}
