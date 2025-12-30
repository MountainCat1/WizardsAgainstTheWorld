#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// Required for OpenScene
// Required for Path

// Required for runtime SceneManager

namespace Utilities.Editor
{
    public class SceneSwitcherWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Scene Switcher")]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            SceneSwitcherWindow window = (SceneSwitcherWindow)EditorWindow.GetWindow(typeof(SceneSwitcherWindow));
            window.titleContent = new GUIContent("Scene Switcher");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Scenes in Build Settings", EditorStyles.boldLabel);

            if (EditorBuildSettings.scenes.Length == 0)
            {
                EditorGUILayout.HelpBox("No scenes found in Build Settings. Please add scenes via File > Build Settings.", MessageType.Info);
                if (GUILayout.Button("Open Build Settings"))
                {
                    EditorApplication.ExecuteMenuItem("File/Build Settings...");
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.enabled) // Only show scenes that are ticked in Build Settings
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    string scenePath = scene.path;

                    EditorGUILayout.BeginHorizontal();

                    // Display scene name and index
                    GUILayout.Label($"[{i}] {sceneName}", GUILayout.Width(position.width - 80));

                    // Button to load the scene
                    if (GUILayout.Button("Load", GUILayout.Width(60)))
                    {
                        // Ask to save the current scene if modified
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            // If in Play mode, use SceneManager
                            if (Application.isPlaying)
                            {
                                SceneManager.LoadScene(scenePath);
                            }
                            // If in Edit mode, use EditorSceneManager
                            else
                            {
                                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    EditorGUILayout.LabelField($"[{i}] {sceneName} (Disabled in Build Settings)");
                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Refresh List / Open Build Settings"))
            {
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
                // Repaint the window to reflect any changes immediately if build settings were modified.
                Repaint();
            }
        }

        // Optional: Refresh the list when the window gets focus, or when build settings change
        void OnFocus()
        {
            Repaint();
        }

        void OnProjectChange() // Called when assets change, including build settings
        {
            Repaint();
        }
    }
}

#endif