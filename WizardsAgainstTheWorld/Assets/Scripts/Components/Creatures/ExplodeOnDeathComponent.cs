using Items.Weapons;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(IDamageable))]
    public class ExplodeOnDeathComponent : MonoBehaviour
    {
        [SerializeField] private KamikazeWeapon kamikazeWeapon;

        private Creature _creature;
        
        private void Start()
        {
            var healthComponent = GetComponent<IDamageable>();
            if (healthComponent != null)
            {
                healthComponent.Health.Death += OnDeath;
                _creature = healthComponent as Creature;
            }
        }

        private void OnDeath(DeathContext obj)
        {
            kamikazeWeapon.PerformAttack(new AttackContext
            {
                Attacker = _creature, 
                Direction = Vector2.zero,
            });
        }
    }
}