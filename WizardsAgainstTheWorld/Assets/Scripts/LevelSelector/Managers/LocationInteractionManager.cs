using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Managers;
using Managers.LevelSelector;
using UI;
using UnityEngine;
using Utilities;
using Zenject;

namespace LevelSelector.Managers
{
    public class LocationInteraction
    {
        public Func<LocationData, bool> IsDisplayed;
        public Func<LocationData, bool> IsEnabled;
        public string MessageKey;
        public Action<LocationData> OnClick;
        public Func<LocationData, string> GetTooltipKey = _ => null;
        public Func<bool> Selected = () => false;
        public LocationInteractionType Type { get; set; }
    }

    public enum LocationInteractionType
    {
        Embark,
        Disengage,
        Dock,
        Inventory,
        Upgrade,
        Trade,
        Jump
    }

    public class CheckVisibleInteractionPreContext
    {
        public CheckVisibleInteractionPreContext(LocationData location, LocationInteraction interaction,
            bool isDisplayed)
        {
            Location = location;
            Interaction = interaction;
            IsDisplayed = isDisplayed;
        }

        public LocationData Location { get; }
        public LocationInteraction Interaction { get; }
        public bool IsDisplayed { get; set; }
    }

    public interface ILocationInteractionManager
    {
        IReadOnlyList<LocationInteraction> Interactions { get; }
        
        ProcessorEvent<CheckVisibleInteractionPreContext> CheckVisibleInteractionProcessor { get; }

        public bool IsInteractionDisplayed(LocationData location, LocationInteraction interaction)
        {
            var preContext = new CheckVisibleInteractionPreContext(
                location: location,
                interaction: interaction,
                isDisplayed: interaction.IsDisplayed(location)
            );
            
            CheckVisibleInteractionProcessor.Invoke(preContext);

            return preContext.IsDisplayed;
        }
        
        public bool IsInteractionEnabled(LocationData location, LocationInteraction interaction)
        {
            return interaction.IsEnabled(location);
        }
    }

    public class LocationInteractionManager : MonoBehaviour, ILocationInteractionManager
    {
        public ProcessorEvent<CheckVisibleInteractionPreContext> CheckVisibleInteractionProcessor { get; } = new();
        
        [Inject] IRegionManager _regionManager;
        [Inject] IDataManager _dataManager;
        [Inject] IDataResolver _dataResolver;
        [Inject] ICrewManager _crewManager;
        [Inject] ILevelSelectorSlideManagerUI _levelSelectorSlideManagerUI;
        [Inject] ISceneLoader _sceneLoader;
        
        [SerializeField] private SceneReference levelScene;

        public IReadOnlyList<LocationInteraction> Interactions => _interactions;

        private readonly List<LocationInteraction> _interactions = new();
        private const int DisengagementCost = 3;

        private void Awake()
        {
            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Embark,
                MessageKey = "UI.LocationInteractions.Embark",
                IsDisplayed = location => _regionManager.GetDistance(_crewManager.CurrentLocationId, location.Id) == 1,
                IsEnabled = _ => _crewManager.CanTravel() && !_crewManager.IsBlocked(),
                OnClick = location =>
                {
                    _crewManager.ChangeCurrentLocation(location);
                    SaveData();
                },
                GetTooltipKey = location =>
                {
                    if (_crewManager.CanTravel() && !_crewManager.IsBlocked())
                    {
                        return null;
                    }

                    if (_crewManager.IsBlocked())
                    {
                        return "UI.LocationInteractions.BlockedTooltip";
                    }

                    return "UI.LocationInteractions.NoFuelTooltip";
                }
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Disengage,
                MessageKey = "UI.LocationInteractions.Disengage",
                IsDisplayed = location =>
                    _regionManager.GetDistance(_crewManager.CurrentLocationId, location.Id) == 1 &&
                    _crewManager.IsBlocked(),
                IsEnabled = _ => DisengagementCost <= _crewManager.Resources.Fuel,
                OnClick = location =>
                {
                    _crewManager.ChangeCurrentLocation(location);
                    _crewManager.Resources.AddFuel(-DisengagementCost + _crewManager.TravelCost);
                    SaveData();
                },
                GetTooltipKey = location =>
                {
                    if (DisengagementCost <= _crewManager.Resources.Fuel)
                    {
                        return null;
                    }

                    return "UI.LocationInteractions.DisengagementCostTooltip";
                }
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Dock,
                MessageKey = "UI.LocationInteractions.Dock",
                IsDisplayed = location => _crewManager.CurrentLocationId == location.Id && location.CanDock,
                IsEnabled = location => !location.Salvaged && _crewManager.Crew.Any(x => x.Selected),
                OnClick = LoadLevel,
                GetTooltipKey = location =>
                {
                    if (location.Salvaged)
                    {
                        return "UI.LocationInteractions.AlreadyVisitedTooltip";
                    }

                    if (!_crewManager.Crew.Any(x => x.Selected))
                    {
                        return "UI.LocationInteractions.NoCrewSelectedTooltip";
                    }

                    return null;
                }
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Inventory,
                MessageKey = "UI.LocationInteractions.Inventory",
                IsDisplayed = _ => true,
                IsEnabled = _ => true,
                OnClick = _ => { _levelSelectorSlideManagerUI.Toggle(LevelSelectorSlideManagerUI.LevelSelectorUIPanel.ShipInventory); },
                Selected = () => _levelSelectorSlideManagerUI.CurrentPanel == LevelSelectorSlideManagerUI.LevelSelectorUIPanel.ShipInventory
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Upgrade,
                MessageKey = "UI.LocationInteractions.Upgrade",
                IsDisplayed = location => location.ShopData != null && IsCurrentLocation(location),
                IsEnabled = _ => true,
                OnClick = location => { _levelSelectorSlideManagerUI.Toggle(LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Upgrade); },
                Selected = () => _levelSelectorSlideManagerUI.CurrentPanel == LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Upgrade
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Trade,
                MessageKey = "UI.LocationInteractions.Trade",
                IsDisplayed = location => IsCurrentLocation(location) && location.ShopData is not null,
                IsEnabled = _ => true,
                OnClick = _ => { _levelSelectorSlideManagerUI.Toggle(LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Shop); },
                Selected = () => _levelSelectorSlideManagerUI.CurrentPanel == LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Shop
            });

            _interactions.Add(new LocationInteraction
            {
                Type = LocationInteractionType.Jump,
                MessageKey = "UI.LocationInteractions.Jump",
                IsDisplayed = location => location.Type == LocationType.EndNode,
                IsEnabled = location => _crewManager.CurrentLocationId == location.Id,
                OnClick = _ => { _levelSelectorSlideManagerUI.Show(LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Travel); },
            });
        }

        private void SaveData()
        {
            var gameData = _dataManager.GetData();

            gameData.Inventory = _crewManager.Inventory;
            gameData.Resources = _crewManager.Resources;
            gameData.Creatures = _crewManager.Crew.ToList();
            gameData.CurrentLocationId = _crewManager.CurrentLocationId.ToString();
            gameData.Region = _regionManager.Region;
            gameData.Upgrades = _crewManager.Upgrades.ToList();

            _dataManager.SaveData();
        }

        public void LoadLevel(LocationData locationData)
        {
            GameLogger.Log($"Loading level: {locationData.Name}");

            locationData.Salvaged = true;
            _regionManager.Region.Locations.First(l => l.Id == locationData.Id).Salvaged = true;

            SaveData();

            GameManager.GameSetup = new GameSetup
            {
                Settings = GenerateMapSettingsData.ToSettings(locationData.MapSettings),
                Name = locationData.Name,
                Location = locationData,
                Level = _regionManager.Region.Difficulity,
            };

            // Load level scene
            _sceneLoader.LoadScene(Scenes.GameplayScene);
        }

        private bool IsCurrentLocation(LocationData locationData)
        {
            return _crewManager.CurrentLocationId == locationData.Id;
        }
    }
}