using JetBrains.Annotations;
using Managers;
using UnityEngine;
using Zenject;

namespace UI
{
    public interface IInventoryDisplayUI
    {
        void ShowInventory(Creature creature);
        [CanBeNull] Creature Creature { get; }
    }


    public class InventoryDisplayUI : MonoBehaviour, IInventoryDisplayUI
    {
        [Inject] private ISelectionDisplayUI _selectionDisplayUI;

        [Inject] private IInputManager _inputManager;
        [Inject] private ISelectionManager _selectionManager;
        [Inject] private ISelectionInspectionManager _selectionInspectionManager;
        [Inject] private DiContainer _container;

        [SerializeField] private Transform popupParent;
        [SerializeField] private InventoryUI instantiatedInventoryUI;

        public Creature Creature => instantiatedInventoryUI.Creature;

        private void Start()
        {
            _inputManager.UI.ShowInventory += ToggleInventory;
            _selectionDisplayUI.CreatureClicked += ShowInventory;
            _selectionInspectionManager.SelectionInspectChanged += UpdateSelectedCreature;

            instantiatedInventoryUI.Toggle(false);
        }

        private void OnDestroy()
        {
            _inputManager.UI.ShowInventory -= ToggleInventory;
            _selectionDisplayUI.CreatureClicked -= ShowInventory;
            _selectionInspectionManager.SelectionInspectChanged -= UpdateSelectedCreature;
        }

        private void ToggleInventory()
        {
            instantiatedInventoryUI.Toggle();
        }

        public void ShowInventory(Creature creature)
        {
            instantiatedInventoryUI.SetCreature(creature);
            instantiatedInventoryUI.Toggle(true);
        }

        private void UpdateSelectedCreature()
        {
            instantiatedInventoryUI.SetCreature(_selectionInspectionManager.SelectedInspectedCreature);
        }
    }
}