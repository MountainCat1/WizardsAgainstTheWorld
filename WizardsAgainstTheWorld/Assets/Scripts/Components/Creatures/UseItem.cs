using System;
using Items;
using UnityEngine;

namespace DefaultNamespace
{
    public class UseItem : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        [SerializeField] private ItemBehaviour item;

        private void Start()
        {
            if (creature == null)
            {
                Debug.LogError("Creature is not assigned in UseItem script.");
                return;
            }

            if (item == null)
            {
                Debug.LogError("Item is not assigned in UseItem script.");
                return;
            }

            item.Use(new ItemUseContext
            {
                Creature = creature,
                Position = transform.position,
            });

            if (item.EquipmentSlot != EquipmentSlot.None)
                creature.Inventory.EquipItem(item);
        }
    }
}