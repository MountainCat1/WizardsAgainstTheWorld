using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators
{
    public class MapData
    {
        public float TileSize { get; }

        private readonly Dictionary<Vector2Int, TileType> _mapData = new();
        private readonly Dictionary<TileType, ICollection<Vector2>> _tilePositions = new();
        private readonly Dictionary<int, RoomData> _rooms = new(); // Stores room data

        public MapData(Vector2Int gridSize, int[,] grid, float tileSize, List<RoomData> rooms)
        {
            // Populate tile data
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var tileType = (TileType)grid[x, y];
                    
                    _mapData[new Vector2Int(x, y)] = tileType;
                    
                    if (!_tilePositions.ContainsKey(tileType))
                    {
                        _tilePositions[tileType] = new List<Vector2>();
                    }
                    _tilePositions[tileType].Add(new Vector2(x, y) * tileSize);
                }
            }

            // Populate room data
            foreach (var room in rooms)
            {
                _rooms[room.RoomID] = room;
            }

            TileSize = tileSize;
        }
        
        public TileType GetTileType(Vector2Int position)
        {
            return _mapData[position];
        }
        
        public Vector2 GetRandomPositionTileOfType(TileType tileType)
        {
            var tiles = new List<Vector2>();

            foreach (var (position, type) in _mapData)
            {
                if (type == tileType)
                {
                    tiles.Add(position);
                }
            }

            return tiles[new Random().Next(0, tiles.Count)] * TileSize;
        }
        
        public List<Vector2> GetSpreadGlobalPositions(Vector2 startPosition, int count, TileType tileType)
        {
            // startPosition += _tileSize / 2;
            
            var tiles = _tilePositions[tileType].ToList();
            
            tiles.Sort((a, b) => Vector2.Distance(a, b).CompareTo(Vector2.Distance(a, b)));

            count = Math.Min(count, tiles.Count);
            return tiles.GetRange(0, count).Select(x => x).ToList();
        }

        [CanBeNull]
        public RoomData GetRoomData(int roomID)
        {
            return _rooms.ContainsKey(roomID) ? _rooms[roomID] : null;
        }

        public List<RoomData> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }

        public List<Vector2Int> GetAllTilePositionsOfType(TileType searchedType)
        {
            var positions = new List<Vector2Int>();

            foreach (var (position, type) in _mapData)
            {
                if (type == searchedType)
                {
                    positions.Add(position);
                }
            }

            return positions;
        }
        
        public List<Vector2Int> GetAllTilePositionsOfNotType(TileType searchedType)
        {
            var positions = new List<Vector2Int>();

            foreach (var (position, type) in _mapData)
            {
                if (type != searchedType)
                {
                    positions.Add(position);
                }
            }

            return positions;
        }
    }

    public class RoomData
    {
        public int RoomID { get; set; }
        public List<Vector2Int> Positions { get; set; } = new List<Vector2Int>();
        public List<Vector2Int> OccupiedPositions { get; set; } = new List<Vector2Int>();
        public List<int> ConnectedRoomIDs { get; set; } = new List<int>();
        public bool IsEntrance { get; set; }
        public bool IsBoss { get; set; }
        public bool IsFarAwayFromSpawn { get; set; }
        public Creature[] Enemies { get; set; }
        public bool Occupied { get; set; } = false;
        
        public IEnumerable<Vector2Int> FreePositions => Positions.Where(pos => !OccupiedPositions.Contains(pos));
    }
}
