using UnityEngine;

namespace Items.PassiveItems
{
    public class ModifierEffect : PassiveEffect
    {
        public Modifier Modifier => _modifier ?? modifierTemplate.ToModifier();
        public ModifierTemplate ModifierTemplate => modifierTemplate;

        [SerializeField] private ModifierTemplate modifierTemplate;
        
        private Modifier _modifier;

        protected override void Awake()
        {
            base.Awake();

            _modifier = modifierTemplate.ToModifier();
        }

        protected override void OnActivation()
        {
            base.OnActivation();

            var creature = Creature;
            
            if (creature is null)
            {
                GameLogger.LogError("Creature is null");
                return;
            }
            
            _modifier ??= modifierTemplate.ToModifier(); // If crew is disabled on spawn, Awake won't be called,
                                                         // so we need to ensure _modifier is initialized.

            creature.ModifierReceiver.AddModifier(_modifier);
        }
        
        protected override void OnDeactivation()
        {
            base.OnDeactivation();

            var creature = Creature;
            
            if (creature is null)
            {
                GameLogger.LogError("Creature is null");
                return;
            }

            if (creature.ModifierReceiver.HasModifier(_modifier))
            {
                creature.ModifierReceiver.RemoveModifier(_modifier);
            }
        }
    }
}