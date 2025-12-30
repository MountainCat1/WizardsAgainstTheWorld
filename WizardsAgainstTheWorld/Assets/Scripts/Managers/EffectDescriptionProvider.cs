using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Components.Creatures;
using Items.PassiveItems;
using Utilities;

namespace Managers
{
    public interface IEffectDescriptionProvider
    {
        string GetDescription(ICollection<PassiveEffect> allEffects);
    }
    
    public class EffectDescriptionProvider : IEffectDescriptionProvider
    {
        // public string GetDescription(PassiveEffect effect)
        // {
        //     
        // }

        public string GetDescription(ICollection<PassiveEffect> allEffects)
        {
            var groupedByType = allEffects.GroupBy(effect => effect.GetType());
            var sb = new StringBuilder();

            foreach (var group in groupedByType)
            {
                var representative = group.FirstOrDefault();
                if (representative is null) continue;

                switch (representative)
                {
                    case ModifierEffect:
                        AppendModifierEffectDescription(sb, group.OfType<ModifierEffect>());
                        break;

                    case RegenerationEffect:
                        AppendRegenerationDescription(sb, group.OfType<RegenerationEffect>());
                        break;
                    
                    case StatusEffectAmmoEffect:
                        AppendStatusEffectAmmoDescription(sb, group.OfType<StatusEffectAmmoEffect>());
                        break;

                    // Add more case handlers for other PassiveEffect types if needed

                    default:
                        // Optional: handle unknown effect types
                        break;
                }
            }

            return sb.ToString().TrimEnd();
        }

        private void AppendStatusEffectAmmoDescription(StringBuilder sb, IEnumerable<StatusEffectAmmoEffect> effects)
        {
            var statusEffects = effects.Select(x => x.StatusEffectPrefab);
            
            if (!statusEffects.Any()) return;

            foreach (var statusEffect in statusEffects)
            {
                if (statusEffect == null)
                {
                    GameLogger.LogError("StatusEffectAmmoEffect: StatusEffectPrefab is null.");
                    continue;
                }

                var statusEffectName = StringUtilities.WrapInColor(statusEffect.NameKey.Localize(), true);
                sb.AppendLine($"UI.Effects.ProjectileStatusEffect".Localize($"{statusEffectName}"));
            }
        }

        private void AppendRegenerationDescription(StringBuilder sb, IEnumerable<RegenerationEffect> effects)
        {
            float regenPerSec = effects.Sum(e => e.RegenerationAmount / e.RegenerationInterval);
            
            if (MathF.Abs(regenPerSec) < 0.05f) return; // Ignore small values
            
            var valueText = StringUtilities.WrapInColor(regenPerSec);
            
            sb.AppendLine($"UI.Effects.RegenerationEffect".Localize(valueText));
        }

        private void AppendModifierEffectDescription(StringBuilder sb, IEnumerable<ModifierEffect> effects)
        {
            var groupedModifiers = effects
                .SelectMany(e => e.Modifier.ToDictionary())
                .GroupBy(kvp => kvp.Key)
                .ToList();

            foreach (var modifier in groupedModifiers)
            {
                var value = modifier.Sum(kvp => kvp.Value);
                if (MathF.Abs(value) < 0.05f) continue; // Ignore small values
                
                var key = modifier.Key;
                
                var usePercentage = Modifier.GetModifierStatType(key) == StatsType.Percentage;
                
                var valueText = usePercentage 
                    ? StringUtilities.WrapInColorPercentage(value)
                    : StringUtilities.WrapInColor(value);
                
                sb.AppendLine($"UI.StatChange.{key}".Localize(valueText));
            }
        }
    }

}