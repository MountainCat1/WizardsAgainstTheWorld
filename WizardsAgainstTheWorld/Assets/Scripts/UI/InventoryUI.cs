using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI
{
    public class InventoryUI : MonoBehaviour, IPointerDownHandler
    {
        [Inject] private IInputManager _inputManager;
        [Inject] private DiContainer _diContainer;

        [SerializeField] private InventoryEntryUI inventoryEntryUIPrefab;
        [SerializeField] private Transform entriesParent;
        [SerializeField] private TextMeshProUGUI creatureNameText;

        private Creature _creature;
        public bool IsToggled { get; private set; }
        public Creature Creature => _creature;

        public void Toggle(bool? toggle = null)
        {
            IsToggled = toggle ?? !IsToggled;
            UpdateInventory();
        }

        private void SetVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetCreature(Creature creature)
        {
            if (_creature != null)
            {
                _creature.Inventory.Changed -= UpdateInventory;
            }

            _creature = creature;
            
            if (_creature == null)
            {
                SetVisibility(false);
                return;
            }

            _creature.Inventory.Changed += UpdateInventory;
            UpdateInventory();
        }

        private void UpdateInventory()
        {
            SetVisibility(IsToggled && _creature != null);
            
            foreach (Transform child in entriesParent)
            {
                Destroy(child.gameObject);
            }

            if (_creature == null) return;

            creatureNameText.text = _creature.name;

            foreach (var item in _creature.Inventory.GetItemsOrdererByEquipped())
            {
                var entry = _diContainer.InstantiatePrefab(inventoryEntryUIPrefab, entriesParent);
                entry.GetComponent<InventoryEntryUI>().SetItem(item, _creature);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }
    }
}

