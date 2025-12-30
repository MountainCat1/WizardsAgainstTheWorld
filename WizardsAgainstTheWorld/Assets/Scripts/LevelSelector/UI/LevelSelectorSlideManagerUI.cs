using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace UI
{
    public interface ILevelSelectorSlideManagerUI
    {
        event Action<LevelSelectorSlideManagerUI.LevelSelectorUIPanel?> Changed;
        void Toggle(LevelSelectorSlideManagerUI.LevelSelectorUIPanel panel);
        void Show(LevelSelectorSlideManagerUI.LevelSelectorUIPanel panel);
        void ClearPanels();
        LevelSelectorSlideManagerUI.LevelSelectorUIPanel? CurrentPanel { get; }
    }

    public class LevelSelectorSlideManagerUI : MonoBehaviour, ILevelSelectorSlideManagerUI
    {
        public event Action<LevelSelectorUIPanel?> Changed;
        
        [SerializeField] private UIManager uiManager;

        [SerializeField] private UISlide inventoryPanel;
        [SerializeField] private UISlide shopPanel;
        [SerializeField] private UISlide crewPanel;
        [SerializeField] private UISlide travelPanel;
        [SerializeField] private UISlide upgradePanel;
        [SerializeField] private UISlide shipPanel;

        private Dictionary<LevelSelectorUIPanel, UISlide> _panels;

        public LevelSelectorUIPanel? CurrentPanel { get; private set; }

        private void Start()
        {
            _panels = new Dictionary<LevelSelectorUIPanel, UISlide>
            {
                { LevelSelectorUIPanel.Inventory, inventoryPanel },
                { LevelSelectorUIPanel.Shop, shopPanel },
                { LevelSelectorUIPanel.Travel, travelPanel },
                { LevelSelectorUIPanel.Upgrade, upgradePanel },
                { LevelSelectorUIPanel.ShipInventory, shipPanel }
            };

            if (_panels.Any(x => x.Value == null))
                GameLogger.LogWarning(
                    $"Some panels are not assigned. {string.Join(", ", _panels.Where(x => x.Value == null).Select(x => x.Key))}"
                );
            
            uiManager.WentBack += OnWentBack;
        }

        private void OnWentBack(IClosableUI obj)
        {
            if (ReferenceEquals(obj, _panels.GetValueOrDefault(CurrentPanel ?? default)))
            {
                CurrentPanel = null;
                Changed?.Invoke(CurrentPanel);
            }
        }

        public enum LevelSelectorUIPanel
        {
            Inventory,
            Shop,
            Travel,
            Upgrade,
            ShipInventory
        }
        
        public void Toggle(LevelSelectorUIPanel panel)
        {
            var panelObject = _panels.FirstOrDefault(x => x.Key == panel).Value
                              ?? throw new KeyNotFoundException(
                                  $"Panel {panel} not found in LevelSelectorSlideManagerUI."
                              );

            if (ReferenceEquals(uiManager.CurrentPanel, panelObject))
            {
                uiManager.Hide(panelObject);
                CurrentPanel = null;
            }
            else
            {
                uiManager.ShowForced(panelObject);
                CurrentPanel = panel;
            }
            
            Changed?.Invoke(CurrentPanel);
        }

        public void Show(LevelSelectorUIPanel panel)
        {
            var panelObject = _panels.FirstOrDefault(x => x.Key == panel).Value
                              ?? throw new KeyNotFoundException(
                                  $"Panel {panel} not found in LevelSelectorSlideManagerUI."
                              );

            uiManager.ShowForced(panelObject);
            CurrentPanel = panel;
            Changed?.Invoke(CurrentPanel);
        }

        public void ClearPanels()
        {
            foreach (var (_, uiElement) in _panels)
            {
                uiManager.Hide(uiElement);
            }
            CurrentPanel = null;
            Changed?.Invoke(CurrentPanel);
        }
    }
}