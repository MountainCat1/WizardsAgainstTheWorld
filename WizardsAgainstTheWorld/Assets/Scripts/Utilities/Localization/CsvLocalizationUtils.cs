using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CsvLocalizationUtils
{
    public static string LocalizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");

    /// <summary>
    /// Parses a CSV file into a dictionary of key-value pairs, preserving multiline support.
    /// </summary>
    public static List<KeyValuePair<string, string>> ParseCsvWithMultilineSupport(string filePath)
    {
        var result = new List<KeyValuePair<string, string>>();

        if (!File.Exists(filePath))
        {
            GameLogger.LogWarning($"CSV file not found: {filePath}");
            return result;
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);

        // Skip header
        reader.ReadLine();

        string current = "";
        bool insideQuotedValue = false;

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (insideQuotedValue)
            {
                current += "\n" + line;
            }
            else
            {
                current = line;
            }

            // Count total quotes (escaped quotes are doubled so even counts are safe)
            int quoteCount = current.Count(c => c == '"');

            if (quoteCount % 2 == 0)
            {
                var split = SplitCsvLine(current);
                if (split.Count >= 2)
                {
                    string key = split[0].Trim().Trim('"');
                    string value = split[1].Trim().Trim('"')
                        .Replace("\"\"", "\"") // unescape double quotes
                        .Replace("\\n", "\n"); // interpret literal \n as newline

                    var filterResult = FilterOutPlaceholders(key, value);

                    if (filterResult)
                    {
                        result.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
                else
                {
                    // We still add valueless keys for the merger's sake
                    result.Add(new KeyValuePair<string, string>(current.Trim().Trim('"'), ""));
                }

                insideQuotedValue = false;
                current = "";
            }
            else
            {
                insideQuotedValue = true;
            }
        }

        return result;
    }

    private static bool FilterOutPlaceholders(string key, string value)
    {
        if (value.Contains("*TODO*"))
            return false;
        if (key.StartsWith("#"))
            return false;
        if (string.IsNullOrWhiteSpace(key))
            return false;
        
        return true;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var value = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value += '"'; // escaped quote
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(value);
                value = "";
            }
            else
            {
                value += c;
            }
        }

        result.Add(value);
        return result;
    }

    public static ICollection<string> ScanAvailableLanguages()
    {
        var availableLanguages = new List<string>();
        if (Directory.Exists(LocalizationPath))
        {
            foreach (string dir in Directory.GetDirectories(LocalizationPath))
            {
                string langCode = Path.GetFileName(dir);
                if (Directory.GetFiles(dir, "*.csv").Length > 0)
                {
                    availableLanguages.Add(langCode);
                }
            }
        }

        return availableLanguages;
    }
}