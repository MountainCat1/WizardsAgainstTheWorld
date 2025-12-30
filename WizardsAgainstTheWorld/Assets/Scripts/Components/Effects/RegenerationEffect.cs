using System.Collections;
using Components;
using UnityEngine;
using Utilities;

namespace Items.PassiveItems
{
    public class RegenerationEffect : PassiveEffect
    {
        public float RegenerationAmount => regenerationAmount;
        public float RegenerationInterval => regenerationInterval;
        
        [SerializeField] private float regenerationAmount = 1f;
        [SerializeField] private float regenerationInterval = 1f;

        private void Start()
        {
            this.StartRandomOffsetCoroutine(RegenerateHealth());
        }

        private IEnumerator RegenerateHealth()
        {
            while (true)
            {
                yield return new WaitForSeconds(regenerationInterval);

                if (!Active)
                    continue;

                var creature = Creature;

                if (creature == null)
                {
                    GameLogger.LogError("Creature is null");
                    yield break;
                }

                if (creature.Health.CurrentValue >= creature.Health.MaxValue && regenerationAmount >= 0)
                {
                    continue;
                }

                if (creature.Health.CurrentValue <= 0 && regenerationAmount < 0)
                {
                    continue;
                }

                if (regenerationAmount >= 0)
                {
                    creature.Heal(new HealContext()
                    {
                        Healer = creature,
                        Target = creature,
                        HealAmount = regenerationAmount,
                    });
                }
                else
                {
                    creature.Damage(new HitContext()
                    {
                        Attacker = creature,
                        Target = creature,
                        Damage = -regenerationAmount,
                        PushFactor = 0,
                        TargetPosition = creature.transform.position
                    });
                }
            }
        }
    }
}