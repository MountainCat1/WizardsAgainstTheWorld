using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Items;
using LevelSelector;
using LevelSelector.Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers.LevelSelector
{
    public class LocationSelectorManager : MonoBehaviour
    {
        [Inject] private IRegionGenerator _regionGenerator;
        [Inject] private IRegionManager _regionManager;
        [Inject] private IDataManager _dataManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private IItemManager _itemManager;
        [Inject] private ICrewGenerator _crewGenerator;
        [Inject] private ISceneLoader _sceneLoader;

        [SerializeField] private bool skipLoad = false;

        [SerializeField] private int startingCrewSize = 3;
        [SerializeField] private List<ItemBehaviour> startingItems;
        [SerializeField] private float startingMoney = 50;
        [SerializeField] private float startingFuel = 5;
        [SerializeField] private float startingJuice = 200;
        
        [SerializeField] private RegionType firstRegionType;

        [SerializeField] private SceneReference mainMenuScene;

        [Inject] private IInputManager _inputManager;

        private void Start()
        {
            _inputManager.GoBackToMenu += GoBackToMenu;

            var data = _dataManager.LoadData();

            if (skipLoad || data?.Region == null)
            {
                var region = _regionGenerator.Generate(1f, regionType: firstRegionType);
                var currentNodeId = region.Locations.First(x => x.Type == LocationType.StartNode).Id;
                _regionManager.SetRegion(region);


                var startingInventory = new InventoryData();
                startingItems.ForEach(x => startingInventory.AddItem(ItemData.FromItem(x)));
                
                var startingCreatures = new List<CreatureData>();
                for (int i = 0; i < startingCrewSize; i++)
                {
                    startingCreatures.Add(_crewGenerator.GenerateCrew());
                }

                var startingResources = new InGameResources()
                {
                    Money = (decimal)startingMoney,
                    Fuel = (decimal)startingFuel,
                    Juice = (decimal)startingJuice,
                };

                _crewManager.SetCrew(startingCreatures, new List<CrewUpgradeData>(), startingInventory, startingResources, currentNodeId);

                var gameData = _dataManager.GetData();

                gameData.Region = _regionManager.Region;
                gameData.Creatures = _crewManager.Crew.ToList();
                gameData.Inventory = _crewManager.Inventory;
                gameData.CurrentLocationId = currentNodeId.ToString();
                gameData.Resources = _crewManager.Resources;
                gameData.Upgrades = new List<CrewUpgradeData>();
                
;
                _dataManager.SaveData();
            }

            LoadData();
        }

        private void GoBackToMenu()
        {
            _dataManager.SaveData();
            _sceneLoader.LoadScene(Scenes.MainMenu);
        }

        private void LoadData()
        {
            var data = _dataManager.LoadData();

            _regionManager.SetRegion(data.Region);

            _dataManager.DoStuffToDataSoItWorks(data);

            _crewManager.SetCrew(data.Creatures, data.Upgrades, data.Inventory, data.Resources, Guid.Parse(data.CurrentLocationId));
        }
    }
}