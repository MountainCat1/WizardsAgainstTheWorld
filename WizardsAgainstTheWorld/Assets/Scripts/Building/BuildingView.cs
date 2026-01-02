using System;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Building
{
    [RequireComponent(typeof(BuildingPrefab))]
    public sealed class BuildingView : Entity
    {
        [Inject] private IAstarManager _astar;
        [Inject] private GridSystem _gridSystem;
        [field: SerializeField] public Weapon Weapon { get; set; }

        private BuildingPrefab _buildingPrefab;

        protected override void Awake()
        {
            base.Awake();

            _buildingPrefab = GetComponent<BuildingPrefab>();
        }

        private void OnDestroy()
        {
            _astar.ScanDelayed();

            var gridPositions = GridUtilities.GetCellsFromWorldPosition(
                _gridSystem,
                transform.position,
                _buildingPrefab.Footprint
            );

            foreach (var gridPosition in gridPositions)
            {
                var cell = _gridSystem.GetCell(gridPosition);
                if (cell == null)
                {
                    Debug.LogWarning("Cell is null when trying to free building footprint");
                    continue;
                }
                
                cell.SetStructureBlocked(false);
            }
        }
    }
}