using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Zenject;

public class SimpleCreatureLoot : MonoBehaviour
{
    [SerializeField] private Creature creature;
    [SerializeField] private List<ItemBehaviour> itemPrefabs;

    [Inject] private IItemManager _itemManager;

    private void Start()
    {
        if (creature == null)
        {
            GameLogger.LogError("Creature is not assigned in CreatureLoot script in " + gameObject.name);
            return;
        }

        GameLogger.Log($"CreatureLoot started for {creature.name} with following items: {string.Join(", ", itemPrefabs)}");


        foreach (var item in itemPrefabs)
        {
            creature.Inventory.AddItemFromPrefab(item);
        }
    }
}