using System;
using System.Collections.Generic;
using System.Linq;
using Items.PassiveItems;
using LevelSelector.Managers;
using Managers;
using Skills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utilities;
using Zenject;

public class SkillEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillCountText;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Button skillButton;
    [SerializeField] private TooltipTrigger tooltipTrigger;
    [SerializeField] private GameObject unclockedIndicator;

    [Inject] private ISkillManager _skillManager;
    [Inject] private IEffectDescriptionProvider _effectDescriptionProvider;

    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color buyableColor = Color.white;
    [SerializeField] private Color disableColor = Color.white;

    public void Set(
        ICollection<SkillDescriptor> skills,
        SkillData skillData, int count = 1,
        Action<SkillData> onClickCallback = null,
        bool enable = false,
        bool unlocked = false)
    {
        var representative = skills.FirstOrDefault();

        // Validate
        if (representative == null)
        {
            Debug.LogError("SkillEntryUI: Set - No skills provided.");
            return;
        }

        if (skills.Any(x => x == null))
        {
            Debug.LogError("SkillEntryUI: Set - One or more skills are null.");
        }

        if (skills.Any(x => x.Name != representative.Name))
        {
            Debug.LogError("SkillEntryUI: Set - Skills have different names.");
        }

        //

        var effects = skills.SelectMany(x => x.GetEffects()).ToList();
        skillNameText.text = representative.NameKey.Localize();
        skillDescriptionText.text = CreateLocalizedDescription(representative, effects);
        tooltipTrigger.text = CreateLocalizedDescription(representative, effects);
        skillCountText.text = count > 1 ? $"x{count}" : string.Empty;
        skillIcon.sprite = representative.Icon;

        if (onClickCallback != null)
        {
            skillButton.onClick.AddListener(() => onClickCallback(skillData));
        }

        skillButton.interactable = enable; // Disable button if unlocked

        var colors = skillButton.colors;

        if (unlocked)
        {
            colors.normalColor = unlockedColor;
            colors.disabledColor = unlockedColor; // Keep the same color when disabled
        }
        else if (enable)
        {
            colors.normalColor = buyableColor;
        }
        else
        {
            colors.normalColor = disableColor;
        }
        
        skillButton.colors = colors;

        unclockedIndicator.SetActive(unlocked);
    }

    private string CreateLocalizedDescription(SkillDescriptor skill, List<PassiveEffect> effects)
    {
        return $"{skill.NameKey.Localize()}\n-----\n" +
               $"{_effectDescriptionProvider.GetDescription(effects)}";
    }
}