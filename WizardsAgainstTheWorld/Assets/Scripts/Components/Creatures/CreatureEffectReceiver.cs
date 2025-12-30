using System.Collections.Generic;
using Items.PassiveItems;
using UnityEngine;

namespace Components.Creatures
{
    public class CreatureEffectReceiver : IEffectReceiver
    {
        private Creature _creature;
        private readonly List<PassiveEffect> _effects = new();

        public CreatureEffectReceiver(Creature creature)
        {
            _creature = creature ?? throw new System.ArgumentNullException(nameof(creature), "Creature cannot be null.");
        }
        
        public void AddEffect(PassiveEffect effect)
        {
            if (effect == null)
            {
                Debug.LogError("EffectReceiver: AddEffect - effect is null.");
                return;
            }

            _effects.Add(effect);
            effect.Activate(new PassiveEffectContext(_creature));
        }

        public void RemoveEffect(PassiveEffect effect)
        {
            if (effect == null)
            {
                Debug.LogError("EffectReceiver: RemoveEffect - effect is null.");
                return;
            }

            if (_effects.Remove(effect))
            {
                effect.Deactivate();
            }
        }

        public IReadOnlyCollection<PassiveEffect> Effects => _effects.AsReadOnly();

        public void ClearEffects()
        {
            foreach (var effect in _effects)
            {
                effect.Deactivate();
            }

            _effects.Clear();
        }

        public bool HasEffect<T>() where T : PassiveEffect
        {
            return _effects.Exists(e => e is T);
        }
    }
}