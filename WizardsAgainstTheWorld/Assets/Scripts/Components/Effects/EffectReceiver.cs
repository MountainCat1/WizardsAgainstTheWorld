using System.Collections.Generic;

namespace Items.PassiveItems
{
    public interface IEffectReceiver
    {
        public void AddEffect(PassiveEffect effect);
        public void RemoveEffect(PassiveEffect effect);
        public IReadOnlyCollection<PassiveEffect> Effects { get; }
        public void ClearEffects();
        public bool HasEffect<T>() where T : PassiveEffect;
    }
}