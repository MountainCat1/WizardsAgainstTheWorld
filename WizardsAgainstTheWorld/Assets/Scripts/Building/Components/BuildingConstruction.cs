using System;
using System.Collections.Generic;
using System.Linq;
using Building.Managers;
using GameplayScene.Managers;
using Managers;
using UnityEngine;
using Zenject;

namespace Building
{
    public class BuildingConstruction : Entity
    {
        // Events
        public event Action ProgressChanged;
        
        // Dependencies
        [Inject] private IBuilderManager _builderManager;
        [Inject] private ISoundPlayer _soundPlayer;
        [Inject] private IAstarManager _astar;
        [Inject] private GridSystem _gridSystem;
        
        // Properties
        public float Progress => CalculateProgress();

        public BuildingPrefab BuildingPrefab { get; private set; }
        public GridPosition AnchorPosition { get; private set; }

        // Serialized Fields
        [SerializeField] private SpriteRenderer previewRenderer;
        [SerializeField] private AudioClip constructionCompleteSound;
        
        // Fields
        private readonly List<GameResource> _paidResources = new();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _astar.ScanDelayed();
            GridUtilities.CleanFootprint(_gridSystem, BuildingPrefab.Footprint, transform.position);
        }
        
        public void Initialize(BuildingPrefab buildingPrefab, GridPosition anchorPosition)
        {
            AnchorPosition = anchorPosition;
            BuildingPrefab = buildingPrefab;
            previewRenderer.sprite = buildingPrefab.MainSpriteRenderer.sprite;
            previewRenderer.size = buildingPrefab.MainSpriteRenderer.size;
            previewRenderer.transform.localScale = buildingPrefab.MainSpriteRenderer.transform.localScale;
            previewRenderer.transform.localPosition = buildingPrefab.MainSpriteRenderer.transform.position -
                                                      buildingPrefab.transform.position;
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
            
            ProgressChanged?.Invoke();
            
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
            
            _soundPlayer.PlaySound(
                constructionCompleteSound,
                transform.position,
                SoundType.UI
            );
        }

        public List<GameResource> GetRequiredResources()
        {
            var requiredResources = BuildingPrefab.Costs
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
        
        private float CalculateProgress()
        {
            float totalCost = BuildingPrefab.Costs.Sum(x => x.amount);
            float paidCost = 0f;

            foreach (var paidResource in _paidResources)
            {
                var costResource = BuildingPrefab.Costs
                    .FirstOrDefault(x => x.type == paidResource.Type);
                
                if (costResource != null)
                {
                    paidCost += Math.Min(paidResource.Amount, costResource.amount);
                }
            }

            return totalCost > 0 ? paidCost / totalCost : 1f;
        }
    }
}