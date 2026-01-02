using Components;
using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(IDamageable))]
    public class DismantableObject : InteractionBehavior
    {
        protected override void OnInteractionComplete(Interaction interaction)
        {
            base.OnInteractionComplete(interaction);

            var damageable = GetComponent<IDamageable>();
            
            damageable.Health.Damage(new HitContext()
            {
                Attacker = interaction.Entity,
                Damage = damageable.Health.CurrentValue,
                PushFactor = 0,
                Target = damageable
            });

            Destroy(gameObject);
        }
    }
}