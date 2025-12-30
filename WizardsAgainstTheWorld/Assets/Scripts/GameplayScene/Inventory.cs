using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public enum EquipmentSlot
{
    None,
    Weapon,
    Armor
}

public class Inventory
{
    public event Action Changed;
    public event Action<EquipmentSlot, ItemBehaviour> ItemEquipped;
    public event Action<EquipmentSlot, ItemBehaviour> ItemUnequipped;
    public event Action<ItemBehaviour> ItemUsed;

    [Inject] private DiContainer _diContainer;
    [Inject] private IItemManager _itemManager;
    [Inject] private ILootManager _lootManager;

    public IReadOnlyList<ItemBehaviour> Items => _items;
    public IReadOnlyDictionary<EquipmentSlot, ItemBehaviour> EquippedItems => _equippedItems;
    [CanBeNull] public Entity Entity { get; }

    private readonly List<ItemBehaviour> _items = new();
    private readonly Dictionary<EquipmentSlot, ItemBehaviour> _equippedItems = new();

    private Transform _transform;

    public Inventory(Transform rootTransform, [CanBeNull] Entity entity = null)
    {
        _transform = rootTransform;
        Entity = entity;
    }

    #region Public Methods
    
    public void EquipItem(ItemBehaviour item)
    {
        if (item == null)
            throw new NullReferenceException("Tried to equip a null item");

        if (!_items.Contains(item))
            throw new InvalidOperationException("Item is not in the inventory");

        if (item.EquipmentSlot == EquipmentSlot.None)
            throw new InvalidOperationException("Item does not have a valid equipment slot");

        // Un-equip any existing item in the same slot
        if (_equippedItems.TryGetValue(item.EquipmentSlot, out var unequippedItem))
        {
            UnequipItem(item.EquipmentSlot);
        }

        // Equip the new item
        _equippedItems[item.EquipmentSlot] = item;
        item.Equip(new ItemUseContext()
        {
            Creature = Entity as Creature,
        });

        Changed?.Invoke();
        ItemEquipped?.Invoke(item.EquipmentSlot, item);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.None)
            throw new ArgumentException("Cannot unequip item from None slot");

        if (_equippedItems.TryGetValue(slot, out var item))
        {
            item.UnEquip(new ItemUnUseContext()
            {
                Creature = Entity as Creature,
            });
            _equippedItems.Remove(slot);
            Changed?.Invoke();
            ItemUnequipped?.Invoke(slot, item);
        }
        else
        {
            GameLogger.LogError(
                $"Tried to unequip item {item.GetIdentifier()} from slot {slot}, but it is not equipped.");
        }
    }

    public bool TryUnequipItem(ItemBehaviour item)
    {
        if (item == null)
            throw new NullReferenceException("Tried to unequip a null item");

        if (!_items.Contains(item))
            throw new InvalidOperationException("Item is not in the inventory");

        if (item.EquipmentSlot == EquipmentSlot.None)
            return false; // Item is not equipped

        if (!IsEquipped(item))
            return false;

        UnequipItem(item.EquipmentSlot);
        return true;
    }

    public void TransferItem(Inventory from, ItemBehaviour item)
    {
        if (from == null)
            throw new NullReferenceException("Tried to transfer item from null inventory");

        if (item == null)
            throw new NullReferenceException("Tried to transfer null item");

        if (from == this)
            return;

        if (from._items.Contains(item))
        {
            from.DeleteItem(item);
            AddInstantiatedItem(item);
        }
    }

    public void TransferItem(Inventory from, ItemBehaviour item, int count)
    {
        if (from == null)
            throw new NullReferenceException("Tried to transfer item from null inventory");

        if (item == null)
            throw new NullReferenceException("Tried to transfer null item");

        if (from == this || count <= 0)
            return;

        if (!from._items.Contains(item))
            return;

        if (item.Stackable)
        {
            // Clamp transfer count to available count
            int transferCount = Math.Min(item.Count, count);

            // Remove from source
            from.RemoveItems(item.GetIdentifier(), transferCount);

            // Add to destination
            var data = item.GetData();
            data.Count = transferCount;
            AddItem(data);
        }
        else
        {
            // Transfer up to `count` instances of the non-stackable item
            var identifier = item.GetIdentifier();
            for (int i = 0; i < count; i++)
            {
                var itemToTransfer = from.GetItem(identifier);
                if (itemToTransfer == null)
                    break;

                from.DeleteItem(itemToTransfer);
                AddInstantiatedItem(itemToTransfer);
            }
        }
    }

    public void DropItem(ItemUnUseContext ctx)
    {
        if (!_items.Contains(ctx.Item))
        {
            GameLogger.LogError($"Tried to drop item {ctx.Item.GetIdentifier()} that is not in the inventory.");
            return;
        }

        var item = ctx.Item;

        TryUnequipItem(item); // Unequip if it was equipped
        item.UnEquip(ctx);
        
        var dropped = Drop(item.GetIdentifier(), item.Count);

        if (item.Count <= 0)
        {
            Object.Destroy(item.gameObject);
        }

        _lootManager.SpawnPickup(dropped, ctx.Creature.transform.position);
    }

    public ItemBehaviour Drop(string identifier, int count)
    {
        if (string.IsNullOrEmpty(identifier) || count <= 0)
            return null;

        var item = GetItem(identifier);
        if (item == null)
            return null;

        if (!item.Stackable)
        {
            DeleteItem(item);
            item.transform.parent = null; // Detach
            return item;
        }

        if (count >= item.Count)
        {
            DeleteItem(item);
            item.transform.parent = null; // Detach
            return item;
        }
        else
        {
            item.Count -= count;

            var dropped = CloneItem(item, count);

            Changed?.Invoke();

            return dropped;
        }
    }

    private ItemBehaviour CloneItem(ItemBehaviour item)
    {
        return CloneItem(item, item.Count);
    }

    private ItemBehaviour CloneItem(ItemBehaviour item, int count)
    {
        var dropped = InstantiateItemPrefab(item.Original);
        var itemData = ItemData.FromItem(dropped);
        itemData.Count = count;
        dropped.SetData(itemData);
        dropped.transform.parent = null; // Detach dropped instance from inventory transform
        dropped.Inventory = null; // Clear inventory reference
        return dropped;
    }


    public ItemBehaviour AddItemFromPrefab(ItemBehaviour itemPrefab)
    {
        if (itemPrefab == null)
            throw new NullReferenceException("Tried to add item to inventory that is null");

        // Instantiate the item first
        var itemInstance = InstantiateItemPrefab(itemPrefab);

        return AddItemToInventory(itemInstance);
    }

    public void AddInstantiatedItem(ItemBehaviour item)
    {
        if (item == null)
            throw new NullReferenceException("Tried to add a null item to inventory");

        if (item.Inventory != null)
            throw new Exception("Item already belongs to an inventory");

        item.transform.SetParent(_transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        AddItemToInventory(item);
    }

    /// <summary>
    /// Adds an item to the inventory based on the provided item data.
    /// For stackable items, increases the count of existing items.
    /// For non-stackable items, creates a new instance.
    /// </summary>
    /// <param name="itemData">The data of the item to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when itemData or itemData.Prefab is null.</exception>
    public void AddItem(ItemData itemData)
    {
        if (itemData == null)
            throw new ArgumentNullException(nameof(itemData));

        if (itemData.Prefab == null)
            throw new ArgumentNullException(nameof(itemData.Prefab));

        if (itemData.Prefab.Stackable == false)
        {
            var itemBehaviour = AddItemFromPrefab(itemData.Prefab);
            itemBehaviour.SetData(itemData);
            return;
        }

        var item = GetItem(itemData.Identifier);
        if (item is null)
        {
            var itemBehaviour = AddItemFromPrefab(itemData.Prefab);
            itemBehaviour.SetData(itemData);
        }
        else
        {
            item.Count += itemData.Count;
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Remove an item from the inventory, THIS DOESNT DESTROY THE GAMEOBJECT
    /// </summary>
    /// <param name="item"></param>
    public void DeleteItem(ItemBehaviour item)
    {
        _items.Remove(item);

        UnregisterItem(item);

        item.Inventory = null;

        Changed?.Invoke();
    }

    public void RemoveItems(string identifier, int count)
    {
        var item = GetItem(identifier);
        if (item == null)
            return;

        if (item.Stackable)
        {
            item.Count -= count;
            if (item.Count <= 0)
            {
                DeleteItem(item);
            }
            else
            {
                Changed?.Invoke(); // Notify that the item count has changed
                // it is necessary coz we don't call DeleteItem()
                // which would otherwise notify it Changed event
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                var itemToRemove = GetItem(identifier);
                if (itemToRemove == null)
                    return;

                DeleteItem(itemToRemove);
            }
        }
    }

    public void SetData(InventoryData dataInventory)
    {
        foreach (var itemData in dataInventory.Items)
        {
            var itemPrefab = _itemManager.GetItemPrefab(itemData.Identifier);
            var item = AddItemFromPrefab(itemPrefab);
            item.SetData(itemData);
        }
    }

    public bool HasItem(string getIdentifier)
    {
        return GetItem(getIdentifier) is not null;
    }

    public ItemBehaviour GetItem(string identifier)
    {
        return _items.Find(x => x.GetIdentifier().Equals(identifier));
    }

    public int GetItemCount(string identifier)
    {
        var item = GetItem(identifier);
        if (item == null)
            return 0;

        if (item.Stackable)
            return item.Count;

        return 1;
    }

    public IEnumerable<ItemBehaviour> GetItemsOrdererByEquipped()
    {
        return Items
            .OrderBy(x => x.Stackable)
            .ThenByDescending(IsEquipped);
    }

    #endregion

    #region Helper Methods

    private ItemBehaviour AddItemToInventory(ItemBehaviour item)
    {
        if (item.Inventory != null)
            throw new Exception("Item already belongs to an inventory");

        if (item.Stackable)
        {
            var existing = GetItem(item.GetIdentifier());
            if (existing is not null)
            {
                existing.Count += item.Count;
                Object.Destroy(item.gameObject); // Destroy redundant instance
                Changed?.Invoke();
                return existing;
            }
        }

        // Non-stackable or first-time stackable item
        item.Inventory = this;
        _items.Add(item);
        RegisterItem(item);

        Changed?.Invoke();
        return item;
    }

    private ItemBehaviour InstantiateItemPrefab(ItemBehaviour itemPrefab)
    {
        var item = _diContainer.InstantiatePrefab(
            itemPrefab,
            _transform.position,
            Quaternion.identity,
            _transform
        ).GetComponent<ItemBehaviour>();

        item.Original = itemPrefab.Original ?? itemPrefab;
        item.SetData(ItemData.FromItem(itemPrefab));

        return item;
    }

    private void RegisterItem(ItemBehaviour item)
    {
        item.Used += HandleItemUsed;
        item.Activated += HandleItemActivated;
        item.Deactivated += HandleItemDeactivated;
    }

    private void UnregisterItem(ItemBehaviour item)
    {
        item.Used -= HandleItemUsed;
        item.Activated += HandleItemActivated;
        item.Deactivated -= HandleItemDeactivated;
    }
    
    private void HandleItemDeactivated()
    {
        Changed?.Invoke();
    }    
    
    private void HandleItemActivated()
    {
        Changed?.Invoke();
    }

    private void HandleItemUsed(ItemBehaviour item)
    {
        ItemUsed?.Invoke(item);
    }

    #endregion

    public bool IsEquipped(ItemBehaviour item)
    {
        if (item == null)
            throw new NullReferenceException("Tried to check if a null item is equipped");

        return _equippedItems.ContainsValue(item);
    }
}