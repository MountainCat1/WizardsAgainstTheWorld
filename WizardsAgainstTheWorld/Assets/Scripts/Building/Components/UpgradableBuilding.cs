using System.Collections.Generic;
using Building.Abstractions;
using Building.Managers;
using UnityEngine;
using Zenject;

namespace Building
{
    public class UpgradableBuilding : BuildingComponent
    {
        [Inject] private IBuilderManager _builderManager;
        
        [field: SerializeField] public List<BuildingPrefab> UpgradePrefabs { get; private set; }

        public void StartUpgrade(BuildingPrefab upgradePrefab)
        {
            _builderManager.StartBuildingUpgrade(Building, upgradePrefab);
        }
    }
}