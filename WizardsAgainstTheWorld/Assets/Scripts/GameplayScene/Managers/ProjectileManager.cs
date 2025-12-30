using System;
using Components;
using Items.Weapons;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface IProjectileManager
    {
        public event Action<Projectile> ProjectileSpawned;
        public event Action<Projectile, AttackContext, Entity> ProjectileMissed;
        public event Action<Projectile, AttackContext, IDamageable> ProjectileHit;
        
        public Projectile SpawnProjectile(Projectile prefab, Vector2 position);
    }
    
    public class ProjectileManager : IProjectileManager
    {
        [Inject] ISpawnerManager _spawnerManager;

        public event Action<Projectile, AttackContext, IDamageable> ProjectileHit;
        public event Action<Projectile> ProjectileSpawned;
        public event Action<Projectile, AttackContext, Entity> ProjectileMissed;
        
        public Projectile SpawnProjectile(Projectile prefab, Vector2 position)
        {
            var projectile = _spawnerManager.Spawn(
                prefab,
                position
            );

            HandleNewProjectile(projectile);
            
            return projectile;
        }

        private void HandleNewProjectile(Projectile projectile)
        {
            ProjectileSpawned?.Invoke(projectile);
            
            projectile.Hit += ((creature, context) =>
            {
                ProjectileHit?.Invoke(projectile, context, creature);
            });
            projectile.Missed += ((attackCtx, entity) =>
            {
                ProjectileMissed?.Invoke(projectile, attackCtx, entity);
            });
        }
    }
}