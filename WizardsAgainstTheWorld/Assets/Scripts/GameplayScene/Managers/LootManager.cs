using System.Linq;
using Items;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ILootManager
    {
        ItemPickup SpawnPickup(ItemBehaviour itemBehaviour, Vector3 position);
    }
    public class LootManager : MonoBehaviour, ILootManager
    {
        [Inject] private ICreatureEventProducer _entityEventProducer;
        [Inject] private ISpawnerManager _spawnerManager;
        
        [SerializeField] private ItemPickup itemPickupPrefab;
        [SerializeField] private float forceOffset = 50f;

        private void Start()
        {
            _entityEventProducer.CreatureDied += OnEntityDied;
        }
        
        private void OnEntityDied(Creature creature, DeathContext deathContext)
        {
            var items = creature.Inventory.Items;

            foreach (var item in items.ToArray())
            {
                creature.Inventory.DeleteItem(item);
                SpawnPickup(item, creature.transform.position);
            }
        }
        
        public ItemPickup SpawnPickup(ItemBehaviour itemBehaviour, Vector3 position)
        {
            if (itemBehaviour == null)
            {
                GameLogger.LogError("Cant spawn pickup with null item behaviour");
                return null;
            }
            
            var itemPickup = _spawnerManager.Spawn(itemPickupPrefab, position, transform);

            itemPickup.SetItem(itemBehaviour);

            var force = new Vector2(
                UnityEngine.Random.Range(-forceOffset, forceOffset),
                UnityEngine.Random.Range(-forceOffset, forceOffset)
            );
            itemPickup.Rigidbody2D.AddForce(force);

            return itemPickup;
        }

    }
}