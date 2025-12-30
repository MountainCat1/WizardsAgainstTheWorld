using UnityEngine;

namespace BossAttacks
{
    public class WeaponBossAttack : BossAttack
    {
        [SerializeField] private Weapon weapon;
        
        protected override void ShowMarker(Vector2 position)
        {
            return;
        }

        protected override void PerformActualAttack(Vector2 targetPosition)
        {
            var attackContext = new AttackContext
            {
                Attacker = bossCreature,
                Target = null,
                TargetPosition = targetPosition,
                Direction = (targetPosition - (Vector2)bossCreature.transform.position).normalized,
            };
            
            weapon.PerformAttack(attackContext);
        }
    }
}