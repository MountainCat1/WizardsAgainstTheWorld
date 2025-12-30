using System.Collections.Generic;
using CrewUpgrades;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ICrewUpgradeManager
    {
        public CrewUpgrade GetPrefab(string id);
        public CrewUpgrade Instantiate(string id);
    }
    
    public class CrewUpgradeManager : MonoBehaviour, ICrewUpgradeManager
    {
        [Inject] private DiContainer _diContainer;
        
        public CrewUpgrade GetPrefab(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                GameLogger.LogError("Attempted to get CrewUpgrade prefab with null or empty identifier.");
                return null;
            }

            if (_prefabs.TryGetValue(id, out var prefab))
            {
                return prefab;
            }
            
            GameLogger.LogWarning($"CrewUpgrade prefab with identifier '{id}' not found.");
            return null;
        }

        public CrewUpgrade Instantiate(string id)
        {
            var prefab = GetPrefab(id);
            if (prefab == null)
            {
                GameLogger.LogError($"Cannot instantiate CrewUpgrade with identifier '{id}' because it does not exist.");
                return null;
            }
            
            GameLogger.Log($"Instantiating CrewUpgrade with identifier '{id}'.");

            var instance = Instantiate(prefab);
            return instance;
        }

        private Dictionary<string, CrewUpgrade> _prefabs;
        
        private const string CrewUpgradeResourcePath = "CrewUpgrades";

        private void Awake()
        {
            _prefabs = new Dictionary<string, CrewUpgrade>();
            var prefabs = Resources.LoadAll<CrewUpgrade>(CrewUpgradeResourcePath);

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                {
                    GameLogger.LogWarning("Found a null CrewUpgrade prefab in Resources/CrewUpgrades.");
                    continue;
                }

                var identifier = prefab.GetIdentifier();
                if (string.IsNullOrEmpty(identifier))
                {
                    GameLogger.LogWarning($"CrewUpgrade prefab '{prefab.name}' does not have a valid identifier.");
                    continue;
                }

                if (_prefabs.ContainsKey(identifier))
                {
                    GameLogger.LogWarning($"Duplicate CrewUpgrade identifier found: {identifier}. Using the first one loaded.");
                    continue;
                }

                _prefabs[identifier] = prefab;
            }
        }
    }
}