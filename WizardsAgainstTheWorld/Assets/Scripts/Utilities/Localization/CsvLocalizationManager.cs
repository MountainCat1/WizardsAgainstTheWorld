using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public enum AvailableFont
{
    PixelArt,
    Normal
}

[Serializable]
public class FontAssetKeyPair
{
    public AvailableFont fontType;
    public TMP_FontAsset fontAsset;
}

public class CsvLocalizationManager : MonoBehaviour
{
    public static CsvLocalizationManager Instance { get; private set; }

    [SerializeField] private List<FontAssetKeyPair> fontAssets = new();
    
    private Dictionary<string, string> _localizedTexts = new();
    private Dictionary<string, string> _fallbackTexts = new(); 
    
    public string CurrentLanguage { get; private set; }
    public TMP_FontAsset CurrentFontAsset { get; private set; }
    
    public bool FallbackToDefaultLanguage { get; private set; } = true;
    public ICollection<string> AvailableLanguages { get; private set; } = new List<string>();
    public event Action LanguageChanged;
    public event Action FontChanged;

    private string _basePath;
    private AvailableFont _currentFontType;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _basePath = CsvLocalizationUtils.LocalizationPath;
            AvailableLanguages = CsvLocalizationUtils.ScanAvailableLanguages();
            Debug.Log($"[Localization] Available languages: {string.Join(", ", AvailableLanguages)}");
            
            LoadLanguage(GameSettings.Instance.Language, GameSettings.Instance.FallbackToDefaultLanguage); 
            
            SetFont(GameSettings.Instance.UsePixelArtFont ? AvailableFont.PixelArt : AvailableFont.Normal);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Display list of available languages in the console
        Debug.Log($"[Localization] Available languages: {string.Join(", ", AvailableLanguages)}");
    }

    public void SetFont(AvailableFont font)
    {
        _currentFontType = font;
        CurrentFontAsset = fontAssets.First(f => f.fontType == font).fontAsset;
        
        FontChanged?.Invoke();
    }

    public void LoadLanguage(string languageCode, bool fallbackToDefault)
    {
        _localizedTexts = LoadLanguageEntries(languageCode);
        CurrentLanguage = languageCode;
        
        FallbackToDefaultLanguage = fallbackToDefault;
        if (FallbackToDefaultLanguage)
        {
            // Load fallback language if enabled
            string fallbackCode = GameSettings.Instance.FallbackToDefaultLanguage ? "En" : languageCode;
            LoadFallbackLanguage(fallbackCode);
        }
        else
        {
            _fallbackTexts.Clear(); // Clear fallback texts if not using fallback
        }
        
        LanguageChanged?.Invoke();
    }

    private void LoadFallbackLanguage(string fallbackCode)
    {
        _fallbackTexts = LoadLanguageEntries(fallbackCode);
    }
    
    private Dictionary<string, string> LoadLanguageEntries(string languageCode)
    {
        string langPath = Path.Combine(_basePath, languageCode);

        if (!Directory.Exists(langPath))
        {
            Debug.LogWarning($"[Localization] Language folder not found: {langPath}");
            return new Dictionary<string, string>();
        }

        var texts = new Dictionary<string, string>();
        foreach (var file in Directory.GetFiles(langPath, "*.csv"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            var entries = CsvLocalizationUtils.ParseCsvWithMultilineSupport(file);
            foreach (var entry in entries)
            {
                string key = $"{fileName}.{entry.Key}";
                texts[key] = entry.Value;
            }
        }

        Debug.Log($"[Localization] Loaded '{languageCode}' with {texts.Count} keys from {Directory.GetFiles(langPath, "*.csv").Length} files.");
        return texts;
    }


    /// <summary>
    /// Returns the localized string as-is or a missing key placeholder.
    /// </summary>
    public string Get(string key)
    {
        if (_localizedTexts.TryGetValue(key, out var value))
            return value;

        if (FallbackToDefaultLanguage && _fallbackTexts.TryGetValue(key, out var fallback))
            return fallback;

        return $"<missing:{key}>";
    }

    /// <summary>
    /// Returns the localized string formatted with parameters.
    /// </summary>
    public string Get(string key, params object[] args)
    {
        string raw = Get(key);

        if (raw.StartsWith("<missing:") || raw.StartsWith("<format-error:"))
            return raw;

        try
        {
            return string.Format(raw, args);
        }
        catch
        {
            return $"<format-error:{key}>";
        }
    }
}
