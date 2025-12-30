namespace Utilities
{
    public static class LocalizationStringExtensions
    {
        public static string Localize(this string fullKey)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
            {
                return fullKey;
            }

            return LocalizationHelper.L(fullKey);
        }

        public static string Localize(this string fullKey, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
            {
                GameLogger.LogWarning("Localization key is null or empty.");
                return fullKey;
            }

            return LocalizationHelper.L(fullKey, args);
        }
    }
}