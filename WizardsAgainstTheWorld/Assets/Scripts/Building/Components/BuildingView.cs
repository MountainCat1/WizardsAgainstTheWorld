using System;
using System.Collections.Generic;
using Items.PassiveItems;
using Managers;
using UnityEngine;
using Zenject;
using static Utilities.LocalizationHelper;

namespace Building
{
    public sealed class BuildingView : Entity, IWeaponable
    {
        [Inject] private IAstarManager _astar;
        [Inject] private GridSystem _gridSystem;
        [field: SerializeField] public Weapon Weapon { get; set; }
        public GridPosition AnchorPosition { get; set; }
        public BuildingPrefab Prefab => _buildingPrefab ?? GetComponent<BuildingPrefab>();
        public override string Name => Prefab.Name;

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