using Items;
using JetBrains.Annotations;
using Managers;
using ScriptableObjects;
using UI;
using UnityEngine;
using Zenject;
using static Utilities.LocalizationHelper;
#if UNITY_EDITOR
using UnityEditor; // Required for marking dirty in the editor
#endif

public class RandomContainerObject : InteractionBehavior
{
    [Inject] private IFloatingTextManager _floatingTextManager;
    [Inject] private IItemManager _itemManager;

    [SerializeField] private LootTable lootTable = null!;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite closeSprite;
    [SerializeField] private Sprite openSprite;

    private LootTableEntry _loot;

    private const string EmptyKey = "Game.Interaction.RandomContainer.Empty";
    private const string LootKey = "Game.Interaction.RandomContainer.Loot";
    private const string LootSingleKey = "Game.Interaction.RandomContainer.LootSingle";

    protected override void Awake()
    {
        base.Awake();

        if (lootTable == null)
        {
            GameLogger.LogError($"Loot table is not in {name}");
            return;
        }

        _loot = lootTable.GetRandomItem();
    }


    protected override void OnInteractionComplete(Interaction interaction)
    {
        base.OnInteractionComplete(interaction);

        if (interaction.Entity is not Creature creature)
        {
            throw new System.InvalidOperationException("RandomContainerObject can only be used by creatures");
        }

        if (_loot.item is null)
        {
            _floatingTextManager.SpawnFloatingText(transform.position, EmptyKey, FloatingTextType.Miss);
            spriteRenderer.sprite = openSprite;
            return;
        }

        var itemData = ItemData.FromItem(_loot.item);
        itemData.Count = Random.Range(_loot.minCount, _loot.maxCount);
        creature.Inventory.AddItem(itemData);

        if (itemData.Count == 1)
        {
            _floatingTextManager.SpawnFloatingText(
                position: transform.position,
                localizationKey: LootSingleKey,
                type: FloatingTextType.InteractionCompleted,
                args: L(itemData.Prefab.NameKey)
            );
        }
        else
        {
            _floatingTextManager.SpawnFloatingText(
                position: transform.position,
                localizationKey: LootKey,
                type: FloatingTextType.InteractionCompleted,
                args: new object[] { L(itemData.Prefab.NameKey), itemData.Count }
            );
        }

        spriteRenderer.sprite = openSprite;
    }

    [CanBeNull]
    public ItemBehaviour GetRandomItem()
    {
        if (_loot?.item is null)
        {
            return null;
        }
        
        var itemData = ItemData.FromItem(_loot.item);
        itemData.Count = Random.Range(_loot.minCount, _loot.maxCount);

        var instantiatedItem = _itemManager.InstantiateItem(itemData);

        return instantiatedItem;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = closeSprite;
            UnityEditor.EditorUtility.SetDirty(spriteRenderer);
        }
    }
#endif
}