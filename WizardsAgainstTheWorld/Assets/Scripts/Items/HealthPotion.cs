using Components;
using UnityEngine;

namespace Items
{
    public class HealthPotion : ItemBehaviour
    {
        [SerializeField] private int healAmount;
        
        public override void Use(ItemUseContext ctx)
        {
            base.Use(ctx);

            ctx.Creature.Heal(new HealContext()
            {
                Healer = ctx.Creature,
                Target = ctx.Creature,
                HealAmount = healAmount
            });

            ctx.Creature.Inventory.DeleteItem(this);
        }
    }
}