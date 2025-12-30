using System.Collections;
using UnityEngine;

namespace Managers
{
    public class DamageStatusEffect : StatusEffect
    {
        [SerializeField] private float damagePerSecond = 1f;
        
        protected override void StartStatusEffect()
        {
            StartCoroutine(ApplyDamage());
        }
        
        private IEnumerator ApplyDamage()
        {
            while (true)
            {
                Target.Damage(new HitContext()
                {
                    Damage = damagePerSecond,
                    Attacker = Source,
                    Target = Target,
                    PushFactor = 0f,
                });
                yield return new WaitForSeconds(1f);
            }
        }
    }
}