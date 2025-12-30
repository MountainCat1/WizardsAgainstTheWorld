using Managers;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class CreatureSpawner : MonoBehaviour
    {
        [Inject] private ICreatureManager _creatureManager;
        
        [field: SerializeField] public bool Enabled { get; set; }
        [field: SerializeField] public bool Once { get; set; }
        
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private Creature creaturePrefab;
        [SerializeField] private float spawnRange;
        
        void Start()
        {
            if (Enabled)
            {
                // Start spawning creatures at intervals
                InvokeRepeating(nameof(SpawnCreature), spawnInterval, spawnInterval);
            }
        }

        private void SpawnCreature()
        {
            if (!Enabled) return;
            
            if (Once)
            {
                Enabled = false;
                CancelInvoke(nameof(SpawnCreature));
            }

            // Logic to spawn a creature
            // This could be a call to a creature manager or spawner
            Debug.Log("Spawning a creature...");
            
            Vector2 spawnPosition = new Vector2(
                Random.Range(-spawnRange, spawnRange) + transform.position.x,
                Random.Range(-spawnRange, spawnRange) + transform.position.y
            );
            
            // Spawn the creature using the creature manager
            var spawnedCreature = _creatureManager.SpawnCreature(creaturePrefab, spawnPosition);
            
            spawnedCreature.gameObject.SetActive(true);
        }
    }
}