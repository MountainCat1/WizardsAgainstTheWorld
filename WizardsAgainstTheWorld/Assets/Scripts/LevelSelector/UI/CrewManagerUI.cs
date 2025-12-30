using System;
using System.Linq;
using Data;
using LevelSelector.Managers;
using Managers;
using UI;
using UnityEngine;
using Zenject;

public class CrewManagerUI : MonoBehaviour
{
    public event Action<CreatureData> SelectedACreature;

    public CreatureData SelectedCreature => _selectedCreature;
    
    [Inject] private IDataManager _dataManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private ICrewManager _crewManager;
    [Inject] private ILevelSelectorSlideManagerUI _levelSelectorSlideManager;
    [Inject] private ISkillManager _skillManager;

    [SerializeField] private CrewEntryUI crewEntryUI;
    [SerializeField] private Transform crewListParent;
    [SerializeField] private CrewInventoryUI crewInventoryUI;

    private CreatureData _selectedCreature;
    
    private void Start()
    {
        _crewManager.Changed += RefreshList;
        _skillManager.SkillsChanged += RefreshList;
        
        if (_crewManager.Crew is not null)
            RefreshList();
    }

    public void ReRollCrew()
    {
        _crewManager.ReRollCrew();
    }

    private void RefreshList()
    {
        foreach (Transform child in crewListParent)
        {
            Destroy(child.gameObject);
        }

        var crew = _crewManager.Crew;

        foreach (var crewMember in crew.OrderBy(x => x.Name))
        {
            var crewMemberUI = _diContainer.InstantiatePrefab(crewEntryUI, crewListParent).GetComponent<CrewEntryUI>();
            crewMemberUI.Initialize(crewMember, OnSelect, OnToggle, IsSelected);
        }
    }

    private bool IsSelected(CreatureData creature)
    {
        return _selectedCreature == creature;
    }

    private void OnToggle(CreatureData creature, bool value)
    {
        _crewManager.ToggleCreature(creature, value);
    }
    
    private void OnSelect(CreatureData creature)
    {
        _selectedCreature = creature;
        
        SelectedACreature?.Invoke(_selectedCreature);
    }
}