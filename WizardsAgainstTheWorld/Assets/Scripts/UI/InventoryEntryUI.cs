using System.Linq;
using Items;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class InventoryEntryUI : MonoBehaviour
    {
        [Inject] private IItemDescriptionManager _itemDescriptionManager;
        
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private Image itemImage;
        
        [SerializeField] private Color equippedColor = Color.green;
        
        private ItemBehaviour _item;
        private Creature _creature;

        private Color _defaultColor;
        
        private void Awake()
        {
            _defaultColor = itemNameText.color;
        }

        public void SetItem(ItemBehaviour item, Creature creature)
        {
            itemNameText.text = _itemDescriptionManager.GetInfoName(item.GetData());
            itemImage.sprite = item.Icon;
            
            _item = item;
            _creature = creature;
            
            itemCountText.text = item.Stackable ? item.Count.ToString() : "";

            var equipped = creature.Inventory.EquippedItems.Values.Contains(item);

            var activated = (item as PassiveItemBehaviour)?.Active ?? false;
            
            itemNameText.color = equipped || activated ? equippedColor : _defaultColor;
        }

        public void DropItem()
        {
            _creature.Inventory.DropItem(new ItemUnUseContext()
            {
                Item = _item,
                Creature = _creature
            });
        }
        
        public void UseItem()
        {
            // Use item
            // _creature.Inventory.EquipItem(_item);
            
            _item.Use(new ItemUseContext()
            {
                Creature = _creature,
                Position = _creature.transform.position,
            });
        }
    }
}