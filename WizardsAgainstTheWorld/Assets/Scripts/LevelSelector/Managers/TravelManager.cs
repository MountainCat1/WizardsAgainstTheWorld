using System;
using System.Linq;
using Data;
using Managers;
using Managers.LevelSelector;
using UnityEngine;
using Zenject;

namespace LevelSelector.Managers
{
    public interface ITravelManager
    {
        public void TravelToRegion(RegionData region);
        public event Action<RegionData> Traveled;
    }

    public class TravelManager : MonoBehaviour, ITravelManager
    {
        [Inject] IRegionManager _regionManager;
        [Inject] ICrewManager _crewManager;
        [Inject] IDataManager _dataManager;

        public event Action<RegionData> Traveled;

        public void TravelToRegion(RegionData region)
        {
            _regionManager.SetRegion(region, false);

            _crewManager.ChangeCurrentLocation(region.Locations.First(x => x.Type == LocationType.StartNode),
                useFuel: false);

            _regionManager.SetRegion(region);

            var data = _dataManager.GetData();

            data.Region = region;
            data.CurrentLocationId = region.Locations
                .First(x => x.Type == LocationType.StartNode).Id
                .ToString();

            _dataManager.SaveData();

            _dataManager.LoadData();

            Traveled?.Invoke(region);
        }
    }
}