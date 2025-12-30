using System;
using Items;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Entity))]
public class ItemPickup : InteractionBehavior
{
    [SerializeField] private ItemBehaviour itemBehaviour;
    [Inject] private DiContainer diContainer;

    public Rigidbody2D Rigidbody2D => _rigidbody2D;

    private Rigidbody2D _rigidbody2D;
    private Inventory _inventory;
    private Entity _entity; // using entity like that cannot be good....

    protected override void Awake()
    {
        base.Awake();

        _rigidbody2D = GetComponent<Rigidbody2D>();
        
        _entity = GetComponent<Entity>();
        
        _inventory = new Inventory(transform, _entity);
        
        diContainer.Inject(_inventory);
    }

    protected override void OnInteractionComplete(Interaction interaction)
    {
        base.OnInteractionComplete(interaction);

        interaction.Creature.Inventory.TransferItem(_inventory, itemBehaviour);

        Destroy(gameObject);
    }

    public void SetItem(ItemBehaviour item)
    {
        if (item == null || item is null)
            throw new Exception("ItemBehaviour is not set in ItemPickup");

        if (item.Original == null)
        {
            itemBehaviour = _inventory.AddItemFromPrefab(item);
        }
        else
        {
            itemBehaviour = item;
            _inventory.AddInstantiatedItem(item);
        }

        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemBehaviour.Icon;
    }
}