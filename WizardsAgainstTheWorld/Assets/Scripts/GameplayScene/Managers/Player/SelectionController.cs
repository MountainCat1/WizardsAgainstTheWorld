using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class SelectionController : MonoBehaviour
    {
        [Inject] private IInputManager _inputManager;
        [Inject] private IInputMapper _inputMapper;
        [Inject] private ISelectionManager _selectionManager;
        [Inject] private ICreatureManager _creatureManager;

        [SerializeField] private PlayerController playerController;

        private void Start()
        {
            _inputManager.SelectUnit1 += OnSelectUnit1;
            _inputManager.SelectUnit2 += OnSelectUnit2;
            _inputManager.SelectUnit3 += OnSelectUnit3;
            _inputManager.SelectUnit4 += OnSelectUnit4;
            _inputManager.SelectUnit5 += OnSelectUnit5;


            _inputManager.SelectAllUnits += OnSelectAllUnits;
        }

        private void OnSelectAllUnits()
        {
            _selectionManager.SetSelection(_creatureManager.PlayerCreatures
                .Where(c => c != null && c.gameObject.activeInHierarchy)
                .ToArray());
        }

        private void OnSelectUnit1()
        {
            SelectUnit(0);
        }

        private void OnSelectUnit2()
        {
            SelectUnit(1);
        }

        private void OnSelectUnit3()
        {
            SelectUnit(2);
        }

        private void OnSelectUnit4()
        {
            SelectUnit(3);
        }

        private void OnSelectUnit5()
        {
            SelectUnit(4);
        }

        private void OnDestroy()
        {
            _inputManager.SelectUnit1 -= OnSelectUnit1;
            _inputManager.SelectUnit2 -= OnSelectUnit2;
            _inputManager.SelectUnit3 -= OnSelectUnit3;
            _inputManager.SelectUnit4 -= OnSelectUnit4;
            _inputManager.SelectUnit5 -= OnSelectUnit5;

            _inputManager.SelectAllUnits -= OnSelectAllUnits;
        }

        private void SelectUnit(int unitIndex)
        {
            _selectionManager.SelectUnitByIndex(unitIndex);
        }
    }
}