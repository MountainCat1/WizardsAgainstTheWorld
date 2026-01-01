using System.Collections.Generic;
using UnityEngine;
using Building.Data;
using Zenject;

namespace Building.Managers
{
    public interface IBuilderManager
    {
        bool CanPlaceBuilding(
            BuildingFootprint footprint,
            IReadOnlyList<GridPosition> occupiedCells);

        void PlaceBuilding(
            BuildingView buildingView,
            IReadOnlyList<GridPosition> cells);

        List<BuildingPrefab> BuildingPrefabs { get; set; }
    }

    public sealed class BuilderManager : MonoBehaviour, IBuilderManager
    {
        [Inject] private GridSystem _grid;
        
        [field: SerializeField] public List<BuildingPrefab> BuildingPrefabs { get; set; }

        public bool CanPlaceBuilding(
            BuildingFootprint footprint,
            IReadOnlyList<GridPosition> occupiedCells)
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

        public void PlaceBuilding(
            BuildingView buildingView,
            IReadOnlyList<GridPosition> cells)
        {
            foreach (var pos in cells)
                _grid.GetCell(pos).SetStructureBlocked(true);

            buildingView.transform.position = _grid.GetCenterFromCells(cells);
        }
    }
}