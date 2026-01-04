using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class ResourcePickupGenerationStep : GenerationStep
    {
        [SerializeField] private int resourcePickupCount = 10;
        [SerializeField] private TouchResourcePickup resourcePickupPrefab;
        
        [Inject] private DiContainer _diContainer;
        
        private List<TouchResourcePickup> _spawnedPickups = new List<TouchResourcePickup>();
         
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var availablePositionsX = data.GetTilesOfType(TileType.Floor)
                .OrderBy(_ => random.Next());

            var availablePositions = availablePositionsX.Select(x => x.Item1).ToList();
            
            int placedPickups = 0;
            foreach (var position in availablePositions)
            {
                if (placedPickups >= resourcePickupCount)
                    break;

                var worldPosition = (Vector2)position * settings.tileSize;
                var pickup = _diContainer.InstantiatePrefabForComponent<TouchResourcePickup>(
                    resourcePickupPrefab,
                    worldPosition,
                    Quaternion.identity,
                    null
                );
                placedPickups++;
                _spawnedPickups.Add(pickup);
            }
        }

        public override void Clear()
        {
            base.Clear();
            
            foreach (var pickup in _spawnedPickups)
            {
                if (pickup != null)
                {
                    GameObject.Destroy(pickup.gameObject);
                }
            }
            _spawnedPickups.Clear();
        }
    }
}