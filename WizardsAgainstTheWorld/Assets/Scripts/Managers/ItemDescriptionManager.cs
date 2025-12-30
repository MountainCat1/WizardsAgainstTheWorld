using System;
using System.Linq;
using System.Text;
using Combat;
using Items;
using Items.PassiveItems;
using Items.Weapons;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers
{
    public interface IItemDescriptionManager
    {
        string GetDescription(ItemData item);
        string GetInfoName(ItemData item);
    }

    public class ItemDescriptionManager : IItemDescriptionManager
    {
        [Inject] private IItemManager _itemManager;
        [Inject] private IEffectDescriptionProvider _effectDescriptionProvider;

        public string GetDescription(ItemData data)
        {
            var item = _itemManager.GetItemPrefab(data.Identifier);

            return item switch
            {
                Weapon weapon => GetWeaponDescription(data, weapon),
                ArmorItem armor => GetArmorDescription(data, armor),
                PassiveItemBehaviour passiveItem => GetPassiveItemDescription(data, passiveItem),
                _ => $"{GetLocalizedDescription(item)}\n\n{GetItemValueLine(data)}"
            };
        }

        private string GetLocalizedDescription(ItemBehaviour item)
        {
            return $"{item.DescriptionKey}".Localize();
        }

        private string GetPassiveItemDescription(ItemData data, PassiveItemBehaviour passiveItem)
        {
            var builder = new StringBuilder();
            var baseDescription = passiveItem.DescriptionKey.Localize() ?? "";

            builder.AppendLine(baseDescription);
            builder.AppendLine();

            var effects = passiveItem.Effects;
            // if (effects != null && effects.Any())
            // {
            //     foreach (var effect in effects)
            //     {
            //         switch (effect)
            //         {
            //             case ModifierEffect modifierItem:
            //                 builder.Append(GetModifierDescription(data, modifierItem));
            //                 break;
            //             case RegenerationEffect regen:
            //                 var regenPerSec = regen.RegenerationAmount / regen.RegenerationInterval;
            //                 var valueText = StringUtilities.WrapInColor(regenPerSec);
            //                 builder.AppendLine("Items.Effects.Regeneration".Localize(valueText));
            //                 break;
            //             case StatusEffectAmmoEffect:
            //                 builder.AppendLine("Items.Effects.AmmoStatusEffect".Localize());
            //                 break;
            //             default:
            //                 GameLogger.LogWarning($"Unknown effect type: {effect.GetType()} in passive item {passiveItem.name}");
            //                 break;
            //         }
            //     }
            // }
            // else if(string.IsNullOrEmpty(passiveItem.DescriptionKey))
            // {
            //     builder.AppendLine("Items.NoDescription".Localize());
            // }
            var effectsDescription = _effectDescriptionProvider.GetDescription(effects.ToList());

            if (!string.IsNullOrEmpty(effectsDescription))
            {
                builder.AppendLine(effectsDescription);
            }
            else
            {
                builder.AppendLine("Items.NoDescription".Localize());
            }

            AppendItemValue(builder, data);

            return builder.ToString();
        }

        public string GetInfoName(ItemData item)
        {
            var modifiers = item.Modifiers.OfType<WeaponValueModifier>().ToArray();
            var modifierValue = _itemManager.GetModifierValue(item);
            var modifierString = modifierValue switch
            {
                > 0 => $"<color=green>+{modifierValue}</color>",
                < 0 => $"<color=red>{modifierValue}</color>",
                _ => ""
            };

            return $"{item.Prefab.NameKey.Localize()} {modifierString}";
        }

        private string GetArmorDescription(ItemData data, ArmorItem armor)
        {
            var builder = new StringBuilder();

            builder.AppendLine("UI.StatChange.ArmorFlat".Localize(armor.FlatArmor));
            AppendItemValue(builder, data);

            return builder.ToString();
        }

        private string GetModifierDescription(ItemData data, ModifierEffect modifier)
        {
            var builder = new StringBuilder();

            void AddIfPresent(string skillChangeKey, float? value)
            {
                if (value.HasValue && MathF.Abs(value.Value) >= 0.05f)
                {
                    builder.AppendLine(
                        skillChangeKey.Localize($"{StringUtilities.WrapInColorPercentage(value.Value)}")
                    );
                }
            }

            var m = modifier.ModifierTemplate;
            AddIfPresent("UI.StatChange.MovementSpeed", m.speedModifier);
            AddIfPresent("UI.StatChange.Damage", m.damageModifier);
            AddIfPresent("UI.StatChange.AttackSpeed", m.attackSpeedModifier);
            AddIfPresent("UI.StatChange.AccuracyFlat", m.accuracyFlatModifier);

            return builder.ToString();
        }

        private string GetWeaponDescription(ItemData data, Weapon weapon)
        {
            var builder = new StringBuilder();

            var dmgMod = GetWeaponModifierString(data, WeaponPropertyModifiers.Damage, weapon.BaseDamage);
            var rangeMod = GetWeaponModifierString(data, WeaponPropertyModifiers.Range, weapon.BaseRange);
            var atkSpeedMod =
                GetWeaponModifierString(data, WeaponPropertyModifiers.AttackSpeed, weapon.BaseAttackSpeed);
            var accMod = GetWeaponPercentageModifierString(
                data,
                WeaponPropertyModifiers.Accuracy,
                weapon.BaseAccuracyPercent
            );
            var hitChance = HitChanceCalculator.GetHitChance(
                HitChanceSettings.Enemy,
                data.GetApplied(
                    WeaponPropertyModifiers.Accuracy,
                    weapon.BaseAccuracyPercent
                ) / 100f
            );
            if (GameSettings.Instance.Preferences.ShowHitChances)
            {
                accMod = $"{accMod} ->  {hitChance * 100f:F0}%";
            }

            var modifierValue = _itemManager.GetModifierValue(data);
            var modifierText = modifierValue switch
            {
                > 0 => $"<color=green>+{modifierValue}</color>",
                < 0 => $"<color=red>{modifierValue}</color>",
                _ => "0"
            };

            builder.AppendLine($"Damage: {weapon.BaseDamage} {dmgMod}");
            builder.AppendLine($"Range: {weapon.BaseRange} {rangeMod}");
            builder.AppendLine($"Attack Speed: {weapon.BaseAttackSpeed} {atkSpeedMod}");
            builder.AppendLine($"Accuracy: {weapon.BaseAccuracyPercent:F0}% {accMod}");

            if (weapon.ReloadComponent is not null)
            {
                var reloadSpeedMod = GetWeaponModifierString(
                    data,
                    WeaponPropertyModifiers.ReloadTime,
                    weapon.ReloadComponent.ReloadTime,
                    reverted: true,
                    min: 0f
                );
                var ammoCapacityMod = GetWeaponIntModifierString(
                    data,
                    WeaponPropertyModifiers.AmmoCapacity,
                    weapon.ReloadComponent.BaseMaxAmmo
                );

                builder.AppendLine($"Reload Time: {weapon.ReloadComponent.ReloadTime:F2}s {reloadSpeedMod}");
                builder.AppendLine($"Ammo Capacity: {weapon.ReloadComponent.BaseMaxAmmo} {ammoCapacityMod}");
            }

            var weaponTypeDesc = weapon switch
            {
                ProjectileWeapon p => GetRangedWeaponDescription(data, p),
                TouchWeapon t => GetTouchWeaponDescription(data, t),
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(weaponTypeDesc))
                builder.AppendLine(weaponTypeDesc);

            builder.AppendLine($"Modifier Value: {modifierText}");

            AppendItemValue(builder, data);

            return builder.ToString();
        }

        private string GetTouchWeaponDescription(ItemData data, TouchWeapon weapon)
        {
            return $"Defense: {weapon.PercentArmor * 100:F0}%\n";
        }

        private string GetRangedWeaponDescription(ItemData data, ProjectileWeapon weapon)
        {
            return $"Projectiles: {weapon.ProjectileCount}\n";
        }

        private string GetWeaponModifierString(ItemData data,
            WeaponPropertyModifiers modType,
            float baseValue,
            bool reverted = false,
            float max = float.MaxValue,
            float min = float.MinValue
        )
        {
            var mod = data.GetChange(modType, baseValue);

            mod = Mathf.Clamp(mod + baseValue, min, max) - baseValue;

            return MathF.Abs(mod) > 0.05f ? $"({StringUtilities.WrapInColor(mod, reverted)})" : string.Empty;
        }

        private string GetWeaponIntModifierString(ItemData data,
            WeaponPropertyModifiers modType,
            float baseValue
        )
        {
            var mod = data.GetChange(modType, baseValue);
            var modRounded = Mathf.FloorToInt(mod);
            return MathF.Abs(mod) > 0.05f ? $"({StringUtilities.WrapInColor(modRounded)})" : string.Empty;
        }

        private string GetWeaponPercentageModifierString(ItemData data,
            WeaponPropertyModifiers modType,
            float baseValue
        )
        {
            var mod = data.GetChange(modType, baseValue);
            return MathF.Abs(mod) > 0.05f ? $"({StringUtilities.WrapInColor(mod)}%)" : string.Empty;
        }

        private string GetItemValueLine(ItemData data)
        {
            return "Items.Value".Localize($"{_itemManager.GetValue(data):F2}");
        }

        private void AppendItemValue(StringBuilder builder, ItemData data)
        {
            var value = _itemManager.GetValue(data);
            if (value > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Items.Value".Localize($"{value:F2}"));
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine("Items.NoValue".Localize());
            }
        }
    }
}