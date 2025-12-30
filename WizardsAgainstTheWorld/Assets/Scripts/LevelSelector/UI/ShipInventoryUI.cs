using Data;
using Items.ItemInteractions;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Zenject;

namespace UI
{
    [RequireComponent(typeof(UISlide))]
    public class ShipInventoryUI : MonoBehaviour
    {
        [Inject] private ILevelSelectorSlideManagerUI _levelSelectorSlideManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private IDataManager _dataManager;
        [Inject] private DiContainer _diContainer;

        [SerializeField] private ItemEntryUI itemEntryUIPrefab;
        [SerializeField] private Button itemInteractionButtonPrefab;

        [SerializeField] private Transform crewInventoryContainer;
        [SerializeField] private Transform itemInteractionContainer;

        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemNameDescription;

        [SerializeField] private CrewManagerUI crewManagerUI;

        private ItemData _selectedItem;

        private void Start()
        {
            _crewManager.Changed += UpdateUI;
            
            GetComponent<UISlide>().Showed += UpdateUI;

            UpdateUI();
        }

        private void SelectItem(ItemData item)
        {
            _selectedItem = item;

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (Transform child in crewInventoryContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in itemInteractionContainer)
            {
                if (child.gameObject == itemNameDescription.gameObject)
                    continue;

                Destroy(child.gameObject);
            }
            
            if(!_crewManager.Inventory.ContainsItem(_selectedItem))
                _selectedItem = null;

            if (_selectedItem?.Prefab != null)
            {
                itemNameText.text = _selectedItem.Prefab.NameKey.Localize();
                itemNameDescription.text = _selectedItem.Prefab.DescriptionKey.Localize();

                foreach (var interaction in _selectedItem.Prefab.GetInteractions(_diContainer))
                {
                    if (!interaction.PerCharacter)
                    {
                        var button = _diContainer.InstantiatePrefabForComponent<Button>(
                            prefab: itemInteractionButtonPrefab,
                            parentTransform: itemInteractionContainer
                        );
                        button.GetComponentInChildren<TextMeshProUGUI>().text = interaction.NameKey.Localize();
                        button.interactable = interaction.Enabled;
                        button.onClick.AddListener(() =>
                        {
                            interaction.Interact(new ItemInteractionContext()
                            {
                                DiContainer = _diContainer,
                                Item = _selectedItem.Prefab,
                            });

                            if (interaction.UseOnce && _selectedItem != null)
                            {
                                var deselectItem = _selectedItem.Count == 1;
                                
                                _crewManager.Inventory.RemoveItem(_selectedItem, 1);

                                if (deselectItem)
                                    _selectedItem = null;

                                UpdateUI();
                            }

                        });
                    }
                    else
                    {
                        foreach (var crew in _crewManager.Crew)
                        {
                            var button = _diContainer.InstantiatePrefabForComponent<Button>(
                                prefab: itemInteractionButtonPrefab,
                                parentTransform: itemInteractionContainer
                            );
                            button.GetComponentInChildren<TextMeshProUGUI>().text =
                                interaction.NameKey.Localize(crew.Name);
                            button.interactable = interaction.Enabled;
                            button.onClick.AddListener(() =>
                            {
                                interaction.Interact(new ItemInteractionContext()
                                {
                                    DiContainer = _diContainer,
                                    Item = _selectedItem.Prefab,
                                    CreatureData = crew
                                });

                                if (interaction.UseOnce)
                                {
                                    var deselectItem = _selectedItem.Count == 1;
                                
                                    _crewManager.Inventory.RemoveItem(_selectedItem, 1);
                                    
                                    if (deselectItem)
                                        _selectedItem = null;
                                    
                                    UpdateUI();
                                }
                            });
                        }
                    }
                }
            }
            else
            {
                itemNameText.text = "UI.ShipInventory.NoItemSelected".Localize();
                itemNameDescription.text = string.Empty;
            }

            foreach (var item in _crewManager.Inventory.Items)
            {
                var itemEntry = _diContainer.InstantiatePrefab(itemEntryUIPrefab, crewInventoryContainer);
                itemEntry.GetComponent<ItemEntryUI>().Set(item, SelectItem);
            }
        }

        private int GetTransferItemCount()
        {
            return Input.GetKey(KeyCode.LeftShift) ? 5 : 1;
        }
    }
}