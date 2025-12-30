using Components;
using UnityEngine;

namespace Items
{
    public class ArmorItem : ItemBehaviour, IArmorSource
    {
        // Serialized fields
        [field: SerializeField] public float ArmorValue { get; set; } = 1f;

        
        // Implement IArmorSource
        public float FlatArmor => ArmorValue;
        public float PercentArmor => 0f;
        public override EquipmentSlot EquipmentSlot => EquipmentSlot.Armor;


        // Override ItemBehaviour properties
        public override bool Stackable => false;
        public override void Use(ItemUseContext ctx)
        {
            base.Use(ctx);

            ctx.Creature.Inventory.EquipItem(this);
        }

        public override void Equip(ItemUseContext ctx)
        {
            base.Equip(ctx);
            
            if (ctx.Creature.Armor != this)
            {
                ctx.Creature.StartUsingArmor(this);
            }
        }

        public override void UnEquip(ItemUnUseContext ctx)
        {
            base.UnEquip(ctx);
            
            if(ctx.Creature.Armor == this)
                ctx.Creature.StartUsingArmor(null);
        }
    }
}