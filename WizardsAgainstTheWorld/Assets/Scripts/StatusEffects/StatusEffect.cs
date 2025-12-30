using UnityEngine;

namespace Managers
{
    public struct StatusEffectContext
    {
        public Creature Source { get; private set; }
        public Creature Target { get; private set; }

        public StatusEffectContext(Creature source, Creature target)
        {
            Source = source;
            Target = target;
        }
    }

    public abstract class StatusEffect : MonoBehaviour
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Description { get; set; }
        [field: SerializeField] public float Duration { get; set; } = -1;

        protected Creature Target { get; private set; }
        protected Creature Source { get; private set; }
        public string NameKey => $"UI.StatusEffects.{Name}.Name";

        public void Initialize(StatusEffectContext ctx)
        {
            // Initialize properties
            Target = ctx.Target;
            Source = ctx.Source;

            // Start the status effect
            StartStatusEffect();
            
            // Destroy the status effect after the duration
            if (Duration > 0)
            {
                Destroy(gameObject, Duration);
            }
        }

        protected abstract void StartStatusEffect();
        protected virtual void OnEndStatusEffect(){}

        public void ExtendDuration(float duration)
        {
            if (Duration < duration)
            {
                Duration = duration;

                // Restart the status effect
                Destroy(gameObject, Duration);
            }
        }

        private void OnDestroy()
        {
            OnEndStatusEffect();
        }
    }
}