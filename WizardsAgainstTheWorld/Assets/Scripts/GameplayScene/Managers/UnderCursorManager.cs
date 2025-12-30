using System;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Managers
{
    public interface IUnderCursorManager
    {
        event Action<IHoverable> HoveredObjectChanged;
        void SetLayerMask(LayerMask mask);
        Object CurrentHovered { get; }
    }

    public interface IHoverable
    {
    }

    public class UnderCursorManager : MonoBehaviour, IUnderCursorManager
    {
        public event Action<IHoverable> HoveredObjectChanged;
        public Object CurrentHovered => _currentHovered;

        private Object _currentHovered;
        [SerializeField] private LayerMask layerMask = Physics2D.DefaultRaycastLayers;

        public void SetLayerMask(LayerMask mask)
        {
            layerMask = mask;
        }

        private void Update()
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, layerMask);

            Object newHovered = hit.collider ? (Object)hit.collider.gameObject : null;

            if (_currentHovered != newHovered)
            {
                _currentHovered = newHovered;
                
                if(_currentHovered == null)
                {
                    HoveredObjectChanged?.Invoke(null);
                    return;
                }

                var hoverable = newHovered.GetComponent<IHoverable>();

                if (hoverable != null)
                {
                    HoveredObjectChanged?.Invoke(hoverable);
                }
                else
                {
                    HoveredObjectChanged?.Invoke(null);
                }
            }
        }
    }
}