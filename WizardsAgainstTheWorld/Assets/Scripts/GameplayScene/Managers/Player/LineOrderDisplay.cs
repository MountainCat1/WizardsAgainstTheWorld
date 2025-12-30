using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class LineOrderDisplay : MonoBehaviour
    {
        [SerializeField] private LineOrderMarker markerPrefab;
        [SerializeField] private PlayerController playerController;
        
        [Inject] private IDynamicPoolingManager _dynamicPoolingManager;
        private IPoolAccess<LineOrderMarker> _markerPool;


        private void Start()
        {
            playerController.LineOrderPreviewChanged += OnLineOrderPreviewChanged;

            _markerPool = _dynamicPoolingManager.CreatePool<LineOrderMarker>();
        }

        private void OnLineOrderPreviewChanged(ICollection<Vector2> linePositions)
        {
            _markerPool.Clear();

            foreach (var pos in linePositions)
            {
                _markerPool.SpawnObject(markerPrefab, pos, 0);
            }
        }
    }
}