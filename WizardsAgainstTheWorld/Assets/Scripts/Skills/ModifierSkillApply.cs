// TODO: Remove this comment when the code is complete
// using System.Collections.Generic;
// using UnityEngine;
// using Utilities;
//
// namespace Skills
// {
//     public class ModifierSkillApply : SkillApply
//     {
//         [SerializeField] private ModifierTemplate modifierTemplate;
//         
//         public override void Apply(SkillContext skillContext)
//         {
//             skillContext.Creature.ModifierReceiver.AddModifier(modifierTemplate.ToModifier());
//         }
//
//         public override string GetDescription(int count = 1)
//         {
//             var modifier = modifierTemplate.ToModifier();
//             var description = string.Empty;
//
//             if (modifier.SpeedModifier != null)
//                 description += $"Speed: {StringUtilities.WrapInColorPercentage(modifier.SpeedModifier * count)}\n";
//             if (modifier.DamageModifier != null)
//                 description += $"Damage: {StringUtilities.WrapInColorPercentage(modifier.DamageModifier * count)}\n";
//             if (modifier.AttackSpeedModifier != null)
//                 description += $"Attack Speed: {StringUtilities.WrapInColorPercentage(modifier.AttackSpeedModifier * count)}\n";
//             if (modifier.AccuracyFlatModifier != null)
//                 description += $"Accuracy: {StringUtilities.WrapInColorPercentage(modifier.AccuracyFlatModifier * count)}\n";
//             if (modifier.ArmorFlatModifier != null)
//                 description += $"Armor: {StringUtilities.WrapInColor(modifier.ArmorFlatModifier * count)}\n";
//
//             return description;
//         }
//
//         public override ICollection<SkillRow> GetSkillRows()
//         {
//             var modifier = modifierTemplate.ToModifier();
//             var skillRows = new List<SkillRow>();
//
//             if (modifier.SpeedModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.Speed, Value = modifier.SpeedModifier.Value, StatsType = StatsType.Percentage });
//
//             if (modifier.DamageModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.DamageModifier, Value = modifier.DamageModifier.Value, StatsType = StatsType.Percentage });
//
//             if (modifier.AttackSpeedModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.AttackSpeedModifier, Value = modifier.AttackSpeedModifier.Value, StatsType = StatsType.Percentage });
//
//             if (modifier.AccuracyFlatModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.AccuracyFlat, Value = modifier.AccuracyFlatModifier.Value, StatsType = StatsType.Percentage });
//
//             if (modifier.ArmorFlatModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.ArmorFlat, Value = modifier.ArmorFlatModifier.Value, StatsType = StatsType.Flat });
//
//             if (modifier.ArmorPercentageModifier != null)
//                 skillRows.Add(new SkillRow { Key = Stat.ArmorPercentage, Value = modifier.ArmorPercentageModifier.Value, StatsType = StatsType.Percentage });
//
//             return skillRows;
//         }
//
//     }
// }