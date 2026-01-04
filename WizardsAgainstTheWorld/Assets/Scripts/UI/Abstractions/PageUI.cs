using System;
using UnityEngine;

namespace UI.Abstractions
{
    public class PageUI : MonoBehaviour, IClosableUI, IUIInteraction
    {
        public event Action OnShow;
        public event Action OnHide;
        
        [field: SerializeField] public bool BlocksInput { get; private set; }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            OnShow?.Invoke();
        }

        public bool IsOpen => gameObject.activeSelf;
        
        public void Enter()
        {
            Show();
        }

        public void Exit()
        {
            Hide();
        }
    }
}