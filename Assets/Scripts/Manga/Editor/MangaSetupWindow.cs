using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace ChefJourney.Manga.Editor
{
    public class MangaSetupWindow : EditorWindow
    {
        [MenuItem("Chef's Journey/Manga/Setup UI Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<MangaSetupWindow>("Manga UI Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Manga UI Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this to generate the base prefabs for the Manga System. It will create Canvas setups with the necessary components.", MessageType.Info);

            if (GUILayout.Button("Generate MangaReader Canvas"))
            {
                GenerateMangaReader();
            }

            if (GUILayout.Button("Generate MangaGallery Canvas"))
            {
                GenerateMangaGallery();
            }

            if (GUILayout.Button("Generate MangaUnlockPopup"))
            {
                GenerateUnlockPopup();
            }
        }

        private void GenerateMangaReader()
        {
            GameObject canvasObj = new GameObject("MangaReaderCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject readerRoot = new GameObject("ReaderRoot");
            readerRoot.transform.SetParent(canvasObj.transform, false);
            var rrRect = readerRoot.AddComponent<RectTransform>();
            rrRect.anchorMin = Vector2.zero;
            rrRect.anchorMax = Vector2.one;
            rrRect.sizeDelta = Vector2.zero;

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(readerRoot.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = Color.black;
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.sizeDelta = Vector2.zero;

            GameObject panelImageObj = new GameObject("PanelImage");
            panelImageObj.transform.SetParent(readerRoot.transform, false);
            Image panelImage = panelImageObj.AddComponent<Image>();
            panelImage.preserveAspect = true;
            panelImage.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
            panelImage.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
            panelImage.rectTransform.sizeDelta = Vector2.zero;

            GameObject narratorBox = new GameObject("NarratorBox");
            narratorBox.transform.SetParent(readerRoot.transform, false);
            Image nBoxImg = narratorBox.AddComponent<Image>();
            nBoxImg.color = new Color(0,0,0,0.8f);
            nBoxImg.rectTransform.anchorMin = new Vector2(0.1f, 0.8f);
            nBoxImg.rectTransform.anchorMax = new Vector2(0.9f, 0.95f);
            nBoxImg.rectTransform.sizeDelta = Vector2.zero;
            
            GameObject narratorText = new GameObject("NarratorText");
            narratorText.transform.SetParent(narratorBox.transform, false);
            var nText = narratorText.AddComponent<TextMeshProUGUI>();
            RectTransform nTextRect = nText.GetComponent<RectTransform>();
            nText.color = Color.white;
            nText.alignment = TextAlignmentOptions.TopLeft;
            nText.fontSize = 24;
            nTextRect.anchorMin = Vector2.zero;
            nTextRect.anchorMax = Vector2.one;
            nTextRect.sizeDelta = new Vector2(-20, -20); // Margins

            GameObject speechBubble = new GameObject("SpeechBubble");
            speechBubble.transform.SetParent(readerRoot.transform, false);
            Image sBubbleImg = speechBubble.AddComponent<Image>();
            sBubbleImg.color = Color.white;
            sBubbleImg.rectTransform.anchorMin = new Vector2(0.2f, 0.1f);
            sBubbleImg.rectTransform.anchorMax = new Vector2(0.8f, 0.3f);
            sBubbleImg.rectTransform.sizeDelta = Vector2.zero;

            GameObject dialogueText = new GameObject("DialogueText");
            dialogueText.transform.SetParent(speechBubble.transform, false);
            var dText = dialogueText.AddComponent<TextMeshProUGUI>();
            RectTransform dTextRect = dText.GetComponent<RectTransform>();
            dText.color = Color.black;
            dText.alignment = TextAlignmentOptions.Center;
            dText.fontSize = 28;
            dTextRect.anchorMin = Vector2.zero;
            dTextRect.anchorMax = Vector2.one;
            dTextRect.sizeDelta = new Vector2(-20, -20);

            MangaReaderUI readerUI = canvasObj.AddComponent<MangaReaderUI>();
            SerializedObject so = new SerializedObject(readerUI);
            so.FindProperty("readerRoot").objectReferenceValue = readerRoot;
            so.FindProperty("backgroundImage").objectReferenceValue = bg;
            so.FindProperty("panelImageDisplay").objectReferenceValue = panelImage;
            so.FindProperty("narratorTextDisplay").objectReferenceValue = nText;
            so.FindProperty("dialogueTextDisplay").objectReferenceValue = dText;
            so.FindProperty("speechBubble").objectReferenceValue = speechBubble;
            so.FindProperty("narratorBox").objectReferenceValue = narratorBox;
            so.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Manga")) AssetDatabase.CreateFolder("Assets/Prefabs", "Manga");
            PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/Manga/MangaReaderCanvas.prefab");
            DestroyImmediate(canvasObj);
            Debug.Log("[MangaSetup] Generated MangaReaderCanvas.prefab");
        }

        private void GenerateMangaGallery()
        {
            GameObject canvasObj = new GameObject("MangaGalleryCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject galleryRoot = new GameObject("GalleryRoot");
            galleryRoot.transform.SetParent(canvasObj.transform, false);
            var grRect = galleryRoot.AddComponent<RectTransform>();
            grRect.anchorMin = Vector2.zero;
            grRect.anchorMax = Vector2.one;
            grRect.sizeDelta = Vector2.zero;
            
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(galleryRoot.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.sizeDelta = Vector2.zero;

            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(galleryRoot.transform, false);
            Image topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            topBarImg.rectTransform.anchorMin = new Vector2(0, 0.9f);
            topBarImg.rectTransform.anchorMax = new Vector2(1, 1);
            topBarImg.rectTransform.sizeDelta = Vector2.zero;
            
            GameObject arcTabs = new GameObject("ArcTabContainer");
            arcTabs.transform.SetParent(topBar.transform, false);
            HorizontalLayoutGroup arcTabsLayout = arcTabs.AddComponent<HorizontalLayoutGroup>();
            arcTabsLayout.childAlignment = TextAnchor.MiddleCenter;
            arcTabsLayout.childControlHeight = true;
            arcTabsLayout.childControlWidth = true;
            RectTransform arcTabsRect = arcTabs.GetComponent<RectTransform>();
            arcTabsRect.anchorMin = Vector2.zero;
            arcTabsRect.anchorMax = Vector2.one;
            arcTabsRect.sizeDelta = Vector2.zero;

            GameObject gridContainer = new GameObject("ChapterGridContainer");
            gridContainer.transform.SetParent(galleryRoot.transform, false);
            GridLayoutGroup gridLayout = gridContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(250, 350);
            gridLayout.spacing = new Vector2(20, 20);
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0);
            gridRect.anchorMax = new Vector2(1, 0.9f);
            gridRect.sizeDelta = new Vector2(0, 0);

            MangaGalleryUI galleryUI = canvasObj.AddComponent<MangaGalleryUI>();
            SerializedObject so = new SerializedObject(galleryUI);
            so.FindProperty("galleryRoot").objectReferenceValue = galleryRoot;
            so.FindProperty("arcTabContainer").objectReferenceValue = arcTabs.transform;
            so.FindProperty("chapterGridContainer").objectReferenceValue = gridContainer.transform;
            so.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Manga")) AssetDatabase.CreateFolder("Assets/Prefabs", "Manga");
            PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/Manga/MangaGalleryCanvas.prefab");
            DestroyImmediate(canvasObj);
            Debug.Log("[MangaSetup] Generated MangaGalleryCanvas.prefab");
        }

        private void GenerateUnlockPopup()
        {
            GameObject canvasObj = new GameObject("MangaUnlockPopupCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject popupRoot = new GameObject("PopupRoot");
            popupRoot.transform.SetParent(canvasObj.transform, false);
            var prRect = popupRoot.AddComponent<RectTransform>();
            prRect.anchorMin = Vector2.zero;
            prRect.anchorMax = Vector2.one;
            prRect.sizeDelta = Vector2.zero;
            CanvasGroup cg = popupRoot.AddComponent<CanvasGroup>();

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(popupRoot.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0,0,0,0.8f);
            bgImg.rectTransform.anchorMin = Vector2.zero;
            bgImg.rectTransform.anchorMax = Vector2.one;
            bgImg.rectTransform.sizeDelta = Vector2.zero;

            GameObject content = new GameObject("ContentBox");
            content.transform.SetParent(popupRoot.transform, false);
            var contentImage = content.AddComponent<Image>();
            contentImage.color = Color.white;
            contentImage.rectTransform.sizeDelta = new Vector2(600, 300);

            GameObject titleText = new GameObject("TitleText");
            titleText.transform.SetParent(content.transform, false);
            var tText = titleText.AddComponent<TextMeshProUGUI>();
            RectTransform titleRect = tText.GetComponent<RectTransform>();
            tText.text = "A New Tale Unlocked!";
            tText.color = Color.black;
            tText.alignment = TextAlignmentOptions.Center;
            tText.fontSize = 36;
            tText.fontStyle = FontStyles.Bold;
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(0, 50);

            GameObject chapterText = new GameObject("ChapterText");
            chapterText.transform.SetParent(content.transform, false);
            var cText = chapterText.AddComponent<TextMeshProUGUI>();
            RectTransform chapterRect = cText.GetComponent<RectTransform>();
            cText.color = Color.black;
            cText.alignment = TextAlignmentOptions.Center;
            cText.fontSize = 24;
            chapterRect.anchorMin = new Vector2(0, 0.5f);
            chapterRect.anchorMax = new Vector2(1, 0.5f);
            chapterRect.pivot = new Vector2(0.5f, 0.5f);
            chapterRect.anchoredPosition = Vector2.zero;
            chapterRect.sizeDelta = new Vector2(0, 100);

            GameObject buttonGrp = new GameObject("ButtonGroup");
            buttonGrp.transform.SetParent(content.transform, false);
            var bgRect = buttonGrp.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(0.5f, 0);
            bgRect.anchoredPosition = new Vector2(0, 20);
            bgRect.sizeDelta = new Vector2(0, 60);

            MangaUnlockPopup popup = canvasObj.AddComponent<MangaUnlockPopup>();
            SerializedObject so = new SerializedObject(popup);
            so.FindProperty("popupRoot").objectReferenceValue = popupRoot;
            so.FindProperty("popupCanvasGroup").objectReferenceValue = cg;
            so.FindProperty("titleText").objectReferenceValue = tText;
            so.FindProperty("chapterNameText").objectReferenceValue = cText;
            so.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Manga")) AssetDatabase.CreateFolder("Assets/Prefabs", "Manga");
            PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/Manga/MangaUnlockPopup.prefab");
            DestroyImmediate(canvasObj);
            Debug.Log("[MangaSetup] Generated MangaUnlockPopup.prefab");
        }
    }
}
