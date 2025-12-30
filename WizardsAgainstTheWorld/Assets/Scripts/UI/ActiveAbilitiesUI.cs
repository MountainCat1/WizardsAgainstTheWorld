using System.Linq;
using Items;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace UI
{
    public class ActiveAbilitiesUI : MonoBehaviour
    {
        [Inject] private ISelectionManager _selectionManager;
        [Inject] private ISelectionInspectionManager _selectionInspectionManager;
        [Inject] private DiContainer _diContainer;
        [Inject] private IInputManager _inputManager;
        [Inject] private IInputMapper _inputMapper;

        [SerializeField]
        private AbilityButtonUI abilityButtonUIPrefab;

        [SerializeField]
        private Transform buttonContainer;

        [SerializeField]
        private LayerMask groundLayer;

        [SerializeField]
        private Texture2D targetingCursor;

        [SerializeField]
        private Texture2D defaultCursor;

        // Removed: private Ability _abilityWaitingForTarget;

        void Start()
        {
            _selectionInspectionManager.SelectionInspectChanged += UpdateUI;
        }

        private void OnEnable()
        {
            _inputManager.Cancel += CancelAbilityTargeting;
        }

        private void OnDisable()
        {
            _inputManager.Cancel -= CancelAbilityTargeting;
            _inputMapper.CancelFollowUpClick(); // Ensure cancellation on disable
        }

        private void OnDestroy()
        {
            _selectionInspectionManager.SelectionInspectChanged -= UpdateUI;
        }

        private void CancelAbilityTargeting(InputAction.CallbackContext context)
        {
            _inputMapper.CancelFollowUpClick();
            GameLogger.Log("Ability use cancelled.");
            _selectionManager.AllowSelection(this);
        }

        private void UpdateUI()
        {
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
            
            // var activeItems = _selectionManager
            //     .SelectedCreatures
            //     .SelectMany(x => x.Inventory.Items)
            //     .Select(x => x as ActiveItemBehaviour)
            //     .Where(x => x != null);

            if(_selectionInspectionManager.SelectedInspectedCreature == null)
                return;
            
            var activeItems = _selectionInspectionManager
                .SelectedInspectedCreature.Inventory.Items
                .OfType<ActiveItemBehaviour>()
                .ToList();

            foreach (var activeItem in activeItems)
            {
                var button = _diContainer
                    .InstantiatePrefab(abilityButtonUIPrefab, buttonContainer)
                    .GetComponent<AbilityButtonUI>();
                button.Initialize(
                    activeItem.GetAbility(),
                    activeItem.Icon,
                    (ability) => ActivateAbility(ability)
                ); 
            }
        }

        private void ActivateAbility(Ability ability)
        {
            if(ability.Targetable == false)
            {
                UseAbilityAtLocation(ability, Vector3.zero);
                return;
            }
            
            _selectionManager.PreventSelection(this);
            GameLogger.Log($"Waiting for target location for ability: {ability.Identifier}");

            _inputMapper.WaitForFollowUpClick((worldPosition) =>
            {
                UseAbilityAtLocation(ability, worldPosition);
                _selectionManager.AllowSelection(this);
            }, targetingCursor);
        }

        private void UseAbilityAtLocation(Ability ability, Vector3 targetLocation)
        {
            // Find the first selected creature with the ability
            var creature = _selectionInspectionManager.SelectedInspectedCreature;

            if (creature == null)
            {
                GameLogger.LogError($"No creature with ability {ability.Identifier} selected.");
                return;
            }

            var item = creature.Inventory.Items.FirstOrDefault(
                    i =>
                        i is ActiveItemBehaviour activeItem
                        && activeItem.GetAbility().Identifier == ability.Identifier
                ) as ActiveItemBehaviour;

            if (item == null)
            {
                GameLogger.LogError(
                    $"No item with ability {ability.Identifier} found in creature's ({creature.name}) inventory."
                );
                return;
            }

            item.UseActiveAbility(new AbilityUseContext(creature, null, targetLocation));

            UpdateUI();
        }
    }
}
