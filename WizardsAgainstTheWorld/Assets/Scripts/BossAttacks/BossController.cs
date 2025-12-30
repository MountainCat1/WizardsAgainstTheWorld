using System.Collections;
using CreatureControllers;
using UnityEngine;

namespace DefaultNamespace.BossAttacks
{
    public class BossController : UnitController
    {
        [Header("Boss Stuff")]
        [SerializeField] private BossAttack[] attacks;
        [SerializeField] private float roundDuration = 5f;
        
        private Coroutine _behaviourCoroutine;

        protected override void Start()
        {
            base.Start();
            
            _behaviourCoroutine = StartCoroutine(BossBehaviourCoroutine());

            foreach (var attack in attacks)
            {
                attack.bossCreature = Creature;
            }
        }

        private IEnumerator BossBehaviourCoroutine()
        {
            while (true)
            {
                foreach (var attack in attacks)
                {
                    if (attack == null) continue;

                    if (Target == null) continue;
                    
                    attack.StartAttack(Target.transform.position);
                    
                    yield return new WaitForSeconds(roundDuration);
                }
                
                yield return new WaitForSeconds(1f); // Pause between rounds
            }
        }
    }
}