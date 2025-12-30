using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace LevelSelector.Managers
{
    public interface IShopGenerator
    {
        public ShopData GenerateShop();
        public IReadOnlyCollection<Trader> Traders { get; }
    }

    public class ShopGenerator : MonoBehaviour, IShopGenerator
    {
        [Inject] private IItemManager _itemManager;
        [Inject] private IUpgradeManager _upgradeManager;

        public IReadOnlyCollection<Trader> Traders => traders;
        
        [SerializeField] private List<Trader> traders;

        private const float ChanceForUpgrades = 0.5f;
        
        private const float MinPriceMultiplier = 0.7f;
        private const float MaxPriceMultiplier = 1.3f;
        
        public ShopData GenerateShop()
        {
            var shopData = new ShopData();
            
            var trader = traders.RandomElement();
            
            var itemCount = Random.Range(trader.minItems, trader.maxItems);
            
            shopData.itemCount = itemCount;
            
            var items = new List<ItemData>();
            for (int i = 0; i < itemCount; i++)
            {
                var lootEntry = trader.traderItems.GetRandomItem();
                
                var itemData = ItemData.FromItem(lootEntry.item);
                
                var count = Random.Range(lootEntry.minCount, lootEntry.maxCount);
                itemData.Count = count;
                
                items.Add(itemData);
            }

            shopData.inventory = new InventoryData();
            items.ForEach(x => shopData.inventory.AddItem(x));
            
            shopData.priceMultiplier = Random.Range(MinPriceMultiplier, MaxPriceMultiplier);
            shopData.traderIdentifier = trader.GetIdentifier();

            foreach (var itemData in shopData.inventory.Items)
            {
                if(itemData.Type != ItemType.Weapon)
                    continue;
                if (Random.value > ChanceForUpgrades)
                    continue;
                
                _upgradeManager.UpgradeItem(itemData);
            }

            return shopData;
        }
    }
}