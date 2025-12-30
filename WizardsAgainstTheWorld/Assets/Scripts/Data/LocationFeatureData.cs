using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LevelSelector.Managers;
using ScriptableObjects;
using Services.MapGenerators;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class LocationFeatureData
{
    public string Name = string.Empty;
    public string DescriptionKey => $"UI.LocationFeatures.{Name}";

    public List<FeatureEnemyData> Enemies = new();
    public List<FeatureEnemyData> OneTimeEnemies = new();

    public string[] RoomBlueprints = Array.Empty<string>();
    public string[] GenericRoomBlueprints = Array.Empty<string>();

    public string MapOverride = string.Empty;
    public MapTileSetOverrideType overrideMapTileSetOverride;
    
    [CanBeNull] public string Marker = null;
    public bool BlockExit;

    public string[] VictoryConditionsIds = Array.Empty<string>();
    [FormerlySerializedAs("fullOverride")] public bool fullLocationOverride;
    [CanBeNull] public string MusicOverride = null;


    [System.Serializable]
    public class FeatureEnemyData
    {
        public CreatureData CreatureData;
        public int SpawnCount = 1;

        public static FeatureEnemyData FromFeatureEnemy(FeatureEnemies data)
        {
            return new FeatureEnemyData
            {
                CreatureData = CreatureData.FromCreature(data.Creature),
                SpawnCount = 1,
            };
        }

        public static FeatureEnemyData FromOneTimeFeatureEnemy(FeatureOneTimeEnemies data)
        {
            return new FeatureEnemyData
            {
                CreatureData = CreatureData.FromCreature(data.Creature),
                SpawnCount = data.SpawnCount,
            };
        }
    }
}