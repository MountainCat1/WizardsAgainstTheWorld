using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace UI
{
    public interface IUIManager
    {
        void Register(IClosableUI uiElement);
        void Unregister(IClosableUI uiElement);
        void Show(IClosableUI uiElement);
        void ShowForced(IClosableUI uiElement);
        void Hide(IClosableUI uiElement);
        void GoBack();
        [CanBeNull] IClosableUI CurrentPanel { get; }
        event Action<IClosableUI> WentBack;
        event Action<IClosableUI> Changed;
    }

    public class UIManager : MonoBehaviour, IUIManager
    {
        public event Action<IClosableUI> WentBack;
        public event Action<IClosableUI> Changed;

        [Inject] private IInputManager _inputManager;

        private readonly Stack<IClosableUI> _openPanels = new();

        public IClosableUI CurrentPanel => _openPanels.Count > 0 ? _openPanels.Peek() : null;

        [SerializeField] private bool notAllowFullClose = false;
        [SerializeField] private bool showOne = false;

        private void Start()
        {
            if (_inputManager == null)
            {
                Debug.LogError("InputManager is not injected into UIManager.");
                return;
            }

            _inputManager.UI.GoBack += OnGoBack;
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.UI.GoBack -= OnGoBack;
            }
        }

        public void Register(IClosableUI uiElement)
        {
            if (uiElement.IsOpen && CurrentPanel != uiElement)
                _openPanels.Push(uiElement);
        }

        public void Unregister(IClosableUI uiElement)
        {
            if (_openPanels.Contains(uiElement))
            {
                var tempStack = new Stack<IClosableUI>();
                while (_openPanels.Count > 0)
                {
                    var top = _openPanels.Pop();
                    if (top != uiElement)
                        tempStack.Push(top);
                    else
                        break;
                }

                while (tempStack.Count > 0)
                    _openPanels.Push(tempStack.Pop());
            }
        }

        public void Show(IClosableUI uiElement)
        {
            if (uiElement == null)
            {
                Debug.LogError("UIManager: Show - uiElement is null.");
                return;
            }

            if (uiElement.IsOpen)
            {
                Debug.LogWarning("UIManager: Show - uiElement is already open.");
                return;
            }

            if (showOne && CurrentPanel != null)
                CurrentPanel.Hide();

            uiElement.Show();
            Register(uiElement);
            Changed?.Invoke(CurrentPanel);
        }


        public void ShowForced(IClosableUI uiElement)
        {
            if (uiElement == null)
            {
                Debug.LogError("UIManager: ShowForced - uiElement is null.");
                return;
            }

            // Hide all currently open panels
            while (_openPanels.Count > 0)
            {
                var panel = _openPanels.Pop();
                if (panel.IsOpen && panel != uiElement)
                    panel.Hide();
            }
            
            if (uiElement.IsOpen)
            {
                Debug.LogWarning("UIManager: Show - uiElement is already open.");
                _openPanels.Push(uiElement); // We still want to keep it in the stack
                Changed?.Invoke(CurrentPanel);
                return;
            }

            uiElement.Show();
            _openPanels.Push(uiElement);
            Changed?.Invoke(CurrentPanel);
        }

        public void Hide(IClosableUI uiElement)
        {
            if (notAllowFullClose && _openPanels.Count <= 1)
            {
                Debug.LogWarning("UIManager: Hide - Not allowed to close the last panel.");
                return;
            }

            if (uiElement == null)
            {
                Debug.LogError("UIManager: Hide - uiElement is null.");
                return;
            }

            if (!uiElement.IsOpen)
            {
                Debug.LogWarning("UIManager: Hide - uiElement is not open.");
                return;
            }

            uiElement.Hide();
            Unregister(uiElement);
            Changed?.Invoke(CurrentPanel);
        }

        public void GoBack()
        {
            OnGoBack();
        }
        
        private void OnGoBack()
        {
            if (_openPanels.Count == 0)
            {
                GameLogger.Log("UIManager: GoBack - No panels to go back to.");
                WentBack?.Invoke(null);
                return;
            }

            if (notAllowFullClose && _openPanels.Count <= 1)
            {
                Debug.LogWarning("UIManager: Hide - Not allowed to close the last panel.");
                return;
            }

            var top = _openPanels.Pop();
            top.Hide();

            if (showOne && _openPanels.Count > 0)
                CurrentPanel.Show();

            WentBack?.Invoke(top);
        }

    }
}