using System;
using System.Collections.Generic;
using UnityEngine;
using Building.Data;
using Managers;
using Zenject;

namespace Building.Managers
{
    public interface IBuilderManager
    {
        List<BuildingPrefab> BuildingPrefabs { get; set; }
        event Action<Entity> EntityPlaced;

        bool CanPlaceBuilding(
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            out IReadOnlyList<GridPosition> cells
        );

        void PlaceBuilding(
            BuildingView building,
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            Teams team = Teams.Player
        );

        void ConstructBuilding(
            BuildingView buildingPrefab,
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            Teams team = Teams.Player
        );
    }

    public sealed class BuilderManager : MonoBehaviour, IBuilderManager
    {
        [Inject] private GridSystem _grid;
        [Inject] private IEntityManager _entityManager;

        [field: SerializeField] public List<BuildingPrefab> BuildingPrefabs { get; set; }
        [field: SerializeField] public BuildingConstruction ConstructionPrefab { get; set; }
        public event Action<Entity> EntityPlaced;

        public bool CanPlaceBuilding(
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            out IReadOnlyList<GridPosition> cells
        )
        {
            cells = GridUtilities.GetCellsFromAnchorPosition(
                anchorPosition,
                footprint
            );

            return AreCellsFree(cells);
        }


        public void PlaceBuilding(
            BuildingView buildingPrefab,
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            Teams team = Teams.Player
        )
        {
            var cells = GridUtilities.GetCellsFromAnchorPosition(
                anchorPosition,
                footprint
            );

            SetCellsOccupied(cells);
            
            var position = _grid.GetCenterFromCells(cells);
            var building = _entityManager.SpawnEntity(buildingPrefab, position) as BuildingView ?? 
                           throw new InvalidOperationException("Spawned entity is not a BuildingView");
            
            building.GetComponent<CircleCollider2D>().radius = footprint.RadiusSize;
            EntityPlaced?.Invoke(building);
        }
        
        public void ConstructBuilding(
            BuildingView buildingPrefab,
            BuildingFootprint footprint,
            GridPosition anchorPosition,
            Teams team = Teams.Player
        )
        {
            var cells = GridUtilities.GetCellsFromAnchorPosition(
                anchorPosition,
                footprint
            );

            SetCellsOccupied(cells);
            
            var position = _grid.GetCenterFromCells(cells);
            var construction = _entityManager.SpawnEntity(ConstructionPrefab, position) as BuildingConstruction ?? 
                           throw new InvalidOperationException("Spawned entity is not a BuildingView");
            
            construction.GetComponent<CircleCollider2D>().radius = footprint.RadiusSize;
            construction.Initialize(buildingPrefab.GetComponent<BuildingPrefab>(), anchorPosition);
            EntityPlaced?.Invoke(construction);
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