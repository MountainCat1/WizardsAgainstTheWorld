using System;
using Items;
using UnityEngine;

public class ContainerObject : InteractionBehavior
{
    public ItemBehaviour ItemBehaviour => itemBehaviour;
    
    [SerializeField] private ItemBehaviour itemBehaviour = null!;
    
    protected override void OnInteractionComplete(Interaction interaction)
    {
        base.OnInteractionComplete(interaction);
        
        if (interaction.Entity is not Creature creature)
        {
            throw new Exception("ItemPickup can only be picked up by Creature");
        }
        
        creature.Inventory.AddItemFromPrefab(itemBehaviour);
    }
}