using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class StepDungeonGenerator : MonoBehaviour, IMapGenerator
    {
        [SerializeField] private List<GenerationStep> generationSteps = new();
        [SerializeField] private GenerateMapSettings settings = null!;
        [SerializeField] private TilemapShadowGenerator tilemapShadowGenerator;
        
        public event Action MapGenerated;
        public event Action MapGeneratedLate;

        public MapData MapData { get; private set; }

        public GenerateMapSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public void GenerateMap()
        {
            GenerateMap(null);
        }

        public void GenerateMap(int? seedOverride)
        {
            Debug.Log("Generating map with seed: " + (seedOverride ?? settings.seed));            
            
            var data = new GenerateMapData(settings);

            Random random;
            if (seedOverride != null)
                random = new Random(seedOverride.Value);
            else if (settings.seed == 0)
                random = new Random(); // system time-based seed
            else
                random = new Random(settings.seed);

            foreach (var step in generationSteps)
            {
                step.Generate(data, settings, random);
            }

            MapData = CreateMapData(data);
            MapGenerated?.Invoke();
            
            tilemapShadowGenerator.UpdateShadows();

            StartCoroutine(DelayedInvoke());
        }

        public void SafeGenerateMap(int? seedOverride)
        {
            int baseSeed = seedOverride ?? settings.seed;
            if(baseSeed == 0)
                baseSeed = Environment.TickCount;
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    GenerateMap(baseSeed + i);
                    return;
                }
                catch (Exception e)
                {
                    if (i == 99)
                    {
                        GameLogger.LogError("Map generation failed after 100 attempts.");
                    }

                    ClearMap();
                }
            }
        }

        private void ClearMap()
        {
            foreach (var generationStep in generationSteps)
            {
                generationStep.Clear();
            }
        }
        
        private IEnumerator DelayedInvoke()
        {
            yield return new WaitForEndOfFrame();
            MapGeneratedLate?.Invoke();
        }

        private MapData CreateMapData(GenerateMapData data)
        {
            return new MapData(data.GridSize, data.Grid, settings.tileSize, data.Rooms);
        }
    }
}