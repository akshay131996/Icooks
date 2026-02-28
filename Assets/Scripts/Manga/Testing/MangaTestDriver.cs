using UnityEngine;
using ChefJourney.Manga;
using ChefJourney.Core;

namespace ChefJourney.Testing
{
    /// <summary>
    /// Debug script to test Manga UI flows without playing the full game.
    /// </summary>
    public class MangaTestDriver : MonoBehaviour
    {
        [Header("References")]
        public MangaGalleryUI galleryUI;
        public MangaReaderUI readerUI;
        public MangaUnlockPopup unlockPopup;
        
        [Header("Test Data")]
        public int testLevelToComplete = 3; // First arc, chapter 1

        private void Start()
        {
            // Ensure MangaManager is loaded
            if (MangaManager.Instance == null)
            {
                Debug.LogWarning("[MangaTestDriver] MangaManager is missing! Make sure it's in the scene.");
            }
        }

        [ContextMenu("Test_CompleteLevel")]
        public void Test_CompleteLevel()
        {
            Debug.Log($"[MangaTestDriver] Simulating completing level {testLevelToComplete}...");
            
            var manager = MangaManager.Instance;
            if (manager == null)
            {
                manager = Object.FindFirstObjectByType<MangaManager>();
            }

            if (manager != null)
            {
                manager.OnLevelCompleted(testLevelToComplete);
            }
            else
            {
                Debug.LogError("[MangaTestDriver] MangaManager could not be found in the scene! Cannot test.");
            }
        }

        [ContextMenu("Test_OpenGallery")]
        public void Test_OpenGallery()
        {
            Debug.Log("[MangaTestDriver] Opening Manga Gallery...");
            if (galleryUI != null)
            {
                galleryUI.gameObject.SetActive(true);
            }
        }

        [ContextMenu("Test_UnlockAll")]
        public void Test_UnlockAll()
        {
            Debug.Log("[MangaTestDriver] Unlocking all chapters for testing...");
            for (int arc = 1; arc <= 5; arc++)
            {
                for (int ch = 1; ch <= 5; ch++)
                {
                    MangaManager.Instance.ForceUnlockChapter(arc, ch);
                }
            }
        }

        [ContextMenu("Test_ResetProgress")]
        public void Test_ResetProgress()
        {
            Debug.Log("[MangaTestDriver] Resetting progress...");
            MangaManager.Instance.ResetProgress();
        }
    }
}
