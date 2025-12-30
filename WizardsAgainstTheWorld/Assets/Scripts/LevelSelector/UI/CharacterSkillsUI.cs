using System.Collections.Generic;
using System.Linq;
using LevelSelector.Managers;
using Managers;
using Skills;
using TMPro;
using UnityEngine;
using Zenject;

public class CharacterSkillsUI : MonoBehaviour
{
    // Dependencies
    [Inject] private ICrewManager _crewManager;
    [Inject] private ISkillManager _skillManager;
    [SerializeField] private CrewManagerUI crewManagerUI;

    // UI Elements
    [SerializeField] private TextMeshProUGUI pointsAvailableText;
    [SerializeField] private Transform skillsContainer;

    // Prefabs
    [SerializeField] private SkillEntryUI skillEntryUIPrefab;
    [SerializeField] private GameObject breakPrefab;

    [Inject] private DiContainer _diContainer;

    private void Start()
    {
        _skillManager.SkillsChanged += UpdateUI;
        crewManagerUI.SelectedACreature += SetSelectedCreature;
    }

    private void SetSelectedCreature(CreatureData creature)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var currentCharacter = crewManagerUI.SelectedCreature;
        if (currentCharacter == null)
            return;

        pointsAvailableText.text = $"Points Available: {currentCharacter.Level.PointsToUse}";

        foreach (Transform child in skillsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var skill in currentCharacter.Level.Skills.GroupBy(x => x.SkillID))
        {
            var skillDescriptors = skill
                .Select(x => _skillManager.GetSkillDescriptor(x.SkillID))
                .ToList();

            var skillEntry =
                _diContainer.InstantiatePrefabForComponent<SkillEntryUI>(skillEntryUIPrefab, skillsContainer);

            skillEntry.Set(
                skills: skillDescriptors,
                skillData: skill.First(),
                count: skill.Count(),
                enable: false,
                unlocked: true
            );
        }

        Instantiate(breakPrefab, skillsContainer);

        foreach (var skill in currentCharacter.Level.AvailableSkills)
        {
            var skillDescriptor = _skillManager.GetSkillDescriptor(skill.SkillID);

            var skillEntry =
                _diContainer.InstantiatePrefabForComponent<SkillEntryUI>(skillEntryUIPrefab, skillsContainer);

            skillEntry.Set(
                skills: new List<SkillDescriptor>() { skillDescriptor },
                skillData: skill,
                onClickCallback: BuySkill,
                enable: currentCharacter.Level.PointsToUse > 0,
                unlocked: false
            );
        }
    }

    private void BuySkill(SkillData skill)
    {
        var creature = crewManagerUI.SelectedCreature;
        if (creature == null)
            return;

        _skillManager.AddSkillToCreature(creature, skill);
    }
}