using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Zenject;

public class CreatureLoot : MonoBehaviour
{
    [SerializeField] private Creature creature;
    [SerializeField] private LootTable lootTable;
    
    [Inject] private IItemManager _itemManager;
    
    private void Start()
    {
        if(creature == null)
        {
            GameLogger.LogError("Creature is not assigned in CreatureLoot script in " + gameObject.name);
            return;
        }
        
        GameLogger.Log($"CreatureLoot started for {creature.name} with loot table {lootTable.name}");
        
        var item = lootTable.GetRandomItem();

        if (item?.item == null)
        {
            GameLogger.LogWarning($"No item found in loot table {lootTable.name} for creature {creature.name}");
            return;
        }
        
        creature.Inventory.AddItemFromPrefab(item.item);
    }
}