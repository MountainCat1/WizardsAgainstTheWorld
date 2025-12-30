using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Managers.LevelSelector;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers
{
    public class ResourceUsagePreContext
    {
        public ResourceUsagePreContext(InGameResource resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }

        public int Amount { get; set; }
        public InGameResource Resource { get; set; }
        public bool Cancel { get; set; }
    }

    public interface ICrewManager
    {
        event Action Changed;
        event Action ChangedLocation;
        event Action Initialized;

        ProcessorEvent<ResourceUsagePreContext> PreResourceUsage { get; }

        ICollection<CreatureData> Crew { get; }
        IReadOnlyCollection<CrewUpgradeData> Upgrades { get; }
        InventoryData Inventory { get; }
        InGameResources Resources { get; }
        Guid CurrentLocationId { get; }
        int TravelCost { get; }
        bool IsInitialized { get; }

        void SetCrew(ICollection<CreatureData> creatures, ICollection<CrewUpgradeData> upgrades,
            InventoryData inventory, InGameResources resources, Guid currentLocationId);

        void ReRollCrew();
        void ChangeCurrentLocation(LocationData toLocation, bool useFuel = true);
        void ToggleCreature(CreatureData creature, bool value);
        void AddXp(CreatureData crewMember, int amount);
        void ForceChangeEvent();
        bool CanTravel();
        bool IsBlocked();
        void AddUpgrade(CrewUpgradeData data);
    }

    public class CrewManager : MonoBehaviour, ICrewManager
    {
        #region Fields & Dependencies

        [Inject] private IDataManager _dataManager;
        [Inject] private ICrewUpgradeManager _upgradeManager;
        [Inject] private IRegionManager _regionManager;
        [Inject] private DiContainer _diContainer;


        #endregion

        #region Events

        public event Action Changed;
        public event Action ChangedLocation;
        public event Action Initialized;
        public ProcessorEvent<ResourceUsagePreContext> PreResourceUsage { get; } = new();

        #endregion

        #region Properties

 

        public ICollection<CreatureData> Crew { get; private set; }
        public IReadOnlyCollection<CrewUpgradeData> Upgrades => _upgrades;
        public InventoryData Inventory { get; private set; }
        public InGameResources Resources { get; private set; }
        public Guid CurrentLocationId { get; private set; }
        public LocationData CurrentLocation { get; private set; }
        public int TravelCost => 1;
        public bool IsInitialized { get; private set; }

        #endregion

        #region Backing 

        private List<CrewUpgradeData> _upgrades = new List<CrewUpgradeData>();

        #endregion

        #region Public Methods

        public void SetCrew(ICollection<CreatureData> creatures, ICollection<CrewUpgradeData> upgrades,
            InventoryData inventory, InGameResources resources, Guid currentLocationId)
        {
            Crew = creatures;
            _upgrades = upgrades.ToList();
            
            foreach (var crewUpgradeData in upgrades)
            {
                var upgrade = _upgradeManager.Instantiate(crewUpgradeData.Id);
                if (upgrade == null)
                {
                    GameLogger.LogError($"Upgrade with ID {crewUpgradeData.Id} not found");
                    continue;
                }

                upgrade.InitializeLevelSelectorScene(_diContainer);
            }

            Inventory = inventory;
            Inventory.Changed += OnInventoryChanged;

            Resources = resources;
            Resources.Changed += OnResourcesChanged;

            CurrentLocationId = currentLocationId;
            CurrentLocation = _regionManager.Region.Locations.FirstOrDefault(l => l.Id == CurrentLocationId);

            IsInitialized = true;
            OnChanged();
            Initialized?.Invoke();
        }

        public void AddXp(CreatureData crewMember, int amount)
        {
            if (Crew == null || !Crew.Contains(crewMember))
            {
                GameLogger.LogError($"Tried to add XP to a crew member that is not in the crew: {crewMember.Name}");
                return;
            }

            crewMember.Level.AddXp(amount);
            OnChanged();
        }

        public void ChangeCurrentLocation(LocationData selectedLocation, bool useFuel = true)
        {
            if (useFuel && Resources.Fuel < TravelCost)
            {
                GameLogger.LogError("Tried to travel without enough fuel");
                return;
            }

            if (useFuel)
            {
                UseResource(InGameResource.Fuel, TravelCost);
            }

            MarkPreviousLocationVisited();

            CurrentLocation = selectedLocation;
            CurrentLocationId = selectedLocation.Id;

            if (!selectedLocation.CanDock)
            {
                // TODO: this should be somewhere elese
                // but basically, if you cannot dock, you cannot visit it
                // so its better to make it visited by just passing by
                selectedLocation.Salvaged = true;
            }

            ChangedLocation?.Invoke();
            OnChanged();
        }

        public bool CanTravel() => Resources?.Fuel >= TravelCost;

        public void ToggleCreature(CreatureData creature, bool value)
        {
            creature.Selected = value;
            OnChanged();
        }

        public void ForceChangeEvent() => OnChanged();

        public bool IsBlocked()
        {
            if (CurrentLocation == null)
            {
                GameLogger.LogWarning(
                    $"Current location with ID {CurrentLocationId} not found in region {_regionManager?.Region?.Name}");
                return false;
            }

            return CurrentLocation.Features.Any(f => f.BlockExit) && !CurrentLocation.Salvaged;
        }

        public void AddUpgrade(CrewUpgradeData data)
        {
            if (data == null)
            {
                GameLogger.LogError("Tried to add a null upgrade");
                return;
            }

            if (_upgrades.Any(u => u.Id == data.Id))
            {
                GameLogger.LogWarning($"Upgrade {data.Id} already exists in the crew upgrades");
                return;
            }

            _upgrades.Add(data);
            
            var upgrade = _upgradeManager.Instantiate(data.Id);
            
            upgrade.InitializeLevelSelectorScene(_diContainer);
            
            OnChanged();
        }

        public void ReRollCrew()
        {
            throw new NotImplementedException();
        }

        public void UseResource(InGameResource resource, int amount)
        {
            var context = new ResourceUsagePreContext(resource, amount);
            
            PreResourceUsage?.Invoke(context);
            
            if (context.Cancel)
            {
                GameLogger.Log($"Resource usage cancelled for {resource} with amount {amount}");
                return;
            }

            if (amount != 0)
            {
                switch (resource)
                {
                    case InGameResource.Money:
                        Resources.Money -= context.Amount;
                        break;
                    case InGameResource.Fuel:
                        Resources.Fuel -= context.Amount;
                        break;
                    case InGameResource.Juice:
                        Resources.Juice -= context.Amount;
                        break;
                    default:
                        GameLogger.LogError($"Unknown resource type: {resource}");
                        return;
                }
            }
        }

        #endregion

        #region Private Methods

        private void OnResourcesChanged()
        {
            GameLogger.Log("Resources changed:\n" + JsonConvert.SerializeObject(Resources));
            OnChanged();
        }

        private void OnInventoryChanged() => OnChanged();

        private void OnChanged() => Changed?.Invoke();

        private void MarkPreviousLocationVisited()
        {
            if (CurrentLocation != null)
                CurrentLocation.Visited = true;
        }

        #endregion
    }
}