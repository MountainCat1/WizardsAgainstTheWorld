using System.Linq;
using DefaultNamespace.PersistentData;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using Steam;
using UnityEngine;
using Zenject;

public class Cheats : MonoBehaviour
{
    [Inject] private ICrewManager _crewManager;
    [Inject] private IRegionManager _regionManager;
    [Inject] private IAchievementsManager _achievementsManager;
    [Inject] private IShopGenerator _shopGenerator;

    public void Update()
    {
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameLogger.Log("Cheat 1 activated");

            _crewManager.Resources.AddFuel(1);
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameLogger.Log("Cheat 2 activated");

            var currentLocation = _regionManager.Region.GetLocation(_crewManager.CurrentLocationId);

            currentLocation.Salvaged = !currentLocation.Salvaged;
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameLogger.Log("Cheat 3 activated");

            _crewManager.Resources.AddMoney(10);
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameLogger.Log("Cheat 4 activated");

            var location = _regionManager.Region.Locations;
            foreach (var loc in location)
            {
                loc.Visited = false;
            }
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha5))
        {
            GameLogger.Log("Cheat 5 activated");

            var currentLocation = _regionManager.Region.GetLocation(_crewManager.CurrentLocationId);
            currentLocation.ShopData = _shopGenerator.GenerateShop();

            var shopInventory = string.Join(", ",
                currentLocation.ShopData.inventory.Items.Select(item => item.Prefab.name));
            GameLogger.Log($"Shop generated for location: {currentLocation.Name} with shop: {shopInventory}");
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Alpha9))
        {
            GameLogger.Log("Cheat 3 activated");

            _achievementsManager.ResetAllAchievements();
            AchievementProgress.Update(new AchievementProgress().CreateDefault());
        }
    }
}