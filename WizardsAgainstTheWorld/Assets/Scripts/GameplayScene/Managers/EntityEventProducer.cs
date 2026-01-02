using System;
using Components;
using Zenject;

namespace Managers
{
    public interface IEntityEventProducer
    {
        public event Action<Entity, HitContext> EntityHit;
        public event Action<Entity, DeathContext> EntityDied;

        public event Action<Entity, HealContext> EntityHeal;
    }
    
    public class EntityEventProducer : IEntityEventProducer
    {
        [Inject] private IEntityManager _creatureManager;
        
        public event Action<Entity, HitContext> EntityHit;
        public event Action<Entity, DeathContext> EntityDied;
        public event Action<Entity, HealContext> EntityHeal;

        [Inject]
        private void Initialize()
        {
            _creatureManager.EntitySpawned += OnEntitySpawned;
        }

        private void OnEntitySpawned(Entity creature)
        {
            creature.Health.Hit += (HitContext context) =>
            {
                EntityHit?.Invoke(creature, context);
            };
            
            creature.Health.Death += (DeathContext context) =>
            {
                EntityDied?.Invoke(creature, context);
            };
            
            creature.Health.Healed += (HealContext context) =>
            {
                EntityHeal?.Invoke(creature, context);
            };
        }
    }
}