// TODO: remove this file when the new stats system is fully implemented
// using System.Collections.Generic;
// using UnityEngine;
// using Utilities;
//
// namespace Skills
// {
//     public abstract class SkillApply : MonoBehaviour
//     {
//         public abstract void Apply(SkillContext skillContext);
//         public abstract string GetDescription(int count = 1);
//         public abstract ICollection<SkillRow> GetSkillRows();
//
//
//         public class SkillRow
//         {
//             public Stat Key { get; set; }
//             public float Value { get; set; }
//             public StatsType StatsType { get; set; }
//             
//             public float FormattedValue
//             {
//                 get
//                 {
//                     return StatsType switch
//                     {
//                         StatsType.Percentage => Mathf.RoundToInt(Value / 100f),
//                         StatsType.Flat => Mathf.RoundToInt(Value * 100) / 100f,
//                         StatsType.Benefit => 1f,
//                         _ => throw new System.ArgumentOutOfRangeException(nameof(StatsType), StatsType, "Unknown StatsType")
//                     };
//                 }
//             }
//         }
//     }
// }