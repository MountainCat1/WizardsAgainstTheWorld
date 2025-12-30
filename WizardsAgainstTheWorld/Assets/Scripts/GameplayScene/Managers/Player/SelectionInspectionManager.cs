using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers
{
    public interface ISelectionInspectionManager
    {
        event Action SelectionInspectChanged;

        [CanBeNull] Creature SelectedInspectedCreature { get; }
        void SetInspectCreature(Creature creature);
    }

    public class SelectionInspectionManager : MonoBehaviour, ISelectionInspectionManager
    {
        public event Action SelectionInspectChanged;

        public Creature SelectedInspectedCreature => selectedInspectedCreature;

        [SerializeField, ReadOnlyInInspector] private Creature selectedInspectedCreature = null;

        [SerializeField] private SelectionMarker selectionMarkerPrefab = null;

        [Inject] private ISelectionManager _selectionManager;
        [Inject] private IInputManager _inputManager;
        [Inject] private ISpawnerManager _spawnerManager;
        private SelectionMarker _selectionMarkerInstance;

        private void Start()
        {
            _selectionManager.OnSelectionChanged += OnSelectionChanged;
            _inputManager.ChangeInspectedUnit += ChangeInspectedUnit;

            _selectionMarkerInstance = _spawnerManager.Spawn(selectionMarkerPrefab, Vector3.zero);
            _selectionMarkerInstance.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _selectionManager.OnSelectionChanged -= OnSelectionChanged;
            _inputManager.ChangeInspectedUnit -= ChangeInspectedUnit;
        }

        private void ChangeInspectedUnit()
        {
            // If there is no selected creature, do nothing
            if (!_selectionManager.SelectedCreatures.Any())
            {
                _selectionMarkerInstance.SetTarget(null);
                return;
            }

            // We allocate a new list here, which is not ideal, but 
            // it does not happen often, so it should be fine?? xdd
            // TODO: Optimize this, but its not a priority
            var selectedUnits = _selectionManager.SelectedCreatures.ToList();

            // We get the next unit as a new inspected unit
            var indexOfSelected = selectedUnits.IndexOf(selectedInspectedCreature);
            indexOfSelected++;
            if (indexOfSelected >= selectedUnits.Count)
            {
                indexOfSelected = 0;
            }

            SetInspectCreature(selectedUnits[indexOfSelected]);
        }

        private void OnSelectionChanged()
        {
            if (selectedInspectedCreature != null)
            {
                if (!_selectionManager.SelectedCreatures.Contains(selectedInspectedCreature))
                {
                    SetInspectCreature(null);
                }
            }

            if (selectedInspectedCreature == null && _selectionManager.SelectedCreatures.Any())
            {
                SetInspectCreature(_selectionManager.SelectedCreatures.First());
            }
        }


        public void SetInspectCreature(Creature creature)
        {
            if (creature is null && selectedInspectedCreature is null)
            {
                GameLogger.LogWarning("Creature is already null while trying to set inspection to null.");
                return;
            }

            if (creature is not null && !_selectionManager.SelectedCreatures.Contains(creature))
            {
                GameLogger.LogError($"Creature {creature} is not selected. Cannot inspect.");
                // return; // should it return here?
            }

            if (creature == selectedInspectedCreature)
            {
                GameLogger.LogWarning($"Creature {creature} is already selected for inspection.");
                return;
            }

            if (selectedInspectedCreature != null)
            {
                UnRegisterEvents(selectedInspectedCreature);
            }


            _selectionMarkerInstance.SetTarget(creature);
            selectedInspectedCreature = creature;

            if (selectedInspectedCreature != null)
            {
                RegisterEvents(selectedInspectedCreature);
            }

            SelectionInspectChanged?.SafeInvoke();
        }

        private void RegisterEvents(Creature creature)
        {
            if (SelectionInspectChanged is not null)
            {
                creature.Inventory.Changed += SelectionInspectChanged.Invoke;
            }
        }

        private void UnRegisterEvents(Creature creature)
        {
            if (SelectionInspectChanged is not null)
            {
                creature.Inventory.Changed -= SelectionInspectChanged.Invoke;
            }
        }
    }
}