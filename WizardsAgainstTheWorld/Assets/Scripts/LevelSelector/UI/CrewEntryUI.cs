using System;
using System.Linq;
using LevelSelector.Managers;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utilities;
using Zenject;

[RequireComponent(typeof(TooltipTrigger))]
public class CrewEntryUI : MonoBehaviour
{
    [Inject] private ISkillManager _skillManager;

    // [Inject] private IStatsDisplayService _statsDisplayService;
    [Inject] private IEffectDescriptionProvider _effectDescriptionProvider;
    [Inject] private ICrewManager _crewManager;
    [Inject] private ILevelSelectorSlideManagerUI _levelSelectorSlideManager;

    [SerializeField] private Image crewMemberImage;
    [SerializeField] private TextMeshProUGUI crewMemberName;
    [SerializeField] private Toggle selectedToggle;
    [SerializeField] private GameObject noWeaponWarning;
    [SerializeField] private GameObject unusedPointWarning;
    [SerializeField] private Image colorDisplay;
    [SerializeField] private TextMeshProUGUI levelDisplay;
    [SerializeField] private HighlightableUI highlightable;

    private CreatureData _crewMember;
    private Action<CreatureData> _onClick;
    private Action<CreatureData, bool> _onToggle;
    private Func<CreatureData, bool> _isSelected;

    private TooltipTrigger _tooltipTrigger;

    private void Awake()
    {
        _tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void Initialize(CreatureData crewMember,
        Action<CreatureData> onClick,
        Action<CreatureData, bool> onToggle,
        Func<CreatureData, bool> isSelected
    )
    {
        crewMemberName.text = crewMember.Name;
        _crewMember = crewMember;

        _onClick = onClick;
        _onToggle = onToggle;

        selectedToggle.isOn = crewMember.Selected;
        selectedToggle.onValueChanged.AddListener(OnToggle);

        levelDisplay.text = $"{"UI.CrewList.Level".Localize(_crewMember.Level.CurrentLevel)}";

        if (crewMember.Inventory.Items.Any(x => x.Type == ItemType.Weapon))
            noWeaponWarning.SetActive(false);
        else
            noWeaponWarning.SetActive(true);

        if (crewMember.Level.PointsToUse > 0)
            unusedPointWarning.SetActive(true);
        else
            unusedPointWarning.SetActive(false);

        colorDisplay.color = crewMember.Color.ToColor();

        _isSelected = isSelected;
        _levelSelectorSlideManager.Changed += OnSlideChanged;
        OnSlideChanged(_levelSelectorSlideManager.CurrentPanel);

        UpdateTooltip();
    }

    private void OnSlideChanged(LevelSelectorSlideManagerUI.LevelSelectorUIPanel? slide)
    {
        if (!slide.HasValue || LevelSelectorSlideManagerUI.LevelSelectorUIPanel.Inventory != slide.Value)
        {
            highlightable.Highlighted = false;
            return;
        }

        // if we went back it means we are NOT in the crew inventory view
        highlightable.Highlighted = _isSelected(_crewMember);
    }

    private void OnDestroy()
    {
        _levelSelectorSlideManager.Changed -= OnSlideChanged;
    }

    private void OnToggle(bool toggle)
    {
        _onToggle?.Invoke(_crewMember, toggle);
    }

    public void Select()
    {
        GameLogger.Log($"Selected {_crewMember.Name}");
        _onClick?.Invoke(_crewMember);
    }

    private void UpdateTooltip()
    {
        var text = $"{_crewMember.Name}\n-" +
                   $"----\n" +
                   $"{"UI.CrewList.Level".Localize(_crewMember.Level.CurrentLevel)}\n\n";

        var skill = _crewMember.Level.Skills
            .Select(x => _skillManager.GetSkillDescriptor(x.SkillID))
            .Where(x => x != null)
            .ToList();

        var rows = skill.SelectMany(x => x.GetEffects());

        var description = _effectDescriptionProvider.GetDescription(rows.ToList());

        // var formattedRows = rows.Select(x => new StatsDisplayRow(
        //     x.Key,
        //     null,
        //     x.Item3,
        //     x.StatsType
        // )).ToList();

        _tooltipTrigger.text += (text + description).Trim();
    }
}