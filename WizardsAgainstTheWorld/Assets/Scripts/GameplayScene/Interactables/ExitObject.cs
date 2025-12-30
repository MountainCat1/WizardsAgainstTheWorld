using System.Linq;
using Managers;
using UnityEngine;
using Zenject;

namespace Interactables
{
    public class ExitObject : InteractionBehavior
    {
        [Inject] private ISignalManager _signalManager;
        [Inject] private IVictoryConditionManager _victoryConditionManager;

        [SerializeField] private string cancelMessage = "Game.Interaction.Elevator.Cancel.VictoryConditions";

        protected override bool ShouldContiniueInteraction(
            Creature creature,
            Interaction interaction,
            out string messageKey
        )
        {
            // If all victory conditions achieved then we dont display
            if (_victoryConditionManager.VictoryConditions.All(x => x.Value))
            {
                messageKey = null;
                return true;
            }
            
            // Else we do display the warning
            messageKey = cancelMessage;
            return false;
        }

        protected override void OnInteractionComplete(Interaction interaction)
        {
            base.OnInteractionComplete(interaction);

            interaction.Creature.gameObject.SetActive(false);

            _signalManager.Signal(Signal.CreatureExited);
        }
    }
}