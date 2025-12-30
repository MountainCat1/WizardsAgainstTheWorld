using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VictoryConditions;
using Zenject;

namespace Managers
{
    public interface IVictoryConditionManager
    {
        event Action VictoryConditionsChanged;
        event Action VictoryAchieved;
        event Action EndGameAchieved;
        event Action LevelEnded;

        IEnumerable<KeyValuePair<VictoryCondition, bool>> VictoryConditions { get; }
        IEnumerable<KeyValuePair<VictoryCondition, bool>> EndGameConditions { get; }
        IEnumerable<KeyValuePair<VictoryCondition, bool>> Conditions { get; }

        void Check();
        void SetVictoryAndDefeatConditions(VictoryCondition[] victoryConditions, VictoryCondition[] endGameCondition);
    }

    public class VictoryConditionManager : MonoBehaviour, IVictoryConditionManager
    {
        public event Action VictoryConditionsChanged;
        public event Action VictoryAchieved;
        public event Action EndGameAchieved;
        public event Action LevelEnded;

        [Inject] private ISpawnerManager _spawnerManager;
        [Inject] private ISignalManager _signalManager;
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private DiContainer _diContainer;

        public IEnumerable<KeyValuePair<VictoryCondition, bool>> VictoryConditions => _victoryConditionStates;
        public IEnumerable<KeyValuePair<VictoryCondition, bool>> EndGameConditions => _defeatConditionStates;
        public IEnumerable<KeyValuePair<VictoryCondition, bool>> Conditions => _victoryConditionStates
            .Concat(_defeatConditionStates);
        
        public bool HasLevelEnded { get; private set; }

        private readonly Dictionary<VictoryCondition, bool> _victoryConditionStates = new();
        private readonly Dictionary<VictoryCondition, bool> _defeatConditionStates = new();

        public void SetVictoryAndDefeatConditions(VictoryCondition[] victoryConditions, VictoryCondition[] endGameCondition)
        {
            HasLevelEnded = false;
            _victoryConditionStates.Clear();
            _defeatConditionStates.Clear();

            var victoryConditionsInstances =   victoryConditions
                .Distinct()
                .Select(Instantiate)
                .ToArray();
            
            var endGameConditions = endGameCondition
                .Distinct()
                .Select(Instantiate)
                .ToArray();
            
            foreach (var condition in victoryConditionsInstances.Concat(endGameConditions))
            {
                _diContainer.Inject(condition);
                
                condition.Start();
            }

            foreach (var v in endGameConditions)
            {
                _defeatConditionStates[v] = false;
            }

            foreach (var v in victoryConditionsInstances)
            {
                _victoryConditionStates[v] = false;
            }

            _spawnerManager.Spawned += OnSpawned;

            foreach (var creature in _creatureManager.GetCreatures())
            {
                OnSpawned(creature);
            }

            _signalManager.Signaled += (_) => UpdateConditions();

            VictoryConditionsChanged?.Invoke();
        }

        private void OnSpawned(Component component)
        {
            if (component is Creature creature)
            {
                creature.Health.Death += (_) => UpdateConditions();
                creature.Inventory.Changed += () => UpdateConditions();
            }
        }

        private void UpdateConditions()
        {
            if (HasLevelEnded)
                return;

            // Update all conditions
            foreach (var key in _victoryConditionStates.Keys.ToList())
                _victoryConditionStates[key] = key.Check();

            foreach (var key in _defeatConditionStates.Keys.ToList())
                _defeatConditionStates[key] = key.Check();

            VictoryConditionsChanged?.Invoke();

            // Check victory
            if (_victoryConditionStates.Values.All(x => x))
            {
                VictoryAchieved?.Invoke();
            }
            // Check en game
            if (_defeatConditionStates.Values.Any(x => x))
            {
                EndGameAchieved?.Invoke();
            }
        }

        public void Check()
        {
            UpdateConditions();
        }

    }
}
