using GameplayScene.Managers;
using Markers;
using UI;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SpriteRenderer))]
public class TouchResourcePickup : MonoBehaviour
{
    [SerializeField] private ColliderEventProducer colliderEventProducer;
    [SerializeField] private GameResourceData itemResources;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Teams teamAbleToPickup;
    
    [Inject] private IFloatingTextManager _floatingTextManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private IResourceManager _resourceManager;

    private Inventory _inventory;

    protected void Awake()
    {
        colliderEventProducer.TriggerEnter += OnTriggerEnterHandler;
    }

    private void Start()
    {
        spriteRenderer.sprite = _resourceManager.GetIcon(itemResources.type);
    }

    private void OnTriggerEnterHandler(Collider2D obj)
    {
        var creatureCollider = obj.GetComponent<CreatureCollider>();
        if(creatureCollider is null)
            return;
        
        var creature = creatureCollider.Creature;
        if(creature.Team != teamAbleToPickup)
            return;
        
        Destroy(gameObject);
        
        _resourceManager.AddResource(itemResources.ToGameResource());
        
        _floatingTextManager.SpawnFloatingText(
            transform.position,
            "Game.FloatingText.PickupItem",
            FloatingTextType.Pickup,
            itemResources.type,
            itemResources.amount
        );
    }
}