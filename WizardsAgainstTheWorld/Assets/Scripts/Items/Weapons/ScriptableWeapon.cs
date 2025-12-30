using Triggers;
using UnityEngine;

namespace Items.Weapons
{
    public class ScriptableWeapon : Weapon
    {
        [SerializeField] private ScriptableTrigger trigger;
        
        protected override void Attack(AttackContext ctx)
        {
            // Please use WeaponAttackTrigger
            if (trigger == null)
            {
                Debug.LogWarning("ScriptableWeapon: No trigger assigned. Please assign a ScriptableTrigger to the weapon.");
                return;
            }
            
            trigger.Trigger();
        }
    }
}