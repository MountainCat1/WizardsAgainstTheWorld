using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;
using Random = UnityEngine.Random;

namespace LevelSelector.Managers
{
    public interface IUpgradeManager
    {
        UpgradeResult PlayerBuyUpgrade(ItemData item);
        UpgradeResult UpgradeItem(ItemData itemData);
        void ScrapItem(ItemData itemData);
        int GetScrapValue(ItemData itemData);
        int GetUpgradeCost(ItemData selectedItem);
        int GetReforgeCost(ItemData selectedItem);
        bool CanUpgrade(ItemData selectedItem);
        bool CanScrap(ItemData selectedItem);
        UpgradeResult BuyReforgeItem(ItemData item);
        bool CanReforge(ItemData selectedItem);
    }

    public enum UpgradeResult
    {
        Good,
        Bad,
        Normal
    }

    public class UpgradeManager : MonoBehaviour, IUpgradeManager
    {
        #region Serialized Fields

        [SerializeField] private float goodUpgradeChance = 1f;
        [SerializeField] private float badUpgradeChance = 1f;
        [SerializeField] private float normalUpgradeChance = 2f;

        [SerializeField] private float normalReforgeChance = 1f;
        [SerializeField] private float goodReforgeChance = 2f;
        [SerializeField] private float badReforgeChance = 1f;

        [SerializeField] private float modifierMaxValue = 0.5f;
        [SerializeField] private float modifierMinValue = 0.1f;

        [SerializeField] private ItemBehaviour scrapItemPrefab;

        #endregion

        #region Dependencies

        [Inject] private ICrewManager _crewManager;
        [Inject] private IItemManager _itemManager;

        #endregion

        #region Public API

        public UpgradeResult PlayerBuyUpgrade(ItemData item)
        {
            if (!CanUpgrade(item))
                throw new Exception("Cannot upgrade item");

            var cost = GetUpgradeCost(item);
            _crewManager.Resources.AddMoney(-cost);

            return UpgradeItem(item);
        }

        public UpgradeResult BuyReforgeItem(ItemData item)
        {
            if (!CanReforge(item))
                throw new Exception("Cannot reforge item");

            var cost = GetReforgeCost(item);
            _crewManager.Resources.AddMoney(-cost);

            return ReforgeItem(item);
        }

        public void ScrapItem(ItemData itemData)
        {
            if (!CanScrap(itemData))
            {
                GameLogger.LogError($"Only weapons can be scrapped, was trying to scrap {itemData.Identifier}");
                return;
            }

            int value = GetScrapValue(itemData);
            _crewManager.Resources.AddMoney(value);
            _crewManager.Inventory.RemoveItem(itemData);
        }

        public UpgradeResult UpgradeItem(ItemData item)
        {
            float roll = Random.Range(0f, goodUpgradeChance + badUpgradeChance + normalUpgradeChance);

            if (roll <= goodUpgradeChance)
            {
                ApplyGoodUpgrade(item);
                return UpgradeResult.Good;
            }

            if (roll <= goodUpgradeChance + badUpgradeChance)
            {
                ApplyBadUpgrade(item);
                return UpgradeResult.Bad;
            }

            ApplyNormalUpgrade(item);
            return UpgradeResult.Normal;
        }

        public bool CanUpgrade(ItemData item) =>
            item.Type == ItemType.Weapon && _crewManager.Resources.Money >= GetUpgradeCost(item);

        public bool CanScrap(ItemData item) => item.Type == ItemType.Weapon;

        public bool CanReforge(ItemData item) =>
            item.Type == ItemType.Weapon && _crewManager.Resources.Money >= GetReforgeCost(item);

        public int GetScrapValue(ItemData item) => Mathf.CeilToInt((float)_itemManager.GetValue(item) / 3f) + 1;

        public int GetUpgradeCost(ItemData item)
        {
            float baseCost = item.Prefab.BaseCost * 0.2f;
            float modifierFactor = item.Modifiers
                .OfType<WeaponValueModifier>()
                .Sum(x => x.Value);
            float modifierCount = item.Modifiers.Count;

            return Mathf.CeilToInt((baseCost * Mathf.Pow(1.4f, modifierFactor) + modifierCount * 2f));
        }

        public int GetReforgeCost(ItemData item)
        {
            float baseCost = item.Prefab.BaseCost * 0.3f;
            float modifierFactor = item.Modifiers
                .OfType<WeaponValueModifier>()
                .Sum(x => x.Value);

            return Mathf.CeilToInt(baseCost * Mathf.Pow(1.4f, modifierFactor));
        }

        #endregion

        #region Upgrade Logic

        private void ApplyGoodUpgrade(ItemData item)
        {
            item.Modifiers.Add(GetPositiveModifier(item));
            item.Modifiers.Add(GetPositiveModifier(item));
        }

        private void ApplyBadUpgrade(ItemData item)
        {
            item.Modifiers.Add(GetNegativeModifier(item));
        }

        private void ApplyNormalUpgrade(ItemData item)
        {
            item.Modifiers.Add(GetPositiveModifier(item));
            item.Modifiers.Add(GetPositiveModifier(item));
            item.Modifiers.Add(GetNegativeModifier(item));
        }

        #endregion

        #region Reforge Logic

        private UpgradeResult ReforgeItem(ItemData item)
        {
            var positives = item.Modifiers
                .OfType<WeaponValueModifier>()
                .Where(x => x.Value > 0)
                .ToList();

            var negatives = item.Modifiers
                .OfType<WeaponValueModifier>()
                .Where(x => x.Value < 0)
                .ToList();

            item.Modifiers.Clear();

            float roll = Random.Range(0f, goodReforgeChance + badReforgeChance + normalReforgeChance);

            if (roll <= goodReforgeChance)
                return ApplyReforge(item, positives.Count + 1, Math.Max(0, negatives.Count - 1), UpgradeResult.Good);

            if (roll <= goodReforgeChance + badReforgeChance)
                return ApplyReforge(item, Math.Max(0, positives.Count - 1), negatives.Count, UpgradeResult.Bad);

            return ApplyReforge(item, positives.Count, negatives.Count, UpgradeResult.Normal);
        }

        private UpgradeResult ApplyReforge(ItemData item, int posCount, int negCount, UpgradeResult result)
        {
            for (int i = 0; i < posCount; i++)
                item.Modifiers.Add(GetPositiveModifier(item));
            for (int i = 0; i < negCount; i++)
                item.Modifiers.Add(GetNegativeModifier(item));

            return result;
        }

        #endregion

        #region Modifier Logic

        private WeaponValueModifier GetPositiveModifier(ItemData item)
        {
            var validProps = GetValidModifierTypes(item);
            return new WeaponValueModifier
            {
                Type = validProps.RandomElement(),
                Value = Random.Range(modifierMinValue, modifierMaxValue)
            };
        }

        private WeaponValueModifier GetNegativeModifier(ItemData item)
        {
            var mod = GetPositiveModifier(item);
            mod.Value *= -1f / 1.5f; // Reduce impact of negative mods
            return mod;
        }

        private List<WeaponPropertyModifiers> GetValidModifierTypes(ItemData item)
        {
            var validProps = new List<WeaponPropertyModifiers>();

            // Always allow these properties
            validProps.Add(WeaponPropertyModifiers.Damage);
            validProps.Add(WeaponPropertyModifiers.AttackSpeed);
            validProps.Add(WeaponPropertyModifiers.Accuracy);

            if (item.Prefab is not TouchWeapon)
                validProps.Add(WeaponPropertyModifiers.Range);

            if (item.Reloadable)
            {
                validProps.Add(WeaponPropertyModifiers.ReloadTime);
                validProps.Add(WeaponPropertyModifiers.AmmoCapacity);
            }

            return validProps;
        }

        #endregion
    }
}