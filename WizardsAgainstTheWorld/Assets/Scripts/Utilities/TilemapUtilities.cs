using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utilities
{
    public static class TilemapUtilities
    {
        public static List<Vector2Int> GetAllLocalTilePositions(Tilemap map)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            BoundsInt bounds = map.cellBounds;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    positions.Add(new Vector2Int(pos.x, pos.y));
                }
            }

            return positions;
        }

        public static List<Vector2> GetAllTilePositions(Tilemap map)
        {
            List<Vector2> positions = new List<Vector2>();
            BoundsInt bounds = map.cellBounds;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    Vector3 worldPos = map.CellToWorld(pos);
                    positions.Add(new Vector2(worldPos.x, worldPos.y));
                }
            }

            return positions;
        }
    }
}