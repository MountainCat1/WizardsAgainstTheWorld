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
        
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var availablePositions = data.GetTilesOfType(TileType.Floor)
                .OrderBy(_ => random.Next());
            
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
                Debug.Log("xd");
                placedPickups++;
            }
        }
    }
}