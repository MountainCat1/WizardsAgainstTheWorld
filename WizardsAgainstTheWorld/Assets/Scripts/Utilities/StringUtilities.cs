using UnityEngine;

namespace Utilities
{
    public static class StringUtilities
    {
        public static string WrapInColor(float? f, bool reverted = false)
        {
            if (f == null)
                return "<color=red>0</color>";

            if (!reverted)
                return f > 0 ? $"<color=green>+{f:F2}</color>" : $"<color=red>{f:F2}</color>";
            else
                return f > 0 ? $"<color=red>+{f:F2}</color>" : $"<color=green>{f:F2}</color>";
        }
        
        public static string WrapInColor(int? i, bool reverted = false)
        {
            if (i == null)
                return "<color=red>0</color>";

            if (!reverted)
                return i > 0 ? $"<color=green>+{i}</color>" : $"<color=red>{i}</color>";
            else
                return i > 0 ? $"<color=red>+{i}</color>" : $"<color=green>{i}</color>";
        }

        public static string WrapInColor(string s, bool isPositive)
        {
            if (string.IsNullOrEmpty(s))
                return "<color=red>0</color>";

            return isPositive ? $"<color=green>{s}</color>" : $"<color=red>{s}</color>";
        }

        public static string WrapInColorPercentage(float? f)
        {
            if (f == null)
                return "<color=red>0%</color>";

            return f > 0 ? $"<color=green>+{(f * 100):F0}%</color>" : $"<color=red>{f * 100:F0}%</color>";
        }

        public static string StringifyFloat(float damage)
        {
            // Show one decimal if the value has a meaningful fractional part
            return (damage % 1f) < 0.001f ? damage.ToString("F0") : damage.ToString("F1");
        }

        public static string WrapInColor(string s, Color color)
        {
            if (string.IsNullOrEmpty(s))
                return "<color=red>0</color>";

            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{s}</color>";
        }
    }
}