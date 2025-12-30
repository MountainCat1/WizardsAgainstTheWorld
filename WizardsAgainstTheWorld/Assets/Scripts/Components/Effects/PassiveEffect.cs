using System;
using UnityEngine;

namespace Items.PassiveItems
{
    public class PassiveEffectContext
    {
        public Creature Creature { get; }

        public PassiveEffectContext(Creature creature)
        {
            Creature = creature ?? throw new ArgumentNullException(nameof(creature), "Creature cannot be null.");
        }
    }

    public class PassiveEffect : MonoBehaviour
    {
        protected Creature Creature { get; private set; }
        protected bool Active { get; private set; } = false;

        protected virtual void Awake()
        {
        }

        public void Activate(PassiveEffectContext ctx)
        {
            Creature = ctx.Creature
                       ?? throw new ArgumentNullException(nameof(ctx.Creature), "Creature cannot be null.");

            if (Active)
            {
                throw new InvalidOperationException("PassiveEffect is already active.");
            }

            Active = true;

            OnActivation();
        }

        public void Deactivate()
        {
            if (Creature == null)
            {
                throw new InvalidOperationException("PassiveEffect is not initialized with an Item.");
            }

            if (!Active)
            {
                throw new InvalidOperationException("PassiveEffect is not active.");
            }

            Active = false;

            try
            {
                OnDeactivation();
            }
            catch (Exception e)
            {
                GameLogger.LogException(e);
            }

            Creature = null;
        }

        protected virtual void OnActivation()
        {
            // Override in derived classes to implement activation logic
        }

        protected virtual void OnDeactivation()
        {
            // Override in derived classes to implement deactivation logic
        }
    }
}