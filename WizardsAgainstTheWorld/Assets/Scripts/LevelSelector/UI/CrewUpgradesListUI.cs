using System;
using Managers;
using UnityEngine;
using Zenject;

namespace LevelSelector.UI
{
    public class CrewUpgradesListUI : MonoBehaviour
    {
        [SerializeField] private CrewUpgradeEntryUI crewUpgradeEntryPrefab;
        [SerializeField] private Transform crewUpgradeListContainer;
        
        [Inject] private ICrewManager _crewManager;
        [Inject] private ICrewUpgradeManager _crewUpgradeManager;
        

        private void Start()
        {
            _crewManager.Changed += OnCrewChanged;
        }

        private void OnCrewChanged()
        {
            // Clear existing entries
            foreach (Transform child in crewUpgradeListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new entries for each upgrade
            foreach (var upgrade in _crewManager.Upgrades)
            {
                var entry = Instantiate(crewUpgradeEntryPrefab, crewUpgradeListContainer);
                var entryUI = entry.GetComponent<CrewUpgradeEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Initialize(_crewUpgradeManager.GetPrefab(upgrade.Id));
                }
                else
                {
                    Debug.LogWarning("CrewUpgradeEntryUI component not found on the instantiated prefab.");
                }
            }
        }
    }
}