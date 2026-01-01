using System.Collections.Generic;
using UnityEngine;
using Building.Data;
using Zenject;

namespace Building.Managers
{
    public interface IBuilderManager
    {
        List<BuildingPrefab> BuildingPrefabs { get; set; }

        bool CanPlaceBuilding(
            BuildingFootprint footprint,
            Vector2 worldPosition,
            out IReadOnlyList<GridPosition> cells
        );

        void PlaceBuilding(
            BuildingView building,
            BuildingFootprint footprint,
            Vector2 worldPosition
        );
    }

    public sealed class BuilderManager : MonoBehaviour, IBuilderManager
    {
        [Inject] private GridSystem _grid;

        [field: SerializeField] public List<BuildingPrefab> BuildingPrefabs { get; set; }

        public bool CanPlaceBuilding(
            BuildingFootprint footprint,
            Vector2 worldPosition,
            out IReadOnlyList<GridPosition> cells
        )
        {
            cells = GridUtilities.GetCellsFromWorldPosition(
                _grid,
                worldPosition,
                footprint
            );

            return AreCellsFree(cells);
        }


        public void PlaceBuilding(
            BuildingView building,
            BuildingFootprint footprint,
            Vector2 worldPosition
        )
        {
            var cells = GridUtilities.GetCellsFromWorldPosition(
                _grid,
                worldPosition,
                footprint
            );

            SetCellsOccupied(cells);
            
            building.transform.position = _grid.GetCenterFromCells(cells);
        }


        private void SetCellsOccupied(IReadOnlyList<GridPosition> cells)
        {
            foreach (var pos in cells)
                _grid.GetCell(pos).SetStructureBlocked(true);
        }


        private bool AreCellsFree(IReadOnlyList<GridPosition> occupiedCells)
        {
            foreach (var pos in occupiedCells)
            {
                if (!_grid.InBounds(pos))
                    return false;

                var cell = _grid.GetCell(pos);

                if (!cell.Walkable)
                    return false;
            }

            return true;
        }
    }
}