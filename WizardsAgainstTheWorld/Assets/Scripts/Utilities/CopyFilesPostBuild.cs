#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class CopyFilesPostBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // path to the build folder (same dir where .exe is)
        string buildDir = Path.GetDirectoryName(pathToBuiltProject);

        // source folder inside the Unity project
        string sourceDir = Path.Combine("Assets", "ExtraFiles");

        if (Directory.Exists(sourceDir))
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                if (file.EndsWith(".meta")) 
                    continue; // skip Unity .meta files

                string fileName = Path.GetFileName(file);
                string dest = Path.Combine(buildDir, fileName);
                File.Copy(file, dest, overwrite: true);
            }
        }
    }
}

#endif