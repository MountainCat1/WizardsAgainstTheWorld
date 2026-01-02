using System;
using System.Collections.Generic;
using GameplayScene.Managers;
using UnityEngine;
using Zenject;

namespace GameplayScene.UI
{
    public class ResourceDisplayUI : MonoBehaviour
    {
        [SerializeField] private ResourceDisplayEntryUI displayEntryUI;
        [SerializeField] private Transform entriesParent;

        [Inject] private IResourceManager _resourceManager;
        [Inject] private DiContainer _diContainer;
        
        private readonly Dictionary<GameResourceType, ResourceDisplayEntryUI> _displayEntries = new();
        
        private void Start()
        {
            _resourceManager.Changed += OnResourceChanged;

            foreach (var resourceType in _resourceManager.AvailableResources)
            {
                var uiEntry = _diContainer.InstantiatePrefabForComponent<ResourceDisplayEntryUI>(
                    displayEntryUI,
                    entriesParent
                );
                
                var resource = _resourceManager.GetResource(resourceType);
                
                uiEntry.Initialize(resource);
                
                _displayEntries.Add(resourceType, uiEntry);
            }
        }

        private void OnResourceChanged()
        {
            foreach (var (type, entry) in _displayEntries)
            {
                var resource = _resourceManager.GetResource(type);

                entry.Initialize(resource);
            }
        }
    }
}