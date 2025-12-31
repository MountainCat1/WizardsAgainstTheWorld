using Building.Managers;
using UnityEngine;
using Zenject;

namespace Building.UI
{
    public class BuilderUI : MonoBehaviour
    {
        [Inject] private IBuilderManager _builderManager;
        [Inject] private DiContainer _container;
        [Inject] private IInputMapper _inputMapper;
            
        [SerializeField] private BuildingPrefabEntryUI buildingPrefabEntryUI;
        [SerializeField] private Transform buildingPrefabListParent;
        
        private BuildingPrefab _selectedBuildingDefinition;
        
        private void Start()
        {
            foreach (var buildingPrefab in _builderManager.BuildingPrefabs)
            {
                _container.InstantiatePrefabForComponent<BuildingPrefabEntryUI>(
                    buildingPrefabEntryUI,
                    buildingPrefabListParent)
                    .Initialize(buildingPrefab, SelectBuilding);
            }

            _inputMapper.OnWorldPressed1 += TryBuild;
        }

        private void TryBuild(Vector2 position)
        {
            if (_selectedBuildingDefinition == null)
            {
                Debug.Log("No building selectedto build."); 
                return;
            }
            
            var gridPosition = GridPosition.FromWorldPosition(position);
            
            if(_builderManager.CanPlaceBuilding(
                gridPosition,
                _selectedBuildingDefinition.Footprint,
                out var occupiedCells))
            {
                var buildingView = _container.InstantiatePrefabForComponent<BuildingView>(
                    _selectedBuildingDefinition);
                
                _builderManager.PlaceBuilding(buildingView, gridPosition);
                Debug.Log($"Built {_selectedBuildingDefinition.Name} at {position}");
                
            }
            else
            {
                Debug.Log($"Cannot build {_selectedBuildingDefinition.Name} at {position}");
            }

            Debug.Log($"Trying to build {_selectedBuildingDefinition.Name} at {position}");
            // Building placement logic would go here
        }

        private void SelectBuilding(BuildingPrefab definition)
        {
            _selectedBuildingDefinition = definition;
            Debug.Log($"Selected building: {_selectedBuildingDefinition.Name}");
        }
    }
}