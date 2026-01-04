using System;
using System.Collections.Generic;
using Building.Data;
using UnityEngine;

namespace Building
{
    public static class GridUtilities
    {
        public static GridPosition GetAnchorFromWorldPosition(
            GridSystem grid,
            Vector2 worldPosition,
            BuildingFootprint footprint)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (footprint.Width <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Width));
            if (footprint.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Height));

            float cellSize = grid.CellSize;

            Vector2 bottomLeftWorld = worldPosition
                                      - new Vector2(
                                          (footprint.Width  - 1) * cellSize * 0.5f,
                                          (footprint.Height - 1) * cellSize * 0.5f                                      );

            return grid.WorldToGrid(bottomLeftWorld);
        }
        
        public static IReadOnlyList<GridPosition> GetCellsFromAnchorPosition(
            GridPosition anchorPosition,
            BuildingFootprint footprint)
        {
            if (footprint.Width <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Width));
            if (footprint.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Height));

            var result = new List<GridPosition>(footprint.Width * footprint.Height);

            for (int x = 0; x < footprint.Width; x++)
            {
                for (int y = 0; y < footprint.Height; y++)
                {
                    result.Add(new GridPosition(
                        anchorPosition.X + x,
                        anchorPosition.Y + y
                    ));
                }
            }

            return result;
        }
        
        public static IReadOnlyList<GridPosition> GetCellsFromWorldPosition(
            GridSystem grid,
            Vector2 worldPosition,
            BuildingFootprint footprint)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (footprint.Width <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Width));
            if (footprint.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(footprint.Height));

            float cellSize = grid.CellSize;

            Vector2 bottomLeftWorld = worldPosition
                                      - new Vector2(
                                          (footprint.Width  - 1) * cellSize * 0.5f,
                                          (footprint.Height - 1) * cellSize * 0.5f
                                      );

            GridPosition bottomLeftCell = grid.WorldToGrid(bottomLeftWorld);

            var result = new List<GridPosition>(footprint.Width * footprint.Height);

            for (int x = 0; x < footprint.Width; x++)
            {
                for (int y = 0; y < footprint.Height; y++)
                {
                    result.Add(new GridPosition(
                        bottomLeftCell.X + x,
                        bottomLeftCell.Y + y
                    ));
                }
            }

            return result;
        }

    }
}