using System.Linq;
using LevelSelector.Managers;
using Managers.LevelSelector;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LevelSelector.UI
{
    [RequireComponent(typeof(UISlide))]
    public class TravelManagerUI : MonoBehaviour
    {
        [Inject] ITravelManager _travelManager;
        [Inject] IRegionManager _regionManager;
        [Inject] IRegionGenerator _regionGenerator;
        [Inject] ILevelSelectorSlideManagerUI _levelSelectorSlideManager;
        [Inject] DiContainer _diContainer;

        [SerializeField] private Button travelButton;
        [SerializeField] private Transform travelButtonsContainer;

        private void Start()
        {
            if(_regionManager.NextRegions.Count() != 0)
                UpdateUI();
            
            _regionManager.RegionChanged += UpdateUI;
            
            _travelManager.Traveled += (_) => _levelSelectorSlideManager.ClearPanels();
        }

        void UpdateUI()
        {
            foreach (Transform child in travelButtonsContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var region in _regionManager.NextRegions)
            {
                var buttonGo = _diContainer.InstantiatePrefab(travelButton, travelButtonsContainer);

                var titleText = buttonGo.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                var descriptionText = buttonGo.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                
                titleText.text = region.Name;
                descriptionText.text = GetDescription(region);
                
                var button = buttonGo.GetComponent<Button>();
                button.onClick.AddListener(() => _travelManager.TravelToRegion(region));
            }
        }

        private string GetDescription(RegionData region)
        {
            var regionType = _regionGenerator.GetRegionType(region.Type);
            
            string result = "";

            result += $"<b>{regionType.typeName}</b>\n";
            result += $"\n";
            result += $"{regionType.typeDescription}\n";
            result += $"\n";
            result += $"\n";

            result += $"Location count: {region.Locations.Count}\n";
            result += $"Shop count: {region.Locations.Count(x => x.ShopData is not null)}\n";
            result += $"\n";
            result += $"\n";
            
            result += $"Danger level: {region.Difficulity}\n";

            foreach (var comment in region.AdditionalComments)
            {
                result += $"\n\n";
                result += comment;
            }
            
            return result;
        }
    }
}