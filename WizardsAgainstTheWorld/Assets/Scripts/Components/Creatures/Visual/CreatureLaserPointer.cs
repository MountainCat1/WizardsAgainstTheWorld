using System.Collections;
using UnityEngine;
using Utilities.Components;

namespace Visual
{
    public class CreatureLaserPointer : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        [SerializeField] private SpriteLineBetween lineBetween;
        [SerializeField] private float resetDelay = 0.5f; // Time in seconds to reset the laser pointer

        private Coroutine _resetLaserCoroutine;


        private void Awake()
        {
            ResetLaser();
            
            RegisterCreature(creature);
        }
        
        private void RegisterCreature(Creature creature)
        {
            creature.WeaponAttacked += OnWeaponAttacked;
        }

        private void UnregisterCreature(Creature creature)
        {
            creature.WeaponAttacked -= OnWeaponAttacked;
        }

        private void OnWeaponAttacked(AttackContext attackContext)
        {
            if (_resetLaserCoroutine != null)
                StopCoroutine(_resetLaserCoroutine);
            
            if (lineBetween == null)
            {
                Debug.LogError("LineBetween component is not assigned in LaserPointer.");
                return;
            }

            if (attackContext.Target != null)
            {
                lineBetween.SetPoints(attackContext.Attacker.transform, attackContext.Target.transform, true);
            }
            else
            {
                Debug.LogWarning("AttackContext does not have a target. Laser pointer will not be drawn.");
            }

            var attackSpeed = attackContext.Weapon.BaseAttackSpeed;
            
            StartCoroutine(ResetLaserAfterDelay(1f/ attackSpeed));
        }

        private IEnumerator ResetLaserAfterDelay(float baseDelay)
        {
            yield return new WaitForSeconds(baseDelay + resetDelay);

            if (lineBetween != null)
            {
                ResetLaser();
            }
            else
            {
                Debug.LogError("LineBetween component is not assigned in LaserPointer.");
            }
        }

        private void ResetLaser()
        {
            lineBetween.SetPoints(null, null, false);
            _resetLaserCoroutine = null;
        }
    }
}