using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Managers.Visual
{
    public class OffScreenIndicatorManager : MonoBehaviour
    {
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private IDynamicPoolingManager _dynamicPoolingManager;
        
        [SerializeField] private GameObject offScreenIndicatorPrefab;
        
        public RectTransform canvasRect;

        private readonly List<GameObject> _indicators = new();
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        void Update()
        {
            var units = _creatureManager.PlayerCreatures
                .Where(x => x != null) // TODO: optimize this, we shouldnt be checking for null every frame
                .Select(x => x.transform);
            
            var unitsOffScreen = units.Where(unit =>
            {
                Vector3 viewportPos = _mainCamera.WorldToViewportPoint(unit.position);
                return viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
            });
            
            // Create arrow if needed 
            while (_indicators.Count < unitsOffScreen.Count())
            {
                var indicator = Instantiate(offScreenIndicatorPrefab, canvasRect);
                _indicators.Add(indicator);
            }

            var arrowIndex = 0;
            foreach (var unit in units)
            {
                Vector3 viewportPos = _mainCamera.WorldToViewportPoint(unit.position);
                bool isOffScreen = viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;

                if (isOffScreen)
                {
                    // Project world position to screen space
                    Vector3 screenPos = _mainCamera.WorldToScreenPoint(unit.position);

                    // Invert for behind camera
                    if (screenPos.z < 0)
                        screenPos *= -1;

                    // Clamp to screen edges
                    screenPos.x = Mathf.Clamp(screenPos.x, 50, Screen.width - 50);
                    screenPos.y = Mathf.Clamp(screenPos.y, 50, Screen.height - 50);

                    // Create arrow
                    GameObject arrow = _indicators.ElementAt(arrowIndex++);
                    arrow.SetActive(true);
                    arrow.transform.position = screenPos;

                    // Rotate to point toward unit
                    Vector3 dir = (unit.position - _mainCamera.transform.position).normalized;
                    Vector3 toUnitScreen = _mainCamera.WorldToScreenPoint(unit.position) - new Vector3(Screen.width / 2, Screen.height / 2);
                    float angle = Mathf.Atan2(toUnitScreen.y, toUnitScreen.x) * Mathf.Rad2Deg;
                    arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
                }
            }
            
            // Disable unused arrows
            for (int i = arrowIndex; i < _indicators.Count; i++)
            {
                _indicators[i].SetActive(false);
            }
        }
    }
}