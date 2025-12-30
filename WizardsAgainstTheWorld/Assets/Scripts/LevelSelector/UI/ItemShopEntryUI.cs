using Data;
using Managers;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class ItemShopEntryUI : ItemEntryUI
    {
        [Inject] private IItemManager _itemManager;
        [Inject] private IDataResolver _dataResolver;
        
        [SerializeField] private TextMeshProUGUI priceDisplay;

        public void SetShopData(ShopData shopData)
        {
            var price = shopData.inventory.ContainsItem(ItemData) // we check if the item is in the shop inventory
                ? shopData.GetBuyPrice(_itemManager.GetValue(ItemData)) // if it is, we get the sell price
                : shopData.GetSellPrice(_itemManager.GetValue(ItemData)); // if it is not, we get the buy price
            
            priceDisplay.text = $"{price}$";
        }
    }
}