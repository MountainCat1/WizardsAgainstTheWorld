using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;
using Zenject;
using Random = System.Random;

namespace Services.MapGenerators
{
    public partial class DungeonGenerator : MonoBehaviour, IMapGenerator
    {
        [Inject] private IRoomDecorator _roomDecorator = null!;

        public event Action MapGenerated;
        public event Action MapGeneratedLate;
        public MapData MapData { get; private set; }
        public GenerateMapSettings Settings {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        [SerializeField] private int seed = 696969;

        [SerializeField] private Vector2Int gridSize = new(50, 50);

        [SerializeField] private int roomCount = 10;
        [SerializeField] private Vector2Int roomMinSize = new(5, 5);
        [SerializeField] private Vector2Int roomMaxSize = new(10, 10);

        [SerializeField] private Tilemap wallTileMap = null!;
        [SerializeField] private Tilemap floorTileMap = null!;

        [SerializeField] private TileBase floorTile = null!;
        [SerializeField] private TileBase wallTile = null!;

        [SerializeField] private bool addWallAroundMap = false;
        [SerializeField] private bool wallsOnlyAroundFloor = false;
        
        [SerializeField] private GameObject entrancePrefab = null!;
        
        private List<Vector2Int> _wallTiles = new();
        private List<Vector2Int> _floorTiles = new();

        private Random _random = new();

        public void Start()
        {
            _random = seed == 0 ? new Random() : new Random(seed);
        }

        public void GenerateMap()
        {
            GameLogger.Log("===== Generating map =====");
            _wallTiles.Clear();
            _floorTiles.Clear();
            var grid = new int[gridSize.x, gridSize.y];
            var rooms = GenerateRooms(grid);
            var cellSize = wallTileMap.cellSize.x;
            var cellSizeSquared = cellSize * cellSize;

            ConnectRooms(grid, rooms);

            DrawDungeon(grid);

            GameLogger.Log("===== Map generated =====");

            GameLogger.Log("Decorating rooms...");
            
            _roomDecorator.DecorateRooms(rooms, tileSize: cellSize);

            MapData = new MapData(gridSize, grid, cellSize, rooms);

            var startingRoom = rooms.First(x => x.IsEntrance);

            var randomWallOfStartingRoom = _wallTiles
                .Where(wallTile => 
                    // Ensure the wall tile is within the required distance
                    startingRoom.Positions.Any(roomTile => (roomTile - wallTile).sqrMagnitude <= cellSizeSquared) 
                    // Ensure the wall tile is adjacent to at least one empty tile
                    && GetAdjacentTiles(wallTile).Any(IsTileEmpty)
                )
                .RandomElement();

            GameLogger.Log($"Creating entrance at {randomWallOfStartingRoom}");
            Instantiate(entrancePrefab, new Vector3(randomWallOfStartingRoom.x, randomWallOfStartingRoom.y, 0), Quaternion.identity);
            
            // Remove the wall tile that the entrance is on
            wallTileMap.SetTile(new Vector3Int(randomWallOfStartingRoom.x, randomWallOfStartingRoom.y, 0), null);
            _wallTiles.Remove(randomWallOfStartingRoom);
            
            MapGenerated?.Invoke();
            StartCoroutine(DelayedInvoke());
        }

        public void SafeGenerateMap()
        {
            throw new NotImplementedException("SafeGenerateMap is not implemented in DungeonGenerator");
        }

        private IEnumerator DelayedInvoke()
        {
            yield return null;
            MapGeneratedLate?.Invoke();
        }

        private List<RoomData> GenerateRooms(int[,] grid)
        {
            var rooms = new List<RoomData>();

            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = _random.Next(roomMinSize.x, roomMaxSize.x + 1);
                int roomHeight = _random.Next(roomMinSize.y, roomMaxSize.y + 1);

                int x = _random.Next(1, gridSize.x - roomWidth - 1);
                int y = _random.Next(1, gridSize.y - roomHeight - 1);

                var roomData = new RoomData { RoomID = i };

                for (int roomx = x; roomx < x + roomWidth; roomx++)
                {
                    for (int roomy = y; roomy < y + roomHeight; roomy++)
                    {
                        grid[roomx, roomy] = (int)TileType.Floor;
                        roomData.Positions.Add(new Vector2Int(roomx, roomy));
                    }
                }

                rooms.Add(roomData);
                GameLogger.Log($"Room {i} created at: {x}, {y}");
            }

            return rooms;
        }

        private void ConnectRooms(int[,] grid, List<RoomData> rooms)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                var startRoom = rooms[i];
                var endRoom = rooms[i + 1];

                var start = startRoom.Positions[_random.Next(startRoom.Positions.Count)];
                var end = endRoom.Positions[_random.Next(endRoom.Positions.Count)];

                CreateCorridor(grid, start, end);

                startRoom.ConnectedRoomIDs.Add(endRoom.RoomID);
                endRoom.ConnectedRoomIDs.Add(startRoom.RoomID);

                GameLogger.Log($"Corridor created between Room {startRoom.RoomID} and Room {endRoom.RoomID}");
            }
        }

        private void CreateCorridor(int[,] grid, Vector2Int start, Vector2Int end)
        {
            var current = start;

            while (current != end)
            {
                if (current.x != end.x)
                {
                    current.x += Math.Sign(end.x - current.x);
                }
                else if (current.y != end.y)
                {
                    current.y += Math.Sign(end.y - current.y);
                }

                grid[current.x, current.y] = (int)TileType.Floor;
            }

            GameLogger.Log($"Corridor created from: {start} to {end}");
        }

        private void DrawDungeon(int[,] grid)
        {
            // 1) Draw floor tiles
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (grid[x, y] == (int)TileType.Floor)
                    {
                        floorTileMap.SetTile(new Vector3Int(x, y, 0), floorTile);
                        _floorTiles.Add(new Vector2Int(x, y));
                    }
                }
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
                        if (grid[x, y] == (int)TileType.Floor)
                        {
                            // Check neighbors
                            foreach (var dir in directions)
                            {
                                int nx = x + dir.x;
                                int ny = y + dir.y;

                                // If neighbor out-of-bounds or not a floor, queue it as a wall
                                if (!IsInBounds(nx, ny) || grid[nx, ny] != (int)TileType.Floor)
                                {
                                    // If within bounds, add it to the wall list
                                    if (IsInBounds(nx, ny))
                                    {
                                        var wallPos = new Vector2Int(nx, ny);
                                        _wallTiles.Add(wallPos);
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
                    {
                        if (grid[x, y] == (int)TileType.Floor)
                            continue;
                        
                        grid[x, y] = (int)TileType.Wall;
                        _wallTiles.Add(new Vector2Int(x, y));
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
                    if (grid[x, 0] != (int)TileType.Floor)
                        _wallTiles.Add(new Vector2Int(x, 0));

                    if (grid[x, gridSize.y - 1] != (int)TileType.Floor)
                        _wallTiles.Add(new Vector2Int(x, gridSize.y - 1));
                }
            }

            // Left and right columns
            if (addWallAroundMap)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (grid[0, y] != (int)TileType.Floor)
                        _wallTiles.Add(new Vector2Int(0, y));

                    if (grid[gridSize.x - 1, y] != (int)TileType.Floor)
                        _wallTiles.Add(new Vector2Int(gridSize.x - 1, y));
                }
            }

            // 4) Use Godot’s terrain connection or standard SetCell to place walls
            wallTileMap.SetTiles(_wallTiles.Select(v => new Vector3Int(v.x, v.y, 0)).ToArray(),
                _wallTiles.Select(x => wallTile).ToArray());

            foreach (var wallTile in _wallTiles)
            {
                grid[wallTile.x, wallTile.y] = (int)TileType.Wall;
            }

            GameLogger.Log("Dungeon drawn on TileMap");
        }

        /// <summary>
        /// Checks if the given (x, y) is within the grid bounds.
        /// </summary>
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
        }
        
        // Gets adjacent tiles (up, down, left, right)
        private IEnumerable<Vector2Int> GetAdjacentTiles(Vector2Int tile)
        {
            return new List<Vector2Int>
            {
                tile + Vector2Int.up,
                tile + Vector2Int.down,
                tile + Vector2Int.left,
                tile + Vector2Int.right
            };
        }

        // Checks if a tile is empty
        private bool IsTileEmpty(Vector2Int tile)
        {
            // Replace it with your logic to determine if a tile is empty
            return !_wallTiles.Contains(tile) && !_floorTiles.Contains(tile);
        }
    }
}