using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Triggers
{
    public class InteractableTrigger : TriggerBase
    {
        [FormerlySerializedAs("interactableObject")] [SerializeField]
        private InteractionBehavior interactionBehavior;

        protected override void OnStart()
        {
            var highestPriorityInteraction = interactionBehavior
                .GetComponents<InteractionBehavior>()
                .OrderByDescending(x => x.Priority)
                .First();


            GameLogger.Log($"[InteractableTrigger] {interactionBehavior.name} started. Subscribing to {highestPriorityInteraction} event.");

            highestPriorityInteraction.InteractCompleted += OnInteractCompleted;
        }

        private void OnInteractCompleted(Entity obj)
        {
            RunActions();
        }
    }
}