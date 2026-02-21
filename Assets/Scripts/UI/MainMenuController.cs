using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChefJourney.UI
{
    /// <summary>
    /// Controls the Main Menu scene — handles button interactions,
    /// scene transitions, and displays player progress.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip buttonClickSfx;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "KitchenLevel";
        [SerializeField] private string shopSceneName = "Shop";
        [SerializeField] private string recipeBookSceneName = "RecipeBook";
        [SerializeField] private string mangaGallerySceneName = "MangaGallery";

        private AudioSource _sfxSource;

        private void Start()
        {
            ShowMainPanel();

            // Ensure we have an SFX source
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            // Play background music
            if (bgmSource != null && menuMusic != null)
            {
                bgmSource.clip = menuMusic;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        // ─── Button Callbacks ──────────────────────────────────

        public void OnPlayButton()
        {
            PlayClickSound();
            if (Core.LevelManager.Instance != null)
                Core.LevelManager.Instance.LoadLevel(Core.GameManager.Instance.CurrentLevel);
            else
                SceneManager.LoadScene(gameSceneName);
        }

        public void OnRecipeBookButton()
        {
            PlayClickSound();
            SceneManager.LoadScene(recipeBookSceneName);
        }

        public void OnShopButton()
        {
            PlayClickSound();
            SceneManager.LoadScene(shopSceneName);
        }

        public void OnMangaButton()
        {
            PlayClickSound();
            SceneManager.LoadScene(mangaGallerySceneName);
        }

        public void OnSettingsButton()
        {
            PlayClickSound();
            mainPanel?.SetActive(false);
            settingsPanel?.SetActive(true);
        }

        public void OnCreditsButton()
        {
            PlayClickSound();
            mainPanel?.SetActive(false);
            creditsPanel?.SetActive(true);
        }

        public void OnBackButton()
        {
            PlayClickSound();
            ShowMainPanel();
        }

        public void OnQuitButton()
        {
            PlayClickSound();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // ─── Helpers ───────────────────────────────────────────
        private void ShowMainPanel()
        {
            mainPanel?.SetActive(true);
            settingsPanel?.SetActive(false);
            creditsPanel?.SetActive(false);
        }

        private void PlayClickSound()
        {
            if (_sfxSource != null && buttonClickSfx != null)
                _sfxSource.PlayOneShot(buttonClickSfx);
        }
    }
}
