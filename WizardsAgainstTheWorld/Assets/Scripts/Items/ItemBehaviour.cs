using System;
using System.Collections;
using System.Collections.Generic;
using Items.ItemInteractions;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using Zenject;

namespace Items
{
    public class ItemBehaviour : MonoBehaviour
    {
        [Inject] private ILootManager _lootManager;

        public event Action<ItemBehaviour> Used;
        public event Action<ItemBehaviour> UnUsed;
        public event Action Activated;
        public event Action Deactivated;

        public string NameKey => $"Items.{Name}.Name";
        public string DescriptionKey => $"Items.{Name}.Description";

        [field: SerializeField] public Sprite Icon { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public float Weight { get; set; } = 1f;
        [field: SerializeField] public string Description { get; set; }
        [field: SerializeField] public float BaseCost { get; set; }

        protected ItemData ItemData;
        [CanBeNull] public Creature Creature => GetCreature();

        public virtual bool Stackable => true;
        public virtual EquipmentSlot EquipmentSlot => EquipmentSlot.None;
        public bool IsOriginal => Original is null;
        public virtual bool AutoSell => false;

        public ItemBehaviour Original { get; set; }

        public int Count
        {
            get => ItemData.Count;
            set => ItemData.Count = value;
        }

        public Inventory Inventory { get; set; } = null;

        public virtual IEnumerable<ItemInteraction> GetInteractions(DiContainer ctx) =>
            new ItemInteraction[] { };

        protected virtual void Awake()
        {
            var dummyItemData = new ItemData
            {
                Identifier = "dummy item data DO NOT EVER FUCKING USE",
                Count = 1,
                Stackable = Stackable
            };

            SetData(dummyItemData); // uses it :3c
        }

        public string GetIdentifier()
        {
            return $"{Name}";
        }

        public virtual void Use(ItemUseContext ctx)
        {
            var creature = ctx.Creature;
            if (creature == null)
            {
                GameLogger.LogError("Creature is null");
                return;
            }

            GameLogger.Log($"{creature} using item " + Name);

            Used?.Invoke(this);
        }

        public virtual void Equip(ItemUseContext ctx)
        {
        }

        public virtual void UnEquip(ItemUnUseContext ctx)
        {
            var creature = ctx.Creature;
            if (creature == null)
            {
                GameLogger.LogError("Creature is null");
                return;
            }

            GameLogger.Log($"{creature} stopped using item " + Name);

            UnUsed?.Invoke(this);
        }

        public virtual void Pickup(ItemUseContext ctx)
        {
            var creature = ctx.Creature;
            if (creature == null)
            {
                GameLogger.LogError("Creature is null");
                return;
            }

            GameLogger.Log($"{creature} picked up item " + Name);
        }

        public virtual void SetData(ItemData itemData)
        {
            ItemData = itemData;
        }

        public virtual ItemData GetData()
        {
            return ItemData;
        }

        [CanBeNull]
        private Creature GetCreature()
        {
            if (Inventory == null)
            {
                GameLogger.LogError("Inventory is null");
                return null;
            }

            return Inventory.Entity as Creature;
        }

        protected void InvokeActivated()
        {
            Activated?.Invoke();
        }

        protected void InvokeDeactivated()
        {
            Deactivated?.Invoke();
        }
    }
}