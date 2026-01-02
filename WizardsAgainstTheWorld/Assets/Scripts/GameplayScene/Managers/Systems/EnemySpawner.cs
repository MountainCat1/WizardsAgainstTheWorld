using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using Data;
using Managers.Helpers;
using Services.MapGenerators;
using UnityEngine;
using Utilities;
using Zenject;
using Random = UnityEngine.Random;

namespace Managers
{
    public interface IEnemySpawner
    {
        void Initialize(MapData mapData);
        public float GatheredMana { get; }
        public float ManaGain { get; }
    }

    public class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [SerializeField] private float minDistance = 16f;
        [SerializeField] private int minEnemiesPerSpawn = 1;
        [SerializeField] private int maxEnemiesPerSpawn = 2;

        [SerializeField] private float minDistanceFromSafePoints = 10f;

        // [SerializeField] private float minDistanceFromBossRoom = 9f;
        // [SerializeField] private float minDistanceFromInitialEnemies = 7f;
        [SerializeField] private float maxDistanceFromPlayerCreatures = 24f;
        [SerializeField] private float oneTimeEnemyMaxSpread = 6f;
        
        [SerializeField] private bool debugDisplayOccupiedPositions = false;

        [Inject] private ICreatureManager _creatureManager;
        [Inject] private IDataResolver _dataResolver;

        private MapData _mapData;

        [field: ReadOnlyInInspector]
        [field: SerializeField]
        public float GatheredMana { get; private set; }
        
        [field: ReadOnlyInInspector]
        [field: SerializeField]
        public float ManaGain { get; private set; }
        
        private SpawnPositionSelector _spawnPositionSelector;

        [SerializeField] private List<Creature> creatures;
        
        private void Update()
        {
            if (debugDisplayOccupiedPositions)
            {
                foreach (var pos in _spawnPositionSelector.GetAvailablePositions())
                {
                    Debug.DrawLine(pos + new Vector2(-0.25f, -0.25f), pos + new Vector2(0.25f, 0.25f), Color.red);
                    Debug.DrawLine(pos + new Vector2(-0.25f, 0.25f), pos + new Vector2(0.25f, -0.25f), Color.red);
                }
            }
        }

        public void Initialize(MapData mapData)
        {
            _mapData = mapData;
            
            var positionSelector = new SpawnPositionSelector(mapData);

            var creaturesToSpawn = creatures;
            
            StartCoroutine(
                EnemySpawningCoroutine(creaturesToSpawn.Select(CreatureData.FromCreature).ToArray(),
                    positionSelector: positionSelector
                )
            );
            
            _spawnPositionSelector = positionSelector;
        }

        private IEnumerator EnemySpawningCoroutine(ICollection<CreatureData> enemies, SpawnPositionSelector positionSelector)
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            float startTime = Time.time;
            GatheredMana = 0f;

            var positions = _mapData.GetAllTilePositionsOfNotType(TileType.Wall)
                .ToList();

            var playerCreatures = _creatureManager.GetCreaturesAliveActive()
                .Where(c => c.Team == Teams.Player)
                .ToList();

            if (playerCreatures.Count == 0)
            {
                GameLogger.Log("No player creatures found, stopping enemy spawning");
                yield break;
            }

            while (true)
            {
                // Update mana gain based on time and difficulty
                float timeElapsed = Time.time - startTime;
                
                // TODO: implement this with some intent
                float baseEnemySpawnManaPerSecond = 5f; // Example base value
                float enemySpawnManaGrowthRate = 0.5f; // Example growth rate
                float manaPerSecond = baseEnemySpawnManaPerSecond +
                                       enemySpawnManaGrowthRate * timeElapsed;

                ManaGain = manaPerSecond;
                GatheredMana += manaPerSecond * fixedDeltaTime;

                bool spawnedAny = false;

                // Try to spawn as many enemies as possible using current mana
                while (true)
                {
                    var enemy = enemies.RandomElement();
                    int spawnCount = Random.Range(minEnemiesPerSpawn, maxEnemiesPerSpawn);
                    float totalManaCost = enemy.ManaCost * spawnCount;

                    if (totalManaCost > GatheredMana)
                        break;

                    GatheredMana -= totalManaCost;
                    
                    // Cleanup destroyed creatures
                    playerCreatures = _creatureManager.GetCreaturesAliveActive()
                        .Where(c => c.Team == Teams.Player)
                        .ToList();

                    var spawnPos = positionSelector.GetRandomValidPosition(
                        positions, playerCreatures, minDistance, maxDistanceFromPlayerCreatures);

                    if (spawnPos == null)
                    {
                        GameLogger.LogWarning("No suitable spawn position found");
                        break;
                    }

                    SpawnEnemiesAtPosition(enemy, spawnPos.Value, spawnCount, memorizeTarget: true);
                    spawnedAny = true;
                }

                if (!spawnedAny)
                    yield return new WaitForFixedUpdate(); // wait until next physics tick
            }
        }


        private void SpawnEnemiesAtPosition(CreatureData enemy, Vector2Int position, int count,
            bool memorizeTarget = true)
        {
            GameLogger.Log($"Spawning {count} {enemy.Name} at {position}");

            for (int i = 0; i < count; i++)
            {
                var spawnOffset = new Vector2(_mapData.TileSize / 2, _mapData.TileSize / 2);
                var spawned = SpawnEnemy(enemy, (Vector2)position + spawnOffset);

                if (spawned?.Controller is CreatureAiController ai && memorizeTarget)
                {
                    var hostiles = _creatureManager.GetCreaturesAliveActive()
                        .Where(x => x.Team == Teams.Player)
                        .ToArray();

                    if (hostiles.Length > 0)
                        ai.Memorize(hostiles.RandomElement());
                    else
                        GameLogger.LogWarning("No player creatures found to memorize");
                }
                else if (spawned == null)
                {
                    GameLogger.LogError("Enemy spawned without AI controller");
                    break;
                }
            }
        }

        private Creature SpawnEnemy(CreatureData enemy, Vector2 position)
        {
            var creature = _creatureManager.SpawnCreature(_dataResolver.ResolveCreaturePrefab(enemy), position);

            if (creature == null)
            {
                GameLogger.LogError($"Failed to spawn enemy {enemy.Name} at {position}");
                return null;
            }

            creature.Move(new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)));
            return creature;
        }
    }
}