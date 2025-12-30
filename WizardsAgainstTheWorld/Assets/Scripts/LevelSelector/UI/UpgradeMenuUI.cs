using LevelSelector.Managers;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Utilities.LocalizationHelper;

namespace UI
{
    [RequireComponent(typeof(UISlide))]
    public class UpgradeMenuUI : MonoBehaviour
    {
        [Inject] private IItemManager _itemManager;
        [Inject] private IItemDescriptionManager _itemDescriptionManager;
        [Inject] private ICrewManager _crewManager;
        [Inject] private IUpgradeManager _upgradeManager;
        [Inject] private DiContainer _diContainer;
        [Inject] private ISoundPlayer _soundPlayer;
        [Inject] private IFloatingTextServiceUI _floatingTextServiceUI;

        [SerializeField] private TextMeshProUGUI selectedItemName;
        [SerializeField] private TextMeshProUGUI selectedItemDescription;
        [SerializeField] private Image selectedItemIcon;
        [SerializeField] private Sprite noItemIcon;

        [SerializeField] private Transform playerInventoryContainer;
        [SerializeField] private ItemEntryUI itemEntryUIPrefab;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button reforgeButton;
        [SerializeField] private Button scrapButton;

        [SerializeField] private TextMeshProUGUI scrapValueText;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private TextMeshProUGUI reforgeCostText;

        [SerializeField] private AudioClip badUpgradeSound;
        [SerializeField] private AudioClip goodUpgradeSound;
        [SerializeField] private AudioClip normalUpgradeSound;

        [SerializeField] private Transform floatingTextParent;

        private ItemData _selectedItem;

        private UISlide _uiSlide;

        private void Start()
        {
            _crewManager.Changed += UpdateUI;
            if (_crewManager.Inventory is not null)
                UpdateUI();

            upgradeButton.onClick.AddListener(UpgradeItem);
            reforgeButton.onClick.AddListener(ReforgeItem);
            scrapButton.onClick.AddListener(ScrapItem);

            _uiSlide = GetComponent<UISlide>();
            _uiSlide.Showed += UpdateUI;
        }

        private void ScrapItem()
        {
            _upgradeManager.ScrapItem(_selectedItem);

            UpdateUI();
        }

        private void ReforgeItem()
        {
            var reforgeResult = _upgradeManager.BuyReforgeItem(_selectedItem);

            switch (reforgeResult)
            {
                case UpgradeResult.Bad:
                    _soundPlayer.PlaySoundGlobal(badUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.ReforgeFailure")}", floatingTextParent.transform.position, Color.red, 1.5f);
                    ;
                    break;
                case UpgradeResult.Good:
                    _soundPlayer.PlaySoundGlobal(goodUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.ReforgeSuccess")}", floatingTextParent.transform.position, Color.green,
                        1.5f);
                    break;
                case UpgradeResult.Normal:
                    _soundPlayer.PlaySoundGlobal(normalUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.ReforgeSuccess")}", floatingTextParent.transform.position, Color.yellow,
                        1.5f);
                    break;
            }

            UpdateUI();
        }

        private void UpgradeItem()
        {
            var upgradeResult = _upgradeManager.PlayerBuyUpgrade(_selectedItem);

            switch (upgradeResult)
            {
                case UpgradeResult.Bad:
                    _soundPlayer.PlaySoundGlobal(badUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.UpgradeFailure")}", floatingTextParent.transform.position, Color.red, 1.5f);
                    ;
                    break;
                case UpgradeResult.Good:
                    _soundPlayer.PlaySoundGlobal(goodUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.UpgradeSuccess")}", floatingTextParent.transform.position, Color.green,
                        1.5f);
                    break;
                case UpgradeResult.Normal:
                    _soundPlayer.PlaySoundGlobal(normalUpgradeSound, SoundType.UI);
                    _floatingTextServiceUI.Show($"{L("UI.Upgrader.UpgradeSuccess")}", floatingTextParent.transform.position, Color.yellow,
                        1.5f);
                    break;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_crewManager.Inventory.ContainsItem(_selectedItem) == false)
                SelectItem(null);
            else
                SelectItem(_selectedItem);

            foreach (Transform child in playerInventoryContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in _crewManager.Inventory.Items)
            {
                var itemEntry = _diContainer
                    .InstantiatePrefab(itemEntryUIPrefab, playerInventoryContainer)
                    .GetComponent<ItemEntryUI>();

                itemEntry.Set(item, SelectItem);
            }
        }

        private void SelectItem(ItemData itemData)
        {
            if (itemData == null)
            {
                selectedItemName.text = string.Empty;
                selectedItemDescription.text = string.Empty;
                if (noItemIcon)
                {
                    selectedItemIcon.sprite = noItemIcon;
                }
                else
                {
                    selectedItemIcon.enabled = false;
                }

                _selectedItem = null;

                upgradeCostText.text = "0";
                scrapValueText.text = "0";

                reforgeButton.interactable = false;
                upgradeButton.interactable = false;
                scrapButton.interactable = false;

                return;
            }

            selectedItemName.text = itemData.Prefab.Name;
            selectedItemDescription.text = _itemDescriptionManager.GetDescription(itemData);
            selectedItemIcon.enabled = true;
            selectedItemIcon.sprite = itemData.Prefab.Icon;
            _selectedItem = itemData;

            var upgradeCost = _upgradeManager.GetUpgradeCost(_selectedItem);
            var reforgeCost = _upgradeManager.GetReforgeCost(_selectedItem);
            var scrapValue = _upgradeManager.GetScrapValue(_selectedItem);
            
            upgradeCostText.text = $"{L("UI.Upgrader.UpgradeCost", upgradeCost)}";
            scrapValueText.text = $"{L("UI.Upgrader.ScrapValue", scrapValue)}";
            reforgeCostText.text = $"{L("UI.Upgrader.ReforgeCost", reforgeCost)}";

            reforgeButton.interactable = _upgradeManager.CanReforge(_selectedItem);
            upgradeButton.interactable = _upgradeManager.CanUpgrade(_selectedItem);
            scrapButton.interactable = _upgradeManager.CanScrap(_selectedItem);
        }
    }
}