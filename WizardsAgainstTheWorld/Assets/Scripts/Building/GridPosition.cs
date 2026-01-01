using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Building
{
    public readonly struct GridPosition
    {
        public readonly int X;
        public readonly int Y;

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridPosition FromWorldPosition(Vector2 position)
        {
            return new GridPosition(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }
        
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
        
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(X, 0, Y);
        }
    }

    public sealed class GridSystem
    {
        public readonly int Width;
        public readonly int Height;
        public readonly float CellSize;
        private readonly Vector3 origin;

        private readonly GridCell[,] cells;

        public GridSystem(int width, int height, float cellSize, Vector3 origin)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            this.origin = origin;

            cells = new GridCell[width, height];

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                cells[x, y] = new GridCell(new GridPosition(x, y));
        }

        public Vector3 GridToWorld(GridPosition pos)
            => origin + new Vector3(
                (pos.X) * CellSize,
                (pos.Y) * CellSize
            );

        public Vector2 GetCenterFromCells(IEnumerable<GridPosition> positions)
        {
            var posCount = positions.Count();
            
            if (positions == null || posCount == 0)
                throw new System.ArgumentException("Positions list cannot be null or empty.", nameof(positions));

            float totalX = 0f;
            float totalY = 0f;

            foreach (var pos in positions)
            {
                totalX += pos.X;
                totalY += pos.Y;
            }

            return new Vector2(totalX / posCount, totalY / posCount);
        }
    
        public GridPosition WorldToGrid(Vector3 world)
        {
            var local = world - origin;

            return new GridPosition(
                Mathf.RoundToInt(local.x / CellSize),
                Mathf.RoundToInt(local.y / CellSize)
            );
        }

        public bool InBounds(GridPosition pos)
            => pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;

        public GridCell GetCell(GridPosition pos)
            => cells[pos.X, pos.Y];
        
        public static IReadOnlyList<GridPosition> GetFootprint(
            GridPosition center,
            int width,
            int height)
        {
            var result = new List<GridPosition>(width * height);

            var startX = center.X - width / 2;
            var startY = center.Y - height / 2;

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                result.Add(new GridPosition(startX + x, startY + y));

            return result;
        }
    }
}