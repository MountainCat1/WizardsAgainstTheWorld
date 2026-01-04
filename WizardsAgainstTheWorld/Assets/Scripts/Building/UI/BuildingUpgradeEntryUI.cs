using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Building.UI
{
    public class BuildingUpgradeEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text upgradeNameText;
        [SerializeField] private TMP_Text upgradeCost;
        [SerializeField] private Button upgradeButton;
        
        private Action<BuildingView, BuildingPrefab> _upgradeCallback;
        private BuildingView _building;
        private BuildingPrefab _upgradePrefab;
        
        public void Initialize(BuildingView building, BuildingPrefab upgradePrefab, Action<BuildingView, BuildingPrefab> upgradeCallback)
        {
            upgradeNameText.text = upgradePrefab.Name;
            upgradeCost.text = upgradePrefab.GetCostText();
            
            _building = building;
            _upgradePrefab = upgradePrefab;
            _upgradeCallback = upgradeCallback;
            
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(HandleUpgradeButtonClicked);
        }

        private void HandleUpgradeButtonClicked()
        {
            _upgradeCallback?.Invoke(_building, _upgradePrefab);
        }
    }
}