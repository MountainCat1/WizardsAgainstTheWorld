using System;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "LootTable", menuName = "Custom/LootTable", order = 1)]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private LootTableEntry[] entries;

        public LootTableEntry GetRandomItem()
        {
            float totalWeight = 0;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            float randomWeight = UnityEngine.Random.Range(0, totalWeight);
            foreach (var entry in entries)
            {
                randomWeight -= entry.weight;
                if (randomWeight <= 0)
                {
                    return entry;
                }
            }

            throw new Exception("No item found");
        }
    }
    
    [Serializable]
    public class LootTableEntry
    {
        [SerializeField] public ItemBehaviour item;
        [SerializeField] public float weight;
        [SerializeField] public int maxCount;
        [SerializeField] public int minCount;

        public LootTableEntry()
        {
            minCount = 1;
            maxCount = 1;
            item = null;
            weight = 1;
        }
    }
}