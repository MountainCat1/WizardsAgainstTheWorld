using UnityEngine;

namespace Managers
{
    public class ModifierStatusEffect : StatusEffect
    {
        [field: SerializeField] public ModifierTemplate Modifier { get; set; } = new();
        
        private Modifier _modifier;

        private void Awake()
        {
            // Initialize the modifier
            _modifier = Modifier.ToModifier();
        }

        protected override void StartStatusEffect()
        {
            // Check if the target is a modifiable object
            if (Target is IModifiable modifiable)
            {
                // Add the modifier to the target's modifier receiver
                modifiable.ModifierReceiver.AddModifier(_modifier);
            }
            else
            {
                GameLogger.LogError("Target does not implement IModifiable");
            }
        }

        protected override void OnEndStatusEffect()
        {
            // Check if the target is a modifiable object
            if (Target is IModifiable modifiable)
            {
                // Remove the modifier from the target's modifier receiver
                modifiable.ModifierReceiver.RemoveModifier(_modifier);
            }
            else
            {
                GameLogger.LogError("Target does not implement IModifiable");
            }
        }
    }
}