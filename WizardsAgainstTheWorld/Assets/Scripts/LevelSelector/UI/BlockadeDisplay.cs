using Managers;
using Managers.LevelSelector;
using UI;
using UnityEngine;
using Zenject;

namespace LevelSelector.UI
{
    public class BlockadeDisplay : MonoBehaviour
    {
        [Inject] private IRegionManager _regionManager;
        [Inject] private ICrewManager _crewManager;
        [SerializeField] private LevelSelectorUI levelSelectorUI;


        private void Start()
        {
            levelSelectorUI.LocationSelected += (_) => UpdateDetails();
            _regionManager.RegionChanged += UpdateDetails;
            _crewManager.Changed += UpdateDetails;
            
            UpdateDetails();
        }

        private void UpdateDetails()
        {
            gameObject.SetActive(_crewManager.IsBlocked());
        }
    }
}