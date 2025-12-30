using System;
using System.Collections.Generic;
using System.Linq;
using Services.MapGenerators;
using UnityEngine;
using UnityEngine.Serialization;
using VictoryConditions;

namespace LevelSelector.Managers
{
    [Serializable]
    public class FeatureEnemies
    {
        public Creature Creature;
    }

    [Serializable]
    public class FeatureOneTimeEnemies
    {
        public Creature Creature;
        public int SpawnCount;
    }

    [CreateAssetMenu(fileName = "LocationFeature", menuName = "Custom/LocationFeature", order = 0)]
    public class LocationFeature : ScriptableObject
    {
        [field: SerializeField] public string Name { get; set; }
        public string description = string.Empty;
        public List<FeatureEnemies> enemies = new();
        public List<FeatureOneTimeEnemies> oneTimeEnemies = new();
        public List<RoomBlueprint> roomBlueprints = new();
        public List<RoomBlueprint> genericRoomBlueprints = new();
        public GameObject mapOverride;
        public string marker = string.Empty;
        public bool blockExit = false;

        public VictoryCondition[] victoryConditions = Array.Empty<VictoryCondition>();
        public MapTileSetOverrideType overrideMapTileSet;
        [FormerlySerializedAs("fullOverride")] public bool fullLocationOverride = false;
        public AudioClip musicOverride;

        public LocationFeatureData ToData()
        {
            return new LocationFeatureData
            {
                Enemies = enemies.Select(LocationFeatureData.FeatureEnemyData.FromFeatureEnemy).ToList(),
                OneTimeEnemies = oneTimeEnemies.Select(LocationFeatureData.FeatureEnemyData.FromOneTimeFeatureEnemy)
                    .ToList(),
                Name = Name,
                RoomBlueprints = roomBlueprints.Select(x => x.name).ToArray(),
                GenericRoomBlueprints = genericRoomBlueprints.Select(x => x.name).ToArray(),
                MapOverride = mapOverride != null ? mapOverride.name : string.Empty,
                Marker = string.IsNullOrEmpty(marker) ? null : marker,
                BlockExit = blockExit,
                VictoryConditionsIds = victoryConditions
                    .Select(vc => vc.GetIdentifier())
                    .ToArray(),
                overrideMapTileSetOverride = overrideMapTileSet,
                fullLocationOverride = fullLocationOverride,
                MusicOverride = musicOverride != null ? musicOverride.name : null
            };
        }
    }
}