using TMPro;

namespace Utilities
{
    public static class LocalizationHelper
    {
        // private const string MoneySymbol = "\u00a4";
        private const string MoneySymbol = "\u00a2";
        public static TMP_FontAsset FontAsset => CsvLocalizationManager.Instance.CurrentFontAsset;

        /// <summary>
        /// Gets a localized string by full key like "UI.Buttons.Start".
        /// </summary>
        public static string L(string fullKey)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
            {
                return fullKey;
            }
            
            var localizedString = CsvLocalizationManager.Instance.Get(fullKey);

            localizedString = localizedString.Replace("$", MoneySymbol);

            return localizedString;
        }

        /// <summary>
        /// Gets a localized string with format arguments.
        /// Example: L("UI.Buttons.Greeting", "Player") → "Hello, Player!"
        /// </summary>
        public static string L(string fullKey, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
            {
                GameLogger.LogWarning("Localization key is null or empty.");
                return fullKey;
            }
            
            var localizedString = CsvLocalizationManager.Instance.Get(fullKey, args);
            
            localizedString = localizedString.Replace("$", MoneySymbol);

            return localizedString;
        }
        
        public static string LMoneySymbol()
        {
            return MoneySymbol;
        }
    }
}