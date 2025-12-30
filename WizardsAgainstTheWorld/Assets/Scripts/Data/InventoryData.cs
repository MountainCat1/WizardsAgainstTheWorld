using System;
using System.Collections.Generic;
using System.Linq;
using Data;

// ReSharper disable InconsistentNaming

[Serializable]
public class InventoryData
{
    public List<ItemData> Items = new(); // TODO: this should noy be public, but we need to serialize it?
    public event Action Changed;

    public bool ContainsItem(ItemData itemData)
    {
        return Items.Contains(itemData);
    }

    public static InventoryData FromInventory(Inventory inventory)
    {
        var data = new InventoryData();
        foreach (var item in inventory.GetItemsOrdererByEquipped().Reverse())
        {
            data.Items.Add(ItemData.FromInstantiatedItem(item));
        }

        return data;
    }

    public void AddItem(ItemData itemData, bool silent = false)
    {
        if (!itemData.Stackable)
        {
            Items.Add(itemData);

            if (!silent)
                Changed?.Invoke();

            return;
        }

        var item = Items.FirstOrDefault(x => x.Identifier == itemData.Identifier);
        if (item == null)
        {
            Items.Add(itemData);
        }
        else
        {
            item.Count += itemData.Count;
        }

        if (!silent)
            Changed?.Invoke();
    }

    public int TransferItem(ItemData itemData, InventoryData targetInventory, int amount = 1)
    {
        if (!ContainsItem(itemData))
        {
            GameLogger.LogError("Tried to transfer item not found in inventory");
            return 0;
        }

        if (!itemData.Stackable)
        {
            RemoveItem(itemData);
            targetInventory.AddItem(itemData);
            
            Changed?.Invoke();
            
            return 1;
        }

        int transferAmount = Math.Min(itemData.Count, amount);
        RemoveItem(itemData, transferAmount, silent: true);

        var newItemData = DataCloner.Clone(itemData);
        newItemData.Prefab = itemData.Prefab;
        newItemData.Count = transferAmount;
        targetInventory.AddItem(newItemData);

        Changed?.Invoke();

        return transferAmount;
    }

    public void RemoveItem(ItemData itemData, bool silent = false)
    {
        if (!itemData.Stackable)
        {
            Items.Remove(itemData);
        }
        else
        {
            var item = Items.FirstOrDefault(x => x.Identifier == itemData.Identifier);
            if (item == null)
            {
                GameLogger.LogError("Tried to remove item not found in inventory");
                return;
            }

            if (item.Count <= itemData.Count)
            {
                Items.Remove(item);
            }
            else
            {
                item.Count -= itemData.Count;
            }
        }

        if (!silent)
            Changed?.Invoke();
    }

    public void RemoveItem(ItemData itemData, int amount, bool silent = false)
    {
        if (!itemData.Stackable)
        {
            // Ignore amount, remove the exact item
            Items.Remove(itemData);
        }
        else
        {
            var item = Items.FirstOrDefault(x => x.Identifier == itemData.Identifier);
            if (item == null)
            {
                GameLogger.LogError("Tried to remove item not found in inventory");
                return;
            }

            if (amount >= item.Count)
            {
                Items.Remove(item);
            }
            else
            {
                item.Count -= amount;
            }
        }

        if (!silent)
            Changed?.Invoke();
    }

    public ItemData GetItem(string identifier)
    {
        return Items.FirstOrDefault(x => x.Identifier == identifier);
    }
}