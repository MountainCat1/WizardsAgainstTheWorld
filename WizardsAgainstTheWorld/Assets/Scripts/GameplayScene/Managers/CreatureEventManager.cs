using System;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class CreatureEventManager : MonoBehaviour
    {
        [Inject] private ICreatureManager _creatureManager;

        [Inject] 
        private void Construct()
        {
            _creatureManager.CreatureSpawned += OnCreatureSpawned;
        }

        private void OnCreatureSpawned(Creature creature)
        {
            RegisterCreatureEvents(creature);   
        }
        
        private void RegisterCreatureEvents(Creature creature)
        {
           creature.Health.Death += OnCreatureDeath;
        }

        
        // Event Handlers
        private void OnCreatureDeath(DeathContext ctx)
        {
            if (ctx.Killer == null)
            {
                GameLogger.LogWarning("Killer is null in DeathContext");
                return;
            }
            if(ctx.KilledEntity == null)
                throw new NullReferenceException("Creature is null in DeathContext");
            
            var killedEntity = ctx.KilledEntity;
            var killer = ctx.Killer;

            if(killer is Creature killerCreature && killedEntity is Creature killedCreature)
                killerCreature.AwardXp(killedCreature.XpAmount);
            
            GameLogger.Log($"{killedEntity.name} was killed by {killer.name}");
        }
    }
}