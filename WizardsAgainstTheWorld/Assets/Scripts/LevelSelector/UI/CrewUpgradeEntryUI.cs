using CrewUpgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utilities;

namespace LevelSelector.UI
{
    public class CrewUpgradeEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TooltipTrigger descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text countText;

        private CrewUpgrade _crewUpgrade;

        public void Initialize(CrewUpgrade crewUpgrade)
        {
            _crewUpgrade = crewUpgrade;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_crewUpgrade == null) return;

            nameText.text = _crewUpgrade.NameKey.Localize();
            descriptionText.text = _crewUpgrade.DescriptionKey.Localize();
            iconImage.sprite = _crewUpgrade.Icon;
        }
    }
}