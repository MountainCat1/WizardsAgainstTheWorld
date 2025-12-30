using System.Linq;
using Data;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utilities;
using Zenject;
using static Utilities.LocalizationHelper;

namespace UI
{
    public class LocationDetailsUI : MonoBehaviour
    {
        [Inject] private IRegionManager _regionManager;
        [Inject] private IDataManager _dataManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private ILocationInteractionManager _locationInteractionManager;
        [Inject] private ILevelSelectorSlideManagerUI _levelSelectorSlideManager;

        [SerializeField] private LevelSelectorUI levelSelectorUI;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [SerializeField] private SceneReference levelScene;

        [SerializeField] private Transform locationInteractionsParent;
        [SerializeField] private Button buttonPrefab;

        [SerializeField] private Color shopDescriptionColor;

        private LocationData _selectedLocation;

        private void Start()
        {
            levelSelectorUI.LocationSelected += (_) => UpdateDetails();
            _regionManager.RegionChanged += UpdateDetails;
            _crewManager.Changed += UpdateDetails;
            _levelSelectorSlideManager.Changed += (_) => UpdateDetails();

            if (_regionManager.Region != null)
            {
                UpdateDetails();
            }
        }
        private void UpdateDetails()
        {
            _selectedLocation = levelSelectorUI.SelectedLocation ??
                                _regionManager.Region.GetLocation(_crewManager.CurrentLocationId);

            if (_selectedLocation == null)
            {
                nameText.text = "UI.LocationDetails.NoSelection".Localize();
                descriptionText.text = "UI.LocationDetails.SelectToView".Localize();
                return;
            }

            GameLogger.Log($"Selected level: {_selectedLocation.Name}");

            // Update UI
            nameText.text = _selectedLocation.Name;
            descriptionText.text = ConstructDescription(_selectedLocation);

            var locationData = _regionManager.Region.Locations
                .FirstOrDefault(x => x.Id == _selectedLocation.Id);

            if (locationData == null)
            {
                locationData = _regionManager.Region.Locations.First(x => x.Type == LocationType.StartNode);
            }

            foreach (Transform child in locationInteractionsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var locationInteraction in _locationInteractionManager.Interactions)
            {
                if (!_locationInteractionManager.IsInteractionDisplayed(locationData, locationInteraction))
                    continue;

                var button = Instantiate(buttonPrefab, locationInteractionsParent);
                button.onClick.AddListener(() => locationInteraction.OnClick(locationData));

                var tooltipKey = locationInteraction.GetTooltipKey(locationData);

                if (!string.IsNullOrEmpty(tooltipKey))
                {
                    button.GetComponent<TooltipTrigger>().text = L(tooltipKey);
                }
                else
                {
                    button.GetComponent<TooltipTrigger>().enabled = false;
                }

                button.GetComponentInChildren<TextMeshProUGUI>().text = locationInteraction.MessageKey.Localize();
                button.interactable =
                    _locationInteractionManager.IsInteractionEnabled(locationData, locationInteraction);
                button.GetComponent<HighlightableUI>().Highlighted = locationInteraction.Selected?.Invoke() ?? false;
            }
        }

        private string ConstructDescription(LocationData selectedLocation)
        {
            var description = string.Empty;

            foreach (var feature in selectedLocation.Features)
            {
                description += $"* {feature.DescriptionKey.Localize()}\n";
            }

            if (selectedLocation.ShopData != null)
            {
                description += $"* {L("UI.LocationFeatures.ShopPresent")}\n";
            }

            if (selectedLocation.CanDock)
            {
                var movementValue = $"{selectedLocation.BaseEnemySpawnManaPerSecond:F2}";
                description += $"* {"UI.LocationFeatures.Movement".Localize(movementValue)}\n";
            }

            if (selectedLocation.Type == LocationType.EndNode)
            {
                description += $"* {L("UI.LocationFeatures.EndNode")}\n";
            }

            if (selectedLocation.Type == LocationType.EventNode && !selectedLocation.Visited)
                description += $"* {L("UI.LocationFeatures.EventNode")}\n";

            if (string.IsNullOrEmpty(description))
            {
                description += $"* {L("UI.LocationFeatures.Empty")}\n";
            }

            return description;
        }
    }
}