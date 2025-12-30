using System;
using Services.MapGenerators;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace LevelSelector.Managers
{
    public interface ILocationGenerator
    {
        LocationData GenerateLocation(System.Random random, float regionDifficulty);
        void AddFeatures(LocationData location, RegionType regionType);
    }

    public class LocationGenerator : MonoBehaviour, ILocationGenerator
    {
        public LocationData GenerateLocation(System.Random random, float regionDifficulty)
        {
            var gameSettings = GameSettings.Instance;
            var difficultySettings = gameSettings.DifficultySettings;

            float difficultyMultiplier = Mathf.Lerp(
                1f,
                gameSettings.Difficulty,
                gameSettings.DifficultySettings.SpawnRateDifficultyModifierFactor
            );

            var mapSettings = GenerateMapSettingsData.FromSettings(GenerateMapSettings.GenerateRandom(random));

            float baseManaPerSecond = Random.Range(
                difficultySettings.MinBaseManaPerSecond + regionDifficulty,
                difficultySettings.MaxBaseManaMultiplier * regionDifficulty
            ) * difficultyMultiplier;

            float initialMana = Random.Range(
                difficultySettings.MinInitialMana,
                difficultySettings.MaxInitialMana
            ) * regionDifficulty * difficultyMultiplier;

            float manaGrowthRate = Random.Range(
                difficultySettings.MinManaGrowthRate,
                difficultySettings.MaxManaGrowthRate
            ) * difficultyMultiplier;

            return new LocationData
            {
                Id = Guid.NewGuid(),
                MapSettings = mapSettings,
                Name = Constants.Names.SpaceStations.RandomElement(),
                BaseEnemySpawnManaPerSecond = baseManaPerSecond,
                InitialEnemySpawnMana = initialMana,
                EnemySpawnManaGrowthRate = manaGrowthRate
            };
        }


        public void AddFeatures(LocationData location, RegionType regionType)
        {
            var originFeature = regionType.originLocationFeatures.GetRandomItem().ToData();
            location.Features.Add(originFeature);

            var mainFeature = regionType.weightedLocationFeatures.GetRandomItem().ToData();
            location.Features.Add(mainFeature);

            var secondaryFeatures = regionType.weightedSecondaryLocationFeatures
                .GetRandomItems(Random.Range(regionType.minSecondaryFeatures, regionType.maxSecondaryFeatures));

            foreach (var secondaryFeature in secondaryFeatures)
            {
                location.Features.Add(secondaryFeature.ToData());
            }
        }
    }
}