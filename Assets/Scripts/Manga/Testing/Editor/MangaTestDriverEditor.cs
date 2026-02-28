using UnityEngine;
using UnityEditor;

namespace ChefJourney.Testing.Editor
{
    [CustomEditor(typeof(MangaTestDriver))]
    public class MangaTestDriverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector (so we can still see the fields)
            DrawDefaultInspector();

            MangaTestDriver script = (MangaTestDriver)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Testing Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Simulate Level Complete"))
            {
                script.Test_CompleteLevel();
            }

            if (GUILayout.Button("Open Manga Gallery"))
            {
                script.Test_OpenGallery();
            }

            if (GUILayout.Button("Unlock All Chapters"))
            {
                script.Test_UnlockAll();
            }

            if (GUILayout.Button("Reset Progress"))
            {
                script.Test_ResetProgress();
            }
        }
    }
}
