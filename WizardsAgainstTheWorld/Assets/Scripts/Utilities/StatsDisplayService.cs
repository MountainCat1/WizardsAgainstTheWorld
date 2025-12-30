// TODO: remove this file when the new stats system is fully implemented

// using System.Collections.Generic;
// using UnityEngine;
// using System.Globalization;
// using System.Text;
// using static Utilities.LocalizationHelper;
//
// namespace Utilities
// {
//     
//     public class Stats : Dictionary<Stat, float>
//     {
//         public static string GetNameLabelKey(Stat stat) => $"UI.Stat.{stat}";
//         
//         public Stats() : base()
//         {
//         }
//
//         public Stats(IDictionary<Stat, float> stats) : base(stats)
//         {
//         }
//     }
//
//     public struct StatsDisplayRow
//     {
//         public Stat Key { get; }
//         public float? BaseValue { get; }
//         public float Modifier { get; }
//         public StatsType Type { get; }
//
//         public StatsDisplayRow(Stat key, float? baseValue, float modifier, StatsType type)
//         {
//             Key = key;
//             BaseValue = baseValue;
//             Modifier = modifier;
//             Type = type;
//         }
//     }
//
//
//     public interface IStatsDisplayService
//     {
//         string GetStatsDisplay(ICollection<StatsDisplayRow> statsRows);
//     }
//
//     public class StatsDisplayService : IStatsDisplayService
//     {
//         public string GetStatsDisplay(ICollection<StatsDisplayRow> statsRows)
//         {
//             Color negativeColor = new Color(1f, 0.2f, 0.2f);
//             Color positiveColor = new Color(0.2f, 1f, 0.2f);
//
//             var display = new StringBuilder();
//
//             foreach (var row in statsRows)
//             {
//                 string statLabelKey = Stats.GetNameLabelKey(row.Key);
//                 string statLabel = L(statLabelKey);
//                 string statValue = "";
//
//                 // Handle Benefit type (use Modifier as label)
//                 if (row.Type == StatsType.Benefit)
//                 {
//                     statValue = WrapInColor(row.Modifier.ToString(CultureInfo.InvariantCulture), positiveColor);
//                 }
//                 else
//                 {
//                     if (row.BaseValue.HasValue)
//                     {
//                         statValue += FormatSigned(row.BaseValue.Value, row.Type == StatsType.Percentage);
//                     }
//
//                     if (Mathf.Abs(row.Modifier) > 0.0001f)
//                     {
//                         Color color = row.Modifier >= 0 ? positiveColor : negativeColor;
//                         string modifierStr = WrapInColor(
//                             FormatSigned(row.Modifier, row.Type == StatsType.Percentage),
//                             color
//                         );
//
//                         if (!string.IsNullOrEmpty(statValue))
//                             statValue += " ";
//
//                         statValue += modifierStr;
//                     }
//
//                     // If neither base nor modifier is present, fallback to a neutral 0
//                     if (string.IsNullOrEmpty(statValue))
//                     {
//                         statValue = "0";
//                     }
//                 }
//
//                 display.AppendLine($"{statLabel}: {statValue}");
//             }
//
//             return display.ToString().TrimEnd('\n');
//         }
//
//         private string FormatSigned(float value, bool percentage = false)
//         {
//             Debug.Log($"Formatting value: {value}");
//             if (percentage)
//             {
//                 value *= 100f; // Convert to percentage
//                 string formatted = value >= 0 ? $"+{value:0.#}" : value.ToString("0.#"); // No padding
//                 return formatted + "%";
//             }
//             else
//             {
//                 string formatted = value >= 0 ? $"+{value:0.00}" : value.ToString("0.00"); // Always 2 decimals
//                 return formatted;
//             }
//         }
//
//
//         private string WrapInColor(string text, Color color)
//         {
//             return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
//         }
//     }
// }