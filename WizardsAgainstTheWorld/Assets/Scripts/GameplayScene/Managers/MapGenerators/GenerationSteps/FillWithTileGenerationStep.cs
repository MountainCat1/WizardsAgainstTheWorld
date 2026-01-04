using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class FillWithTileGenerationStep : GenerationStep
    {
        [SerializeField] private TileType tileType = TileType.Floor;
        
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            foreach (var tilePosition in data.GetAllTilesEnumerable())
            {
                data.SetTile(tilePosition, tileType);
            }
        }
    }
}