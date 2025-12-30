using System.Collections.Generic;
using System.Linq;
using Constants;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class CreateEntranceGenerationStep : GenerationStep
    {
        [SerializeField] private GameObject entrancePrefab = null!;
        
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var wallTiles = data.CreateTileList(TileType.Wall);
            var wallClusters = TileCluster.GetConnectedClusters(wallTiles);
            var longestWall = wallClusters.OrderByDescending(x => x.Count).First();

            var validEntrancePoints = longestWall.Where(v => ValidEntrancePoint(v, data));
            var entrance = validEntrancePoints.RandomElement();

            data.SetTile(entrance.x, entrance.y, TileType.Floor);
            
            Instantiate(entrancePrefab, new Vector3(entrance.x, entrance.y, 0), Quaternion.identity);
        }
        private bool ValidEntrancePoint(Vector2Int v, GenerateMapData data)
        {
            var directions = DirectionsInt.Directions;
            var neighbour = new List<(Vector2Int position, int type)>();

            foreach (var direction in directions)
            {
                var x = v.x + direction.x;
                var y = v.y + direction.y;

                if (!data.IsInBounds(x, y))
                {
                    return false;
                }

                neighbour.Add((new Vector2Int(x, y), data.GetTile(x, y)));
            }

            // Check if the entrance is not surrounded by walls
            if (neighbour.Count(x => x.type == (int)TileType.Wall) > 2)
                return false;

            // Check if the entrance is not surrounded by floors
            if (neighbour.Count(x => x.type == (int)TileType.Floor) != 1)
                return false;

            // Check if there is a direct path to the edge of the map in one direction
            foreach (var direction in DirectionsInt.Directions)
            {
                if (IsConnectedToEdgeInDirection(v, direction, data))
                    return true;
            }

            return false;
        }

        private bool IsConnectedToEdgeInDirection(Vector2Int start, Vector2Int direction, GenerateMapData data)
        {
            var current = start;

            while (data.IsInBounds(current.x, current.y))
            {
                current += direction;

                // If we reach the edge of the map, return true
                if (current.x == 0 || current.y == 0 || current.x == data.GridSize.x - 1 || current.y == data.GridSize.y - 1)
                    return true;

                // If we hit a wall, stop checking this direction
                if (data.GetTile(current.x, current.y) == (int)TileType.Wall)
                    return false;
            }

            return false;
        }


    }
}