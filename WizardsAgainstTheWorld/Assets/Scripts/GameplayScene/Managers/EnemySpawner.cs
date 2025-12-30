using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using Data;
using Interactables;
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
        void Initialize(MapData mapData, LocationData location);
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

        private LocationData _locationData;
        private MapData _mapData;

        [field: ReadOnlyInInspector]
        [field: SerializeField]
        public float GatheredMana { get; private set; }
        
        [field: ReadOnlyInInspector]
        [field: SerializeField]
        public float ManaGain { get; private set; }
        
        private SpawnPositionSelector _spawnPositionSelector;

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

        public void Initialize(MapData mapData, LocationData location)
        {
            var positionSelector = new SpawnPositionSelector(mapData);

            _locationData = location;
            this._mapData = mapData;

            SpawnRoomBasedEnemies(positionSelector);

            SpawnOneTimeEnemies(positionSelector);

            SpawnInitialEnemies(positionSelector, _locationData.InitialEnemySpawnMana);

            StartCoroutine(
                EnemySpawningCoroutine(
                    _locationData.Features
                        .SelectMany(x => x.Enemies)
                        .Select(x => x.CreatureData)
                        .ToList(),
                    positionSelector: positionSelector
                )
            );
            
            _spawnPositionSelector = positionSelector;
        }

        private void SpawnOneTimeEnemies(SpawnPositionSelector spawnPositionSelector)
        {
            var validRooms = _mapData.GetAllRooms()
                .Where(x => x.Occupied == false && x.IsEntrance == false);

            var validRoomsPositions = validRooms
                .SelectMany(x => x.Positions)
                .Where(pos => _mapData.GetTileType(pos) == TileType.Floor)
                .ToList();

            foreach (var enemy in _locationData.Features.SelectMany(x => x.OneTimeEnemies))
            {
                var centerPosition = spawnPositionSelector.TakeAnyOff(validRoomsPositions);
                if (centerPosition == null)
                {
                    GameLogger.LogWarning("No valid position found for one-time enemy spawn");
                    continue;
                }
                
                var positionsInProximity = validRoomsPositions
                    .Where(pos => Vector2.Distance(pos, centerPosition.Value) < oneTimeEnemyMaxSpread).ToList();

                for (int i = 0; i < enemy.SpawnCount; i++)
                {
                    var position = spawnPositionSelector.TakeAnyOff(positionsInProximity);
                    
                    if (position == null)
                    {
                        GameLogger.LogWarning("No valid position found for one-time enemy spawn");
                        break;
                    }
                    
                    SpawnEnemiesAtPosition(
                        enemy: enemy.CreatureData,
                        position: position.Value,
                        count: 1,
                        memorizeTarget: false
                    );
                }
            }
        }

        private void SpawnRoomBasedEnemies(SpawnPositionSelector spawnPositionSelector)
        {
            foreach (var room in _mapData.GetAllRooms().Where(r => r.Enemies?.Any() == true))
            {
                if (room.Occupied)
                    continue;

                room.Occupied = true;
                foreach (var enemy in room.Enemies)
                {
                    var position = spawnPositionSelector.TakeRandomRoomPosition(room);
                    if (position != null)
                    {
                        var offset = new Vector2(_mapData.TileSize / 2, _mapData.TileSize / 2);
                        _creatureManager.SpawnCreature(enemy, position.Value + offset);
                    }
                }
            }
        }


        public void SpawnInitialEnemies(SpawnPositionSelector spawnPositionSelector, float initialMana)
        {
            var exitPosition = FindObjectOfType<ExitObject>().transform.position;

            var safePoints = new List<Vector2>();
            safePoints.Add(exitPosition);

            if (_mapData.GetAllRooms().Any(x => x.IsBoss))
            {
                var bossRoomCenter = _mapData.GetAllRooms()
                    .Single(x => x.IsBoss)
                    .Positions.GetAverageCenter();

                safePoints.Add(bossRoomCenter);
            }

            var initialEnemiesPositions = _creatureManager
                .GetCreaturesAliveActive()
                .Select(c => (Vector2)c.transform.position);

            safePoints.AddRange(initialEnemiesPositions);

            float remainingMana = initialMana;

            while (true)
            {
                var enemy = GetRandomInitialEnemyFromLocation();
                float manaCost = enemy.ManaCost;
                if (manaCost > remainingMana)
                    break;

                remainingMana -= manaCost;
                var position = spawnPositionSelector.TakeExcludeNearPoints(safePoints, minDistanceFromSafePoints);

                if (position == null)
                {
                    GameLogger.LogWarning("No suitable position found for initial enemy spawn");
                    break;
                }

                SpawnEnemiesAtPosition(enemy, position.Value, 1, memorizeTarget: false);
            }
        }

        private IEnumerator EnemySpawningCoroutine(ICollection<CreatureData> enemies, SpawnPositionSelector positionSelector)
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            float startTime = Time.time;
            GatheredMana = 0f;

            var positions = _mapData.GetAllRooms()
                .SelectMany(room => room.FreePositions)
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
                float manaPerSecond = _locationData.BaseEnemySpawnManaPerSecond +
                                      _locationData.EnemySpawnManaGrowthRate * timeElapsed;

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


        private CreatureData GetRandomInitialEnemyFromLocation()
        {
            return _locationData.Features
                .SelectMany(x => x.Enemies)
                .Select(x => x.CreatureData)
                .RandomElement();
        }

        private void SpawnEnemiesAtPosition(CreatureData enemy, Vector2Int position, int count,
            bool memorizeTarget = true)
        {
            GameLogger.Log($"Spawning {count} {enemy.Name} at {position}");

            for (int i = 0; i < count; i++)
            {
                var spawnOffset = new Vector2(_mapData.TileSize / 2, _mapData.TileSize / 2);
                var spawned = SpawnEnemy(enemy, (Vector2)position + spawnOffset);

                if (spawned?.Controller is AiController ai && memorizeTarget)
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