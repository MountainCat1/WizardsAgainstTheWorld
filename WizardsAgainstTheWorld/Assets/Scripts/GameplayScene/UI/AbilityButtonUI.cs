using System;
using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AbilityButtonUI : MonoBehaviour
    {
        [SerializeField] private Image iconDisplay;
        
        private Ability _ability;
        private Action<Ability> _callback;
        
        public void Initialize(Ability ability, Sprite icon, Action<Ability> callback)
        {
            _ability = ability;
            iconDisplay.sprite = icon;
            _callback = callback;
        }

        public void UseAbility()
        {
            _callback?.Invoke(_ability);
        }
    }
}