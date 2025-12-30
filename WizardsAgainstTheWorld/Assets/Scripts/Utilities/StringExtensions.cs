using System.Globalization;

namespace Utilities
{
    public static class StringExtensions
    {
        public static string ToLocalizedString2Decimals(this float value)
            => value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string ToLocalizedString2Decimals(this double value)
            => value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string ToLocalizedString2Decimals(this decimal value)
            => value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}