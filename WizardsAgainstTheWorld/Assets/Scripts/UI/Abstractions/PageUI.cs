using System;
using UnityEngine;

namespace UI.Abstractions
{
    public class PageUI : MonoBehaviour, IClosableUI
    {
        public event Action OnShow;
        public event Action OnHide;
        
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
    }
}