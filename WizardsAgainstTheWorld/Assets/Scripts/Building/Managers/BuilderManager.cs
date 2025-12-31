using System.Collections.Generic;
using UnityEngine;
using Building.Data;
using Zenject;

namespace Building.Managers
{
    public interface IBuilderManager
    {
        bool CanPlaceBuilding(
            GridPosition center,
            BuildingFootprint footprint,
            out IReadOnlyList<GridPosition> occupiedCells);

        void PlaceBuilding(
            BuildingView buildingView,
            GridPosition center);

        List<BuildingPrefab> BuildingPrefabs { get; set; }
    }

    public sealed class BuilderManager : MonoBehaviour, IBuilderManager
    {
        [Inject] private GridSystem _grid;
        
        [field: SerializeField] public List<BuildingPrefab> BuildingPrefabs { get; set; }

        public bool CanPlaceBuilding(
            GridPosition center,
            BuildingFootprint footprint,
            out IReadOnlyList<GridPosition> occupiedCells)
        {
            occupiedCells = GetFootprintCells(center, footprint);

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
            GridPosition center)
        {
            var footprint = buildingView.Footprint;
            var cells = GetFootprintCells(center, footprint);

            foreach (var pos in cells)
                _grid.GetCell(pos).SetStructureBlocked(true);

            buildingView.transform.position = _grid.GridToWorld(center);
        }

        private static List<GridPosition> GetFootprintCells(
            GridPosition center,
            BuildingFootprint footprint)
        {
            var result = new List<GridPosition>(footprint.Width * footprint.Height);

            var startX = center.X - footprint.Width / 2;
            var startY = center.Y - footprint.Height / 2;

            for (var x = 0; x < footprint.Width; x++)
            for (var y = 0; y < footprint.Height; y++)
                result.Add(new GridPosition(startX + x, startY + y));

            return result;
        }
    }
}