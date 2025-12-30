using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Items.Weapons
{
    public class KamikazeWeapon : TouchWeapon
    {
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private GameObject effectPrefab;

        [Inject] private ISpawnerManager _spawnerManager;


        public override void Equip(ItemUseContext ctx)
        {
            // base.Equip(ctx);
            
            if (Creature != null)
            {
                Creature.Health.Death += OnCreatureDeath;
            }
        }

        public override void UnEquip(ItemUnUseContext ctx)
        {
            base.UnEquip(ctx);

            if (Creature != null)
            {
                Creature.Health.Death -= OnCreatureDeath;
            }
        }

        protected override void Attack(AttackContext ctx)
        {
            Explode(ctx);
        }

        private void Explode(AttackContext ctx)
        {
            var damage = WeaponItemData.GetApplied(WeaponPropertyModifiers.Damage, BaseDamage);

            var hitTargets = CollisionUtility.GetCreaturesInRadius(transform.position, explosionRadius);

            foreach (var target in hitTargets)
            {
                var hitCtx = new HitContext()
                {
                    Attacker = ctx.Attacker,
                    Damage = CalculateDamage(damage, ctx),
                    Target = target,
                    PushFactor = PushFactor
                };

                if (target.Health == ctx.Attacker?.Health)
                {
                    hitCtx.Damage = target.Health.CurrentValue; // Ensure we deal enough damage to destroy ourselves
                }

                OnHit(target, hitCtx);
            }


            InvokeStroked();

            _spawnerManager.Spawn(effectPrefab, transform.position);
        }

        private void OnCreatureDeath(DeathContext ctx)
        {
            Explode(new AttackContext
            {
                Attacker = Creature ?? null,
                Direction = Vector2.zero, // No direction needed for explosion
            });
        }
    }
}