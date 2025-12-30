using System;
using System.Linq;
using Managers;
using Managers.LevelSelector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class LevelEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI customMarkerText;
        [SerializeField] private GameObject customMarkerGo;

        [SerializeField] private GameObject presenceMarker;
        [SerializeField] private GameObject shopMarker;
        [SerializeField] private GameObject eventMarker;
        [SerializeField] private GameObject exitMarker;
        [SerializeField] private GameObject blockadeMarker;
        [SerializeField] private Image icon;
        
        [SerializeField] private GameObject showOnHovered;
        [SerializeField] private GameObject showOnSelected;

        [Header("Colors")]
        [SerializeField] private Color eventColor;
        [SerializeField] private Color shopColor;
        [SerializeField] private Color bossColor;

        [Inject] private ILevelSelectorUI _levelSelectorUI;
        [Inject] private IRegionManager _regionManager;
        [Inject] private ICrewManager _crewManager;

        private LocationData _location;
        private Action<LocationData> _selectLevel;

        public LocationData Location => _location;

        public void Initialize(LocationData location, Action<LocationData> selectLevel, int distanceToCurrent)
        {
            _location = location;
            _selectLevel = selectLevel;

            showOnHovered?.SetActive(false);

            UpdateTextAndStyle(location, distanceToCurrent);
            UpdateCustomMarker(location);
            UpdatePresenceAndDistanceStyle(distanceToCurrent);
            UpdateMarkers(location, distanceToCurrent);
            UpdateColorIfSalvaged(location);
            
            showOnSelected?.SetActive(false);
            _levelSelectorUI.LocationSelected += OnLevelSelectorUIOnLocationSelected;
        }

        private void OnDestroy()
        {
            _levelSelectorUI.LocationSelected -= OnLevelSelectorUIOnLocationSelected;
            _selectLevel = null;
        }

        private void OnLevelSelectorUIOnLocationSelected(LocationData location)
        {
            if (location.Id == _location.Id)
            {
                showOnSelected?.SetActive(true);
            }
            else
            {
                showOnSelected?.SetActive(false);
            }
        }

        private void UpdateTextAndStyle(LocationData location, int distance)
        {
            levelNameText.text = $"{location.Name} ({distance})";

            if (location.Type == LocationType.BossNode)
            {
                levelNameText.color = Color.red;
                icon.color = bossColor;
            }
        }

        private void UpdateCustomMarker(LocationData location)
        {
            var marker = location.Features.LastOrDefault(f => f.Marker != null);
            if (marker != null)
            {
                customMarkerText.text = marker.Marker;
                customMarkerGo.SetActive(true);
            }
            else
            {
                customMarkerGo.SetActive(false);
            }
        }

        private void UpdatePresenceAndDistanceStyle(int distance)
        {
            presenceMarker.SetActive(distance == 0);

            levelNameText.fontStyle = distance switch
            {
                0 => FontStyles.Bold | FontStyles.Underline | FontStyles.UpperCase,
                1 => FontStyles.Bold,
                _ => FontStyles.Normal
            };

            if (distance > 1)
            {
                var button = GetComponentInChildren<Button>();
                if (button?.targetGraphic != null)
                    button.targetGraphic.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        private void UpdateMarkers(LocationData location, int distance)
        {
            var currentLocation = _regionManager.Region.Locations.First(x => x.Id == _crewManager.CurrentLocationId);
            var isBlockaded = currentLocation.Features.Any(f => f.BlockExit) && !currentLocation.Salvaged;

            blockadeMarker.SetActive(isBlockaded && distance == 1);
            shopMarker.SetActive(location.Type == LocationType.ShopNode);
            exitMarker.SetActive(location.Type == LocationType.EndNode);

            if (location.Type == LocationType.ShopNode)
                icon.color = shopColor;

            eventMarker.SetActive(location.Type == LocationType.EventNode && !location.Visited);

            if (location.Type == LocationType.EventNode && !location.Visited)
                icon.color = eventColor;
        }

        private void UpdateColorIfSalvaged(LocationData location)
        {
            if (location.Salvaged && location.Type != LocationType.ShopNode)
            {
                levelNameText.color = Color.gray;
                icon.color = Color.gray;
            }
        }

        public void SelectLevel()
        {
            _selectLevel?.Invoke(_location);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            showOnHovered?.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            showOnHovered?.SetActive(false);
        }
    }
}
