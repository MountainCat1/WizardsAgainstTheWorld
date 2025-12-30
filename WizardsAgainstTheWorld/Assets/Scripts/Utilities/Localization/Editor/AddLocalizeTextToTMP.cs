#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

namespace Utilities.Editor
{
    public class AddLocalizeTextToTMP : EditorWindow
    {
        [MenuItem("Tools/Add LocalizeText to TMP_Texts (Scene + Prefabs)")]
        public static void AddLocalizeTextToAllTMP()
        {
            int countScene = 0;
            int countPrefabs = 0;

            // Scene objects
            TMP_Text[] allSceneTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text tmp in allSceneTexts)
            {
                if (tmp.GetComponent<LocalizeText>() == null)
                {
                    Undo.AddComponent<LocalizeText>(tmp.gameObject);
                    countScene++;
                }
            }

            // Prefabs
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                bool changed = false;

                // Load prefab contents (safe editing)
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                TMP_Text[] tmpTexts = root.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text tmp in tmpTexts)
                {
                    if (tmp.GetComponent<LocalizeText>() == null)
                    {
                        Undo.AddComponent<LocalizeText>(tmp.gameObject); // works in editor, tracks changes
                        changed = true;
                        countPrefabs++;
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            Debug.Log($"LocalizeText added to {countScene} TMP_Text objects in scenes, {countPrefabs} in prefabs.");
        }
    }
}
#endif
