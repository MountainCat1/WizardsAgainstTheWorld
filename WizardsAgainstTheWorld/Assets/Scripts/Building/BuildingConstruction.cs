using System.Collections.Generic;
using System.Linq;
using Building.Managers;
using GameplayScene.Managers;
using Zenject;

namespace Building
{
    public class BuildingConstruction : Entity
    {
        [Inject] private IBuilderManager _builderManager;
        
        public BuildingPrefab BuildingPrefab { get; private set; }
        public GridPosition AnchorPosition { get; private set; }
        
        private readonly List<GameResource> _paidResources = new();
        
        public void Initialize(BuildingPrefab buildingPrefab, GridPosition anchorPosition)
        {
            AnchorPosition = anchorPosition;
            BuildingPrefab = buildingPrefab;
        }
        
        public void PayResource(GameResource resource)
        {
            var paidResource = _paidResources
                .FirstOrDefault(x => x.Type == resource.Type);
            
            if (paidResource == null)
            {
                paidResource = new GameResource(resource.Type)
                {
                    Amount = 0
                };
                
                _paidResources.Add(paidResource);
            }

            paidResource.Amount += resource.Amount;
            
            if(IsPayed())
                FinishConstruction();
        }

        private void FinishConstruction()
        {
            Health.Kill();
            _builderManager.PlaceBuilding(
                BuildingPrefab.View,
                BuildingPrefab.Footprint,
                AnchorPosition,
                Teams.Player
            );
        }

        public List<GameResource> GetRequiredResources()
        {
            var requiredResources = BuildingPrefab.costs
                .Select(x => x.ToGameResource()).ToList();

            foreach (var paidResource in _paidResources)
            {
                var requiredResource = requiredResources
                    .FirstOrDefault(x => x.Type == paidResource.Type);
                
                if (requiredResource != null)
                {
                    requiredResource.Amount -= paidResource.Amount;
                }
            }
            
            return requiredResources
                .Where(x => x.Amount > 0)
                .ToList();
        }

        public bool IsPayed()
        {
            return !GetRequiredResources().Any();
        }

    }
}