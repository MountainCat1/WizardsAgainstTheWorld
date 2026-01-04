using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayScene.Managers
{
    public enum GameResourceType
    {
        Wood,
        Stone,
        Gold
    }

    [Serializable]
    public class GameResourceData
    {
        public int amount;
        public GameResourceType type;

        public GameResource ToGameResource()
        {
            return new GameResource(type)
            {
                Amount = amount
            };
        }
    }
    
    public record GameResource
    {
        public GameResourceType Type { get; }
        public int Amount { get; set; }

        public GameResource(GameResourceType type)
        {
            Type = type;
        }
    }
    
    public interface IResourceManager
    {
        public event Action Changed;

        void AddResource(GameResourceType type, int amount);
        IEnumerable<GameResourceType> AvailableResources { get; }
        GameResource GetResource(GameResourceType resourceType);
        Sprite GetIcon(GameResourceType resourceType);
        void AddResource(GameResource itemResources);
    }

    [Serializable]
    public class ResourceIcon
    {
        public GameResourceType resourceType;
        public Sprite icon;
    }

    public class ResourceManager : MonoBehaviour, IResourceManager
    {
        public event Action Changed;

        public IEnumerable<GameResourceType> AvailableResources =>
            Enum.GetValues(typeof(GameResourceType)).OfType<GameResourceType>();

        private readonly Dictionary<GameResourceType, GameResource> _resources = new();

        [SerializeField] private List<ResourceIcon> resourceIcons;

        private void Awake()
        {
            foreach (var resourceType in Enum.GetValues(typeof(GameResourceType)))
            {
                var type = (GameResourceType)resourceType;
                _resources.Add(type, new GameResource(type)
                {
                    Amount = 0,
                });
            }
            
            _resources[GameResourceType.Wood].Amount = 10;
        }

        public GameResource GetResource(GameResourceType resourceType)
        {
            return _resources[resourceType];
        }

        public Sprite GetIcon(GameResourceType resourceType)
        {
            return resourceIcons
                .FirstOrDefault(x => x.resourceType == resourceType)?
                .icon;
        }

        public void AddResource(GameResource itemResources)
        {
            if (_resources.ContainsKey(itemResources.Type))
            {
                _resources[itemResources.Type].Amount += itemResources.Amount;
                Changed?.Invoke();
            }
        }

        public void AddResource(GameResourceType type, int amount)
        {
            if (_resources.ContainsKey(type))
            {
                _resources[type].Amount += amount;
                Changed?.Invoke();
            }
        }
    }
}