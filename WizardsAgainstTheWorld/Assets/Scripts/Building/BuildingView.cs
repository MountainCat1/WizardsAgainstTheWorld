using System;
using Items.PassiveItems;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Building
{
    public sealed class BuildingView : Entity, IWeaponable
    {
        [Inject] private IAstarManager _astar;
        [Inject] private GridSystem _gridSystem;
        [field: SerializeField] public Weapon Weapon { get; set; }

        public event Action WeaponChanged;

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