using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DraggablePanel : MonoBehaviour, IDragHandler, IBeginDragHandler,
        IEndDragHandler
    {
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private Vector3 _dragOffset; // Offset between mouse and panel
        private Camera _eventCamera;

        void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                GameLogger.LogError("DraggablePanel must be a child of a Canvas.");
                enabled = false;
                return;
            }

            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                GameLogger.LogError("DraggablePanel must have a RectTransform.");
                enabled = false;
                return;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _eventCamera = eventData.pressEventCamera; // null in Overlay
            Vector3 globalMousePos;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    _eventCamera, // null is fine in Overlay
                    out globalMousePos))
            {
                _dragOffset = _rectTransform.position - globalMousePos;
                _rectTransform.position = globalMousePos + _dragOffset;
            }
        }



        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (_eventCamera == null) return;

                Vector3 globalMousePos;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        _rectTransform,
                        eventData.position,
                        _eventCamera,
                        out globalMousePos
                    ))
                {
                    // Set the panel's position based on the mouse position and the offset
                    _rectTransform.position = globalMousePos + _dragOffset;
                }
            }
            else
            {
                // For Overlay and World Space, use the delta directly
                _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _eventCamera = null;
        }
    }
}