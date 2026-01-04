using System;
using Items.PassiveItems;
using Managers;
using UnityEngine;
using Zenject;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _astar.ScanDelayed();

            GridUtilities.CleanFootprint(_gridSystem, Prefab.Footprint, transform.position);
        }
    }
}