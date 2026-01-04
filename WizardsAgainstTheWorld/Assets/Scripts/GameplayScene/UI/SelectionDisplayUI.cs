using System;
using System.Linq;
using Managers;
using UI;
using UnityEngine;
using Zenject;

public interface ISelectionDisplayUI
{
    event Action<Creature> CreatureClicked;
}

public class SelectionDisplayUI : MonoBehaviour, ISelectionDisplayUI
{
    public event Action<Creature> CreatureClicked;
    
    [SerializeField] private SelectionDisplayEntryUI entryPrefab;
    [SerializeField] private Transform parent;
    
    [Inject] private ISelectionManager _selectionManager;
    [Inject] private ISelectionInspectionManager _selectionInspectionManager;
    [Inject] private DiContainer _container;

    private void Start()
    {
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        ClearEntries();
        foreach (var creature in _selectionManager.SelectedCreatures.OrderBy(_selectionManager.GetCreatureIndex))
        {
            var index = _selectionManager.GetCreatureIndex(creature);
            var entry = _container.InstantiatePrefabForComponent<SelectionDisplayEntryUI>(entryPrefab, parent);
            entry.SetCreature(creature, OnClick, index);
        }
    }

    private void OnClick(Creature creature)
    {
        CreatureClicked?.Invoke(creature);

        _selectionInspectionManager.SetInspectCreature(creature);
    }

    private void ClearEntries()
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}