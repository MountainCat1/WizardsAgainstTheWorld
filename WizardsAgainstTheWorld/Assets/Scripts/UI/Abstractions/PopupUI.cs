using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace UI.Abstractions
{
    public class PopupUI : MonoBehaviour
    {
        [Inject] private IInputManager _inputManager;
  
        public bool IsVisible { get; private set; }
            
        private RectTransform _rectTransform;
        private Canvas _canvas;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }


        private void Start()
        {
            _inputManager.Cancel += OnCancel;

            OnShow();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Optional: Bring the panel to the front when clicked
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null) return;

            // Move the panel based on drag delta
            Vector2 moveDelta = eventData.delta / _canvas.scaleFactor;
            _rectTransform.anchoredPosition += moveDelta;

            GameLogger.Log("xD");
        }

        private void OnDestroy()
        {
            _inputManager.Cancel -= OnCancel;

            OnHide();
        }

        public void Show(bool show)
        {
            if (show)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            IsVisible = true;
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            IsVisible = false;
            gameObject.SetActive(false);
            OnHide();
        }

        private void OnCancel(InputAction.CallbackContext ctx)
        {
            Hide();
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnShow()
        {
        }
    }
}