using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ReplaceFontsEditor : EditorWindow
{
    private Font uiFont;
    private TMP_FontAsset tmpFont;

    private bool replaceUIText = true;
    private bool replaceTMPUGUI = true;
    private bool replaceTMP3D = true;

    [MenuItem("Tools/Replace Fonts In Scene & Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceFontsEditor>("Replace Fonts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Font Replacement Tool", EditorStyles.boldLabel);

        uiFont = (Font)EditorGUILayout.ObjectField("UI Font", uiFont, typeof(Font), false);
        tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", tmpFont, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        GUILayout.Label("Replace in:", EditorStyles.boldLabel);
        replaceUIText = EditorGUILayout.Toggle("UI Text", replaceUIText);
        replaceTMPUGUI = EditorGUILayout.Toggle("TextMeshProUGUI", replaceTMPUGUI);
        replaceTMP3D = EditorGUILayout.Toggle("TextMeshPro (3D)", replaceTMP3D);

        EditorGUILayout.Space();

        if (GUILayout.Button("Replace Fonts In Scene"))
        {
            ReplaceFontsInScene();
        }

        if (GUILayout.Button("Replace Fonts In Prefabs"))
        {
            ReplaceFontsInPrefabs();
        }
    }

    private void ReplaceFontsInScene()
    {
        int count = 0;

        if (replaceUIText && uiFont != null)
        {
            var uiTexts = GameObject.FindObjectsOfType<Text>(true);
            foreach (var text in uiTexts)
            {
                Undo.RecordObject(text, "Replace UI Font");
                text.font = uiFont;
                EditorUtility.SetDirty(text);
                count++;
            }
        }

        if (tmpFont != null)
        {
            if (replaceTMPUGUI)
            {
                var tmpUIs = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
                foreach (var tmp in tmpUIs)
                {
                    Undo.RecordObject(tmp, "Replace TMP UGUI Font");
                    tmp.font = tmpFont;
                    EditorUtility.SetDirty(tmp);
                    count++;
                }
            }

            if (replaceTMP3D)
            {
                var tmp3Ds = GameObject.FindObjectsOfType<TextMeshPro>(true);
                foreach (var tmp in tmp3Ds)
                {
                    Undo.RecordObject(tmp, "Replace TMP 3D Font");
                    tmp.font = tmpFont;
                    EditorUtility.SetDirty(tmp);
                    count++;
                }
            }
        }

        Debug.Log($"[Scene] Replaced fonts on {count} text components.");
    }

    private void ReplaceFontsInPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (var guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool modified = false;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (replaceUIText && uiFont != null)
            {
                foreach (var text in instance.GetComponentsInChildren<Text>(true))
                {
                    text.font = uiFont;
                    EditorUtility.SetDirty(text);
                    modified = true;
                    count++;
                }
            }

            if (tmpFont != null)
            {
                if (replaceTMPUGUI)
                {
                    foreach (var tmp in instance.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        tmp.font = tmpFont;
                        EditorUtility.SetDirty(tmp);
                        modified = true;
                        count++;
                    }
                }

                if (replaceTMP3D)
                {
                    foreach (var tmp in instance.GetComponentsInChildren<TextMeshPro>(true))
                    {
                        tmp.font = tmpFont;
                        EditorUtility.SetDirty(tmp);
                        modified = true;
                        count++;
                    }
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }

            DestroyImmediate(instance);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Prefabs] Replaced fonts on {count} text components.");
    }
}
