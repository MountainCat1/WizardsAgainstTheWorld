using System.Collections;
using Components;
using Managers;
using UnityEngine;
using Zenject;

namespace Items.Weapons
{
    public class ProjectileWeapon : Weapon
    {
        [Inject] private IProjectileManager _projectileManager;

        [field: SerializeField] private Projectile ProjectilePrefab { get; set; }
        [field: SerializeField] private float ProjectileSpeed { get; set; }
        [field: SerializeField] private float ShotOverTime { get; set; } = 0f; // 0 = Instant (Shotgun), >0 = Over time (Flamethrower)
        [field: SerializeField] private bool DestroyProjectilesOnHit { get; set; } = true;
        [field: SerializeField] public int ProjectileCount { get; set; } = 1; // Number of projectiles per shot
        
        public override bool NeedsLineOfSight => true;
        
        private const float MaxSpreadAngle = 90f; // Maximum spread angle in degrees

        protected override void Attack(AttackContext ctx)
        {
            if (ShotOverTime > 0)
            {
                // Flamethrower-style attack over time
                StartCoroutine(FireOverTime(ctx));
            }
            else
            {
                // Shotgun-style burst attack
                FireBurst(ctx);
            }
        }

        private void FireBurst(AttackContext ctx)
        {
            for (int i = 0; i < ProjectileCount; i++)
            {
                FireProjectile(ctx, GetSpreadDirection(ctx.Direction, ctx));
            }
        }

        private IEnumerator FireOverTime(AttackContext ctx)
        {
            float interval = ShotOverTime / ProjectileCount;

            for (int i = 0; i < ProjectileCount; i++)
            {
                FireProjectile(ctx, GetSpreadDirection(ctx.Direction, ctx));
                yield return new WaitForSeconds(interval);
            }
        }

        private void FireProjectile(AttackContext ctx, Vector2 overrideDirection)
        {
            var projectile = _projectileManager.SpawnProjectile(ProjectilePrefab, transform.position);

            Vector2 normalizedDirection = overrideDirection.normalized;
            float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            var damage = WeaponItemData.GetApplied(WeaponPropertyModifiers.Damage, BaseDamage);
            var accuracy = WeaponItemData.GetApplied(WeaponPropertyModifiers.Accuracy, BaseAccuracyPercent);
            
            projectile.Initialize(
                speed: ProjectileSpeed,
                damage: CalculateDamage(damage, ctx),
                accuracy: CalculateAccuracy(accuracy, ctx),
                destroyOnHit: DestroyProjectilesOnHit
            );

            ctx.Direction = overrideDirection;

            projectile.Hit += OnProjectileHit;
            projectile.Missed += OnProjectileMissed;
            projectile.Launch(ctx);
            
            InvokeStroked();
        }
        private Vector2 GetSpreadDirection(Vector2 baseDirection, AttackContext ctx)
        {
            var calculatedSpreadAngle = CalculateSpreadAngle(BaseAccuracyPercent, ctx.Attacker);
            float randomAngle = Random.Range(-calculatedSpreadAngle, calculatedSpreadAngle);
            float angleRad = randomAngle * Mathf.Deg2Rad;

            float newX = baseDirection.x * Mathf.Cos(angleRad) - baseDirection.y * Mathf.Sin(angleRad);
            float newY = baseDirection.x * Mathf.Sin(angleRad) + baseDirection.y * Mathf.Cos(angleRad);

            return new Vector2(newX, newY).normalized;
        }

        private static float CalculateSpreadAngle(float baseAngle, Entity creature)
        {
            return Mathf.Lerp(MaxSpreadAngle, 0f, (baseAngle + creature.ModifierReceiver.AccuracyFlatModifier) / 100f);
        }

        private void OnProjectileHit(IDamageable damageable, AttackContext attackCtx)
        {
            var hitCtx = new HitContext
            {
                Attacker = attackCtx.Attacker,
                Damage = CalculateDamage(WeaponItemData.GetApplied(WeaponPropertyModifiers.Damage, BaseDamage),
                    attackCtx),
                Target = damageable,
                PushFactor = PushFactor
            };

            OnHit(damageable, hitCtx);
        }
        
        private void OnProjectileMissed(AttackContext attackContext, Entity missedEntity)
        {
            InvokeMissed(attackContext, missedEntity);
        }
    }
}