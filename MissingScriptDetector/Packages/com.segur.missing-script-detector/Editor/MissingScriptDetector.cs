using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MissingScriptDetector.Editor
{
    public class MissingScriptDetector : EditorWindow
    {
        /// <summary>
        /// Variable to maintain scroll view position
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// List of GameObjects with MissingScript
        /// </summary>
        private List<GameObject> gameObjectsWithMissingScripts = new List<GameObject>();

        /// <summary>
        /// Target GameObject for search
        /// </summary>
        private GameObject targetGameObject;

        /// <summary>
        /// Service for searching and removing MissingScript
        /// </summary>
        private MissingScriptService missingScriptService = new MissingScriptService();

        [MenuItem("Tools/Missing Script Detector")]
        public static void ShowWindow()
        {
            GetWindow<MissingScriptDetector>("Missing Script Detector");
        }

        private void OnGUI()
        {
            GUILayout.Label("Missing Script Detector", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // GameObject selection field
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Target GameObject:", GUILayout.Width(120));
            targetGameObject = (GameObject)EditorGUILayout.ObjectField(targetGameObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Search button
            if (GUILayout.Button("Search MissingScript", GUILayout.Height(30)))
            {
                SearchMissingScripts();
            }

            GUILayout.Space(10);

            // Results display
            if (gameObjectsWithMissingScripts.Count > 0)
            {
                GUILayout.Label($"MissingScript found: {gameObjectsWithMissingScripts.Count} objects", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var gameObject in gameObjectsWithMissingScripts)
                {
                    EditorGUILayout.BeginHorizontal("box");

                    // GameObject name
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(gameObject.name, EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    // Buttons
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = gameObject;
                        EditorGUIUtility.PingObject(gameObject);
                    }

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm",
                            $"Remove MissingScript from GameObject '{gameObject.name}'?",
                            "Remove", "Cancel"))
                        {
                            missingScriptService.RemoveMissingScripts(gameObject);
                            SearchMissingScripts(); // Re-search
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);

                // Bulk remove button
                if (GUILayout.Button("Remove All MissingScript", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirm",
                        "Remove all detected MissingScript?",
                        "Remove", "Cancel"))
                    {
                        missingScriptService.RemoveAllMissingScripts(gameObjectsWithMissingScripts);
                        SearchMissingScripts(); // Re-search
                    }
                }
            }
            else
            {
                GUILayout.Label("No MissingScript found.", EditorStyles.helpBox);
            }
        }

        private void SearchMissingScripts()
        {
            gameObjectsWithMissingScripts.Clear();

            // Check target GameObject
            if (targetGameObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a target GameObject.", "OK");
                return;
            }

            // Use service to search for MissingScript
            gameObjectsWithMissingScripts = missingScriptService.FindGameObjectsWithMissingScripts(targetGameObject);
        }
    }
}
