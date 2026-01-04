using Building.Managers;
using TMPro;
using UnityEngine;
using Utilities;
using Zenject;

namespace Building.UI
{
    public class BuildingInspectorUI : MonoBehaviour
    {
        [SerializeField] private BuildingUpgradeEntryUI upgradeEntryUIPrefab;
        [SerializeField] private Transform upgradesContainer;
        
        [Inject] private DiContainer _diContainer;
        [Inject] private IBuilderManager _builderManager;
        
        public void Initialize(BuildingView buildingView)
        {
            gameObject.SetActive(true);
            
            var upgrades = buildingView.Prefab.AvailableUpgrades;

            upgradesContainer.CleanChildren();
            
            foreach (var upgrade in upgrades)
            {
                var upgradeEntryUI = _diContainer.InstantiatePrefabForComponent<BuildingUpgradeEntryUI>(
                    upgradeEntryUIPrefab,
                    upgradesContainer);
                
                upgradeEntryUI.Initialize(buildingView, upgrade, HandleUpgrade);
            }
        }

        private void HandleUpgrade(BuildingView building, BuildingPrefab upgradePrefab)
        {
            _builderManager.StartBuildingUpgrade(building, upgradePrefab);
        }

        public void Deinitialize()
        {
            gameObject.SetActive(false);
            upgradesContainer.CleanChildren();
        }
    }
}