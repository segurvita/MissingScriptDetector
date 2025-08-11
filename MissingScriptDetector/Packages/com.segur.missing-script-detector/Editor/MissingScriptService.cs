using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MissingScriptDetector.Editor
{
    /// <summary>
    /// Business logic class for searching and removing MissingScript
    /// </summary>
    public class MissingScriptService
    {
        /// <summary>
        /// Searches for GameObjects with MissingScript from the specified GameObject and its children
        /// </summary>
        /// <param name="targetGameObject">Target GameObject to search</param>
        /// <returns>List of GameObjects with MissingScript</returns>
        public List<GameObject> FindGameObjectsWithMissingScripts(GameObject targetGameObject)
        {
            var results = new List<GameObject>();
            
            if (targetGameObject == null)
            {
                return results;
            }
            
            SearchMissingScriptsRecursive(targetGameObject, results);
            return results;
        }
        
        /// <summary>
        /// Removes MissingScript from the specified GameObject
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        public void RemoveMissingScripts(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
        
        /// <summary>
        /// Removes MissingScript from all GameObjects in the specified list
        /// </summary>
        /// <param name="gameObjects">Target GameObject list</param>
        public void RemoveAllMissingScripts(List<GameObject> gameObjects)
        {
            if (gameObjects == null) return;
            
            foreach (var gameObject in gameObjects)
            {
                RemoveMissingScripts(gameObject);
            }
        }
        
        /// <summary>
        /// Recursively searches for MissingScript from GameObject and its children
        /// </summary>
        /// <param name="gameObject">Target GameObject to search</param>
        /// <param name="results">List to store results</param>
        private void SearchMissingScriptsRecursive(GameObject gameObject, List<GameObject> results)
        {
            // Check MissingScript in current GameObject
            Component[] components = gameObject.GetComponents<Component>();
            bool hasMissingScript = false;
            
            foreach (var component in components)
            {
                if (component == null)
                {
                    hasMissingScript = true;
                    break;
                }
            }
            
            if (hasMissingScript)
            {
                results.Add(gameObject);
            }
            
            // Recursively search child objects
            foreach (Transform child in gameObject.transform)
            {
                SearchMissingScriptsRecursive(child.gameObject, results);
            }
        }
    }
}
