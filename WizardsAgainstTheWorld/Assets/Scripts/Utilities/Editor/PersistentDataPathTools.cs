using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Utilities.Editor
{
    public static class PersistentDataPathTools
    {
        private const string MenuRoot = "Tools/Data Paths/";

        [MenuItem(MenuRoot + "Open Persistent Data Path", priority = 10)]
        public static void OpenPersistentDataPath()
        {
            var path = Application.persistentDataPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Application.persistentDataPath is null or empty.");
                return;
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            OpenInFileBrowser(path);
            Debug.Log($"Opened: {path}");
        }

        [MenuItem(MenuRoot + "Copy Persistent Data Path", priority = 11)]
        public static void CopyPersistentDataPath()
        {
            var path = Application.persistentDataPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Application.persistentDataPath is null or empty.");
                return;
            }

            EditorGUIUtility.systemCopyBuffer = path;
            Debug.Log($"Copied to clipboard: {path}");
        }

        private static void OpenInFileBrowser(string path)
        {
#if UNITY_EDITOR_WIN
            // Explorer accepts forward or back slashes; normalize to backslashes.
            var normalized = path.Replace('/', '\\');
            var psi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{normalized}\"",
                UseShellExecute = true
            };
            Process.Start(psi);
#elif UNITY_EDITOR_OSX
            var psi = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
#else
            // Linux
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(psi);
            }
            catch
            {
                // Fallback (Unity might handle this on some distros)
                EditorUtility.RevealInFinder(path);
            }
#endif
        }
    }
}