using System;
using Components;
using Zenject;

namespace Managers
{
    public interface ICreatureEventProducer
    {
        public event Action<Creature, HitContext> CreatureHit;
        public event Action<Creature, DeathContext> CreatureDied;

        public event Action<Creature, HealContext> CreatureHeal;
    }
    
    public class CreatureEventProducer : ICreatureEventProducer
    {
        [Inject] private ICreatureManager _creatureManager;
        
        public event Action<Creature, HitContext> CreatureHit;
        public event Action<Creature, DeathContext> CreatureDied;
        public event Action<Creature, HealContext> CreatureHeal;

        [Inject]
        private void Initialize()
        {
            _creatureManager.CreatureSpawned += OnCreatureSpawned;

            foreach (var creature in _creatureManager.GetCreatures())
            {
                OnCreatureSpawned(creature);
            }
        }

        private void OnCreatureSpawned(Creature creature)
        {
            creature.Health.Hit += (HitContext context) =>
            {
                CreatureHit?.Invoke(creature, context);
            };
            
            creature.Health.Death += (DeathContext context) =>
            {
                CreatureDied?.Invoke(creature, context);
            };
            
            creature.Health.Healed += (HealContext context) =>
            {
                CreatureHeal?.Invoke(creature, context);
            };
        }
    }
}