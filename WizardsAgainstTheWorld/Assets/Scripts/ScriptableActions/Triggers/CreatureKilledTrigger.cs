using UnityEngine;

namespace Triggers
{
    public class CreatureKilledTrigger : TriggerBase
    {
        [field: SerializeField] private Creature creature;

        protected override void OnStart()
        {
            creature.Health.Death += OnCreatureDeath;
        }

        private void OnCreatureDeath(DeathContext obj)
        {
            RunActions();
        }
    }
}