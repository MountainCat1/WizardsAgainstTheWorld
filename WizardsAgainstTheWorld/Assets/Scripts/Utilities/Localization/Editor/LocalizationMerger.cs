#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utilities.Localization.Editor
{
    public static class LocalizationMerger
    {
        private const string BasePath = "Assets/StreamingAssets/Localization";
        private const string SourceLanguage = "En";

        [MenuItem("Tools/Localization/Merge Missing Keys")]
        public static void MergeMissingKeys()
        {
            if (!Directory.Exists(BasePath))
            {
                Debug.LogError($"Localization base folder not found: {BasePath}");
                return;
            }

            string sourceDir = Path.Combine(BasePath, SourceLanguage);
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source language folder '{SourceLanguage}' not found.");
                return;
            }

            var sourceFiles = Directory.GetFiles(sourceDir, "*.csv");
            var otherLanguages = CsvLocalizationUtils
                .ScanAvailableLanguages()
                .Except(new[] { SourceLanguage })
                .ToList();

            foreach (string lang in otherLanguages)
            {
                string targetDir = Path.Combine(BasePath, lang);
                foreach (string sourceFile in sourceFiles)
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string targetFile = Path.Combine(targetDir, fileName);
                    MergeFile(sourceFile, targetFile);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Localization merge complete!");
        }

        private static void MergeFile(string sourcePath, string targetPath)
        {
            var sourceLines = CsvLocalizationUtils.ParseCsvWithMultilineSupport(sourcePath);
            var targetLines = CsvLocalizationUtils.ParseCsvWithMultilineSupport(targetPath);

            // var sourceDict = CsvLocalizationUtils.ParseCsvToDictFromFile(sourcePath, out List<string> orderedKeys);
            // var targetDict = CsvLocalizationUtils.ParseCsvToDictFromFile(targetPath, out _);

            bool headerPresent = sourceLines.Count > 0 && sourceLines.First().Key.StartsWith("Key");
            var mergedLines = new List<string>();

            if (headerPresent)
                mergedLines.Add("Key,Value");
        
            var enumerable = headerPresent ? sourceLines.Skip(1) : sourceLines;

            foreach (var (key, value) in enumerable)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    mergedLines.Add("");
                    continue;
                }

                if (key.StartsWith("#"))
                {
                    mergedLines.Add(key);
                    continue;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    string mergedValue = targetLines.Any(x => x.Key == key) 
                        ? targetLines.First(x => x.Key == key).Value
                        : $"*TODO* {value}";
                    
                    mergedValue = mergedValue.Replace("\"", "\"\""); // Escape quotes for CSV
                    
                    mergedLines.Add($"{key},\"{mergedValue}\"");
                }
            }

            foreach (var kv in targetLines)
            {
                if (sourceLines.All(x => x.Key != kv.Key))
                {
                    mergedLines.Add($"NOT-FOUND-{kv.Key},{kv.Value}");
                }
            }

            string newLine = "\n";

            using (var writer = new StreamWriter(targetPath, false, new System.Text.UTF8Encoding(false)))
            {
                writer.NewLine = newLine;
                
                // Add header
                writer.WriteLine("Key,Value");
                
                foreach (var line in mergedLines)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}


#endif