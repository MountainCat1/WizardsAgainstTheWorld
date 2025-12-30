using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Zenject;

namespace UI
{
    public interface ILevelSelectorUI
    {
        event Action<LocationData> LocationSelected;
        void SelectLevel(LocationData location);

    }

    public class LevelSelectorUI : MonoBehaviour, ILevelSelectorUI
    {
        public event Action<LocationData> LocationSelected;

        [Inject] private IRegionGenerator _regionGenerator;
        [Inject] private IRegionManager _regionManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private ITravelManager _travelManager;
        [Inject] private DiContainer _diContainer;

        [SerializeField] private LevelEntryUI levelEntryPrefab;
        [SerializeField] private UILineRenderer linePrefab;
        [SerializeField] private UILineRenderer secondaryLinePrefab;

        [SerializeField] private Transform lineParent;

        [SerializeField] private TextMeshProUGUI selectedLevelNameText;
        [SerializeField] private TextMeshProUGUI selectedLevelDescriptionText;

        [SerializeField] private Transform levelsParent;

        private Vector2 _lastScreenSize;

        [field: NonSerialized] 
        [CanBeNull] public LocationData SelectedLocation { get; private set; }

        // Unity Methods
        private void Start()
        {
            _regionManager.RegionChanged += OnRegionGenerated;
            _crewManager.Changed += OnRegionGenerated;
            if (_regionManager.Region != null)
                OnRegionGenerated();
            
            _travelManager.Traveled += (region) =>
            {
                var startingLocation = region.Locations.FirstOrDefault(x => x.Type == LocationType.StartNode);
                SelectLevel(startingLocation);
            };
        }

        private void Update()
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (this._lastScreenSize != screenSize)
            {
                this._lastScreenSize = screenSize;
                OnRegionGenerated();
            }
        }

        // Callbacks
        public void SelectLevel(LocationData location)
        {
            //

            SelectedLocation = location;
            LocationSelected?.Invoke(location);
        }

        private void OnRegionGenerated()
        {
            foreach (Transform child in levelsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var level in _regionManager.Region.Locations)
            {
                var levelEntry =  _diContainer
                    .InstantiatePrefab(levelEntryPrefab, levelsParent)
                    .GetComponent<LevelEntryUI>();
                
                var distance = _regionManager.GetDistance(_crewManager.CurrentLocationId, level.Id);
                levelEntry.Initialize(level, SelectLevel, distance);

                var rectTransform = levelEntry.GetComponent<RectTransform>();

                // Extract X and Y percentages from the Vector2 key
                Vector2 positionPercent = level.Position;
                float xPercentage = positionPercent.x; // 0 = left, 1 = right
                float yPercentage = positionPercent.y; // 0 = bottom, 1 = top

                // Set anchors based on percentage position
                rectTransform.anchorMin = new Vector2(xPercentage, yPercentage);
                rectTransform.anchorMax = new Vector2(xPercentage, yPercentage);
                rectTransform.pivot = new Vector2(0.5f, 0.5f); // Center pivot

                // Reset position offset
                rectTransform.anchoredPosition = Vector2.zero;
            }


            foreach (Transform child in lineParent)
            {
                Destroy(child.gameObject);
            }

            var levelUIComponents = levelsParent.GetComponentsInChildren<LevelEntryUI>();

            var createdConnections = new List<(LocationData, LocationData)>();
            foreach (var level in _regionManager.Region.Locations
                         .OrderBy(x => _regionManager.GetDistance(_crewManager.CurrentLocationId, x.Id)))
            {
                UILineRenderer prefab = null;

                var distanceToCurrent = _regionManager.GetDistance(_crewManager.CurrentLocationId, level.Id);

                if (distanceToCurrent == 0)
                    prefab = linePrefab;
                if (distanceToCurrent == 1)
                    prefab = secondaryLinePrefab;
                if (distanceToCurrent > 1)
                    continue;

                var uiComponent = Array.Find(levelUIComponents, x => x.Location == level);
                foreach (var connectionLevel in level.Neighbours)
                {
                    if (createdConnections.Contains((connectionLevel, level)))
                        continue;

                    var connectionUIComponent = Array.Find(levelUIComponents, x => x.Location == connectionLevel);

                    var lineRendererInstance = Instantiate(prefab, lineParent);

                    lineRendererInstance.Points = new[]
                    {
                        (Vector2)lineParent.InverseTransformPoint(uiComponent.transform.position),
                        (Vector2)lineParent.InverseTransformPoint(connectionUIComponent.transform.position)
                    };


                    createdConnections.Add((level, connectionLevel));
                    
                    lineRendererInstance.transform.position = Vector3.zero;
                    lineRendererInstance.rectTransform.position = Vector3.zero;
                }
            }
        }
    }
}