using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class CreateWallsGenerationStep : GenerationStep
    {
        [SerializeField] private bool wallsOnlyAroundFloor;
        [SerializeField] private bool addWallAroundMap;

        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var gridSize = data.GridSize;
            
            bool IsInBounds(int x, int y)
            {
                return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
            }
            
            // 4-way (or 8-way) directions. This example is 4-way.
            var directions = new Vector2Int[]
            {
                new(0, -1), // Up
                new(0, 1), // Down
                new(-1, 0), // Left
                new(1, 0), // Right
                new(-1, -1), // Top-left
                new(1, -1), // Top-right
                new(-1, 1), // Bottom-left
                new(1, 1) // Bottom-right
            };

            // For diagonal checks, add e.g. new Vector2Int(-1, -1), etc.

            if (wallsOnlyAroundFloor)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        if (data.GetTile(x, y) == (int)TileType.Floor)
                        {
                            // Check neighbors
                            foreach (var dir in directions)
                            {
                                int nx = x + dir.x;
                                int ny = y + dir.y;

                                // If neighbor out-of-bounds or not a floor, queue it as a wall
                                if (!IsInBounds(nx, ny) || data.GetTile(nx, ny) != (int)TileType.Floor)
                                {
                                    // If within bounds, add it to the wall list
                                    if (IsInBounds(nx, ny))
                                    {
                                        data.SetTile(nx, ny, TileType.Wall);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    { if (data.GetTile(x, y) == (int)TileType.Floor)
                            continue;
                        
                        data.SetTile(x, y, TileType.Wall);
                    }
                }
            }

            // 3) Also add walls around the edges of the entire map
            // Top and bottom rows
            if (addWallAroundMap)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    // If not floor, make it a wall
                    if (data.GetTile(x, 0) != (int)TileType.Floor)
                        data.SetTile(x, 0, TileType.Wall);

                    if (data.GetTile(x, gridSize.y - 1) != (int)TileType.Floor)
                        data.SetTile(x, gridSize.y - 1, TileType.Wall);
                }
            }

            // Left and right columns
            if (addWallAroundMap)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (data.GetTile(0, y) != (int)TileType.Floor)
                        data.SetTile(0, y, TileType.Wall);

                    if (data.GetTile(gridSize.x - 1, y) != (int)TileType.Floor)
                        data.SetTile(gridSize.x - 1, y, TileType.Wall);
                }
            }
            
            // foreach (var wallTile in _wallTiles)
            // {
            //     grid[wallTile.x, wallTile.y] = (int)TileType.Wall;
            // }

            GameLogger.Log("Dungeon drawn on TileMap");
        }
    }
}