using Building.Managers;
using UI;
using UnityEngine;
using Zenject;

namespace Building.UI
{
    public class BuilderUI : MonoBehaviour, IUIInteraction
    {
        [Inject] private IBuilderManager _builderManager;
        [Inject] private IInputMapper _inputMapper;
        [Inject] private IUIInteractionStack _uiInteractionStack;
        [Inject] private DiContainer _container;
        [Inject] private GridSystem _gridSystem;

        [SerializeField] private BuildingPrefabEntryUI buildingPrefabEntryUI;
        [SerializeField] private Transform buildingPrefabListParent;
        [SerializeField] private BuildingPreview buildingPreviewPrefab;

        private BuildingPrefab _selectedBuildingDefinition;
        private BuildingPreview _buildingPreviewInstance;

        private void Start()
        {
            foreach (var buildingPrefab in _builderManager.BuildingPrefabs)
            {
                _container.InstantiatePrefabForComponent<BuildingPrefabEntryUI>(
                        buildingPrefabEntryUI,
                        buildingPrefabListParent
                    )
                    .Initialize(buildingPrefab, SelectBuilding);
            }

            _inputMapper.OnWorldPressed1 += TryBuild;

            _buildingPreviewInstance = Instantiate(buildingPreviewPrefab, transform);
            _buildingPreviewInstance.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_selectedBuildingDefinition is null)
            {
                _buildingPreviewInstance.gameObject.SetActive(false);
                return;
            }
            else
            {
                _buildingPreviewInstance.gameObject.SetActive(true);
                
                var mouseWorldPos = _inputMapper.GetMouseWorldPosition();
                var anchorGridPos = GridUtilities.GetAnchorFromWorldPosition(
                    _gridSystem,
                    mouseWorldPos,
                    _selectedBuildingDefinition.Footprint
                );
                
                var canBuild = _builderManager.CanPlaceBuilding(
                    _selectedBuildingDefinition.Footprint,
                    anchorGridPos,
                    out var gridCells
                );

                _buildingPreviewInstance.Initialize(
                    canBuild,
                    _selectedBuildingDefinition.GetComponentInChildren<SpriteRenderer>().sprite,
                    _selectedBuildingDefinition.Footprint,
                    _gridSystem.GetCenterFromCells(gridCells)
                );
            }
        }

        private void TryBuild(Vector2 position)
        {
            if (_selectedBuildingDefinition == null)
            {
                Debug.Log("No building selectedto build.");
                return;
            }
            
            var mouseWorldPos = _inputMapper.GetMouseWorldPosition();
            var anchorGridPos = GridUtilities.GetAnchorFromWorldPosition(
                _gridSystem,
                mouseWorldPos,
                _selectedBuildingDefinition.Footprint
            );

            if (_builderManager.CanPlaceBuilding(
                    _selectedBuildingDefinition.Footprint,
                    anchorGridPos, out _
                ))
            {

                _builderManager.ConstructBuilding(
                    _selectedBuildingDefinition.View,
                    _selectedBuildingDefinition.Footprint,
                    anchorGridPos
                );
                
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
            Debug.Log($"Selected building: {_selectedBuildingDefinition?.Name}");
                            
            _uiInteractionStack.Push(this);
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            SelectBuilding(null);
        }

        public bool BlocksInput => true;
    }
}