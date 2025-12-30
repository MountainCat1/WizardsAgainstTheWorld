using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace LevelSelector.Managers
{
    public class Summary
    {
        public enum Type
        {
            AutoSell,
            TotalProfit
        }

        private readonly Dictionary<(Type, string), int> _summaryRows = new();

        public IReadOnlyDictionary<(Type, string), int> SummaryRows => _summaryRows;

        public void Add(Type type, string id, int value)
        {
            var key = (type, id);
            if (_summaryRows.ContainsKey(key))
            {
                GameLogger.LogError(
                    $"Summary already contains row with type {type} and id {id}. Value: {value}");
            }
            else
            {
                _summaryRows[key] = value;
            }
        }
    }

    public interface IPostLevelHandler
    {
        public event Action<Summary> SummaryGenerated;
        public Summary GeneratedSummary { get; }
    }


    public class PostLevelHandler : MonoBehaviour, IPostLevelHandler
    {
        public event Action<Summary> SummaryGenerated;
        public Summary GeneratedSummary { get; private set; }

        [Inject] private ICrewManager _crewManager;
        [Inject] private IJsonService _serializer;

        private Summary _summary;

        private void Start()
        {
            _summary = null;

            if (_crewManager.IsInitialized)
            {
                OnCrewInitialized();
            }
            else
            {
                _crewManager.Changed += OnCrewInitialized;
            }
        }

        private void OnCrewInitialized()
        {
            if (_summary != null)
            {
                GameLogger.LogWarning("PostLevelHandler already initialized, skipping re-initialization.");
                return;
            }

            _summary = new Summary();

            var crewInventories = _crewManager.Crew
                .Select(x => x.Inventory)
                .ToArray();

            var autoSoldItems = new List<ItemData>();

            foreach (var inventory in crewInventories)
            {
                var items = AutoSell(inventory);

                foreach (var item in items)
                {
                    var alreadyFoundItem = autoSoldItems
                        .FirstOrDefault(x => x.Identifier == item.Identifier);

                    if (alreadyFoundItem != null)
                    {
                        alreadyFoundItem.Count += item.Count;
                    }
                    else
                    {
                        autoSoldItems.Add(item);
                    }
                }
            }

            foreach (var autoSoldItem in autoSoldItems)
            {
                _summary.Add(
                    Summary.Type.AutoSell,
                    autoSoldItem.Identifier,
                    Mathf.FloorToInt(autoSoldItem.Prefab.BaseCost * autoSoldItem.Count)
                );
            }

            var totalProfit = autoSoldItems.Sum(x => Mathf.FloorToInt(x.Prefab.BaseCost * x.Count));

            if (totalProfit != 0)
            {
                _summary.Add(Summary.Type.TotalProfit, "Total Profit", totalProfit);
            }

            GameLogger.Log($"PostLevelHandler generated summary:\n\n{_serializer.Serialize(_summary)}");

            GeneratedSummary = _summary;
            SummaryGenerated?.Invoke(_summary);
        }

        private ItemData[] AutoSell(InventoryData inventory)
        {
            var itemsToSell = inventory.Items
                .Where(x => x.Prefab.AutoSell)
                .ToArray();

            int money = 0;
            foreach (var item in itemsToSell)
            {
                money += Mathf.FloorToInt(item.Count * item.Prefab.BaseCost);
                inventory.RemoveItem(item);
            }

            if (money > 0)
            {
                _crewManager.Resources.AddMoney(money);
            }

            return itemsToSell;
        }
    }
}