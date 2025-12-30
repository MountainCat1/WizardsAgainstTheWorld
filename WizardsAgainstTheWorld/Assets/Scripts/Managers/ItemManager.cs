using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface IItemManager
    {
        public ItemBehaviour InstantiateItem(ItemData itemData, Transform parent = null);
        
        public ICollection<ItemBehaviour> GetItems();

        ItemBehaviour GetItemPrefab(string dataIdentifier);
        
        public decimal GetValue(ItemData itemData);
        public int GetModifierValue(ItemData itemData);
        ItemBehaviour InstantiateFromPrefab(ItemBehaviour itemPrefab, Transform creatureTransform = null);
    }

    public class ItemManager : MonoBehaviour, IItemManager
    {
        [Inject] private DiContainer _diContainer;

        private ICollection<ItemBehaviour> _items;
        
        public ICollection<ItemBehaviour> GetItems() => _items; 

        private void Awake()
        {
            _items = Resources.LoadAll<ItemBehaviour>("Items");
            GameLogger.Log($"Loaded {_items.Count} items.\n{string.Join("\n", _items.Select(i => i.GetIdentifier()))}");
        }

        public ItemBehaviour GetItemPrefab(string itemDataIdentifier)
        {
            return _items.FirstOrDefault(i => i.GetIdentifier() == itemDataIdentifier);
        }

        public decimal GetValue(ItemData itemData)
        {
            var baseCost = itemData.Prefab.BaseCost;
            
            var modifierSum = itemData.Modifiers
                .OfType<WeaponValueModifier>()
                .Sum(m => m.Value);

            return (decimal)(baseCost + (baseCost * modifierSum));
        }
        public int GetModifierValue(ItemData itemData)
        {
            var modifiers = itemData.Modifiers.OfType<WeaponValueModifier>().ToArray();
            var modifierValue = modifiers.Count(x => x.Value > 0) - modifiers.Count(x => x.Value < 0);
            return modifierValue;
        }

        public ItemBehaviour InstantiateFromPrefab(ItemBehaviour itemPrefab, Transform creatureTransform = null)
        {
            var item = _diContainer.InstantiatePrefab(
                itemPrefab,
                creatureTransform?.position ?? Vector3.zero,
                Quaternion.identity,
                creatureTransform
            ).GetComponent<ItemBehaviour>();

            item.SetData(ItemData.FromItem(itemPrefab));

            return item;
        }

        public ItemBehaviour InstantiateItem(ItemData itemData, Transform parent = null)
        {
            var itemPrefab = GetItemPrefab(itemData.Identifier);

            if(itemPrefab == null)
            {
                GameLogger.LogError($"Item prefab not found for identifier: {itemData.Identifier}");
                return null;
            }
            
            var item = _diContainer.InstantiatePrefab(
                itemPrefab,
                parent?.position ?? Vector3.zero,
                Quaternion.identity,
                parent
            ).GetComponent<ItemBehaviour>();

            item.SetData(itemData);

            return item;
        }
    }
}