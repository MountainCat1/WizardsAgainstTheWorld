using System.Linq;
using DefaultNamespace.PersistentData;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using Steam.Steam;
using UnityEngine;
using Zenject;

namespace Steam
{
    public class LevelSelectAchievementManager : MonoBehaviour
    {
        [Inject] private IAchievementsManager _achievementsManager;
        [Inject] private ITravelManager _travelManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private IRegionManager _regionManager;

        private AchievementProgress _achievementProgress;

        private void Start()
        {
            _achievementProgress = AchievementProgress.Instance;
            _travelManager.Traveled += OnTravel;
            _crewManager.Changed += OnCrewChanged;

            var currentLocation = _regionManager.Region.Locations.First(x => _crewManager.CurrentLocationId == x.Id);
            if (currentLocation.Salvaged && _crewManager.Crew.Count > 0)
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementClearStation);
            }
        }

        private void OnCrewChanged()
        {
            if (_crewManager.Resources.Money >= 200)
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementGather200Money);
            }

            if (_crewManager.Upgrades.Count >= 5)
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementMiscGet5ShipUpgrades);
            }
         
            var allInventories = _crewManager.Crew
                .Select(creature => creature.Inventory)
                .Concat(new []{_crewManager.Inventory})
                .ToList();
            
            _achievementsManager.HandleInventoryAchievements(allInventories);
        }

        private void OnDestroy()
        {
            AchievementProgress.Update(_achievementProgress);
            AchievementProgress.Save();
        }

        private void OnTravel(RegionData region)
        {
            _achievementsManager.UnlockAchievement(Achievements.AchievementTravelRegion1);

            var regionTypeId = region.Type.ToLowerInvariant().Replace(" ", "");

            if (regionTypeId.Contains("quarantine"))
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementVisitZombies);
            }

            if (regionTypeId.Contains("alien"))
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementVisitAliens);
            }

            if (regionTypeId.Contains("federation"))
            {
                _achievementsManager.UnlockAchievement(Achievements.AchievementVisitFederation);
            }

            HandleEventHandling();
        }

        private void HandleEventHandling()
        {
            AchievementProgress.Update(_achievementProgress);
            _achievementsManager.CheckForAchievements();
        }
    }
}