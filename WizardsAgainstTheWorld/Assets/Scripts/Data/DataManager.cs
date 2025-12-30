using System;
using System.IO;
using System.Linq;
using Managers;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Data
{
    public interface IDataManager
    {
        void SaveData();
        GameData LoadData();
        bool HasData();
        GameData GetData();
        void DeleteData();
        void DoStuffToDataSoItWorks(GameData data);
    }

    public class DataManager : IDataManager
    {
        [Inject] private IItemManager _itemManager;
        [Inject] private JsonSerializerSettings _serializerSettings;
         
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        private GameData _gameData;
        
        public void SaveData()
        {
            GameLogger.Log("Saving data...");
            SaveData(_gameData);
        }

        public void SaveData(GameData gameData)
        {
            try
            {
                // Serialize with Type Handling and Vector2 support
                string json = JsonConvert.SerializeObject(gameData, Formatting.Indented, _serializerSettings);

                GameLogger.Log($"Saving game data to: {SaveFilePath}\n{json}");

                File.WriteAllText(SaveFilePath, json);
                GameLogger.Log("Game data saved successfully.");

                _gameData = gameData;
            }
            catch (IOException e)
            {
                GameLogger.LogError($"Failed to save data: {e.Message}");
            }
        }

        public GameData LoadData()
        {
            _gameData = null;

            if (!File.Exists(SaveFilePath))
            {
                GameLogger.LogWarning("Save file not found! Skipping load.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);

                // Deserialize with Type Handling and Vector2 support
                GameData gameData;


                try
                {
                    gameData = JsonConvert.DeserializeObject<GameData>(json, _serializerSettings);
                }
                catch (Exception e)
                {
                    GameLogger.LogError($"Failed to load data: {e.Message}");
                    return null;
                }
                

                // Load sprites for items
                GameLogger.Log($"Game data loaded successfully.\n{SaveFilePath}");

                _gameData = gameData;
                return gameData;
            }
            catch (IOException e)
            {
                GameLogger.LogError($"Failed to load data: {e.Message}");
            }

            return null;
        }

        public bool HasData()
        {
            return File.Exists(SaveFilePath);
        }

        public GameData GetData()
        {
            if(_gameData is not null) return _gameData;

            GameLogger.Log("Creating new game data...");

            _gameData = new GameData();
            
            return _gameData;
        }

        // TODO: This should be in a separate manager
        public void DoStuffToDataSoItWorks(GameData data)
        {
            var crewItems = data.Creatures.SelectMany(x => x.Inventory.Items).ToList();
            var inventoryItems = data.Inventory.Items;

            foreach (var item in crewItems)
            {
                item.Prefab = _itemManager.GetItemPrefab(item.Identifier);
            }
            
            foreach (var item in inventoryItems)
            {
                item.Prefab = _itemManager.GetItemPrefab(item.Identifier);
            }
        }
        
        public void DeleteData()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                GameLogger.Log("Save file deleted.");
            }
            else
            {
                GameLogger.LogWarning("Save file not found! Skipping delete.");
            }
        }
    }
}