using System;
using UnityEngine;

namespace Items
{
    public struct AbilityUseContext
    {
        public Creature User { get; }
        public Creature Target { get; }
        public Vector2 TargetPosition { get; }

        public AbilityUseContext(Creature user, Creature target, Vector2 targetPosition)
        {
            User = user;
            Target = target;
            TargetPosition = targetPosition;
        }
    }

    public class Ability
    {
        public bool Targetable { get; }

        public string Identifier { get; }

        public Ability(string identifier, bool targetable = false)
        {
            Targetable = targetable;
            Identifier = identifier;
        }
    }

    public class ActiveItemBehaviour : ItemBehaviour
    {
        public event Action<AbilityUseContext> ActiveAbilityUsed;
        protected virtual bool Targetable => false;

        public Ability GetAbility()
        {
            return new Ability(
                identifier: $"{GetIdentifier()}_ability",
                targetable: Targetable
            );
        }

        public virtual void UseActiveAbility(AbilityUseContext context)
        {
            GameLogger.Log(
                $"Using ability {GetAbility().Identifier} on {context.Target?.name ?? "position " + context.TargetPosition}");

            ActiveAbilityUsed?.Invoke(context);
        }
    }
}