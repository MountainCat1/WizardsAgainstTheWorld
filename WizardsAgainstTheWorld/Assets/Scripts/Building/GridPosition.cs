using System.Collections.Generic;
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
            return new GridPosition(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
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
                (pos.X + 0.5f) * CellSize,
                (pos.Y + 0.5f) * CellSize
            );

        public GridPosition WorldToGrid(Vector3 world)
        {
            var local = world - origin;

            return new GridPosition(
                Mathf.FloorToInt(local.x / CellSize),
                Mathf.FloorToInt(local.z / CellSize)
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