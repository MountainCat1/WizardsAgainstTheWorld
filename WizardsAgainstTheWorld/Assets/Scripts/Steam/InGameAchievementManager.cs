using Components;
using DefaultNamespace.PersistentData;
using Items.Weapons;
using Managers;
using UnityEngine;
using Zenject;

namespace Steam
{
    public class InGameAchievementManager : MonoBehaviour
    {
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private ICreatureEventProducer _creatureEventProducer;
        [Inject] private IProjectileManager _projectileManager;

        [Inject] private IAchievementsManager _achievementsManager;

        private AchievementProgress _achievementProgress;

        private void Start()
        {
            _creatureEventProducer.CreatureDied += OnCreatureDied;
            _creatureEventProducer.CreatureHit += OnCreatureHit;
            _projectileManager.ProjectileHit += OnProjectileHit;
            _achievementProgress = AchievementProgress.Instance;
        }

        private void OnProjectileHit(Projectile projectile, AttackContext attackCtx, IDamageable damageable)
        {
            if(attackCtx.Attacker == null)
                return;
            
            if(projectile.name.ToLower().Trim().Contains("grenade") 
               && attackCtx.Attacker.Team == Teams.Player
               && damageable is not Creature)
            {
                _achievementProgress.PropsKilledWithGrenades++;
                HandleEventHandling();
            }
        }

        private void OnCreatureHit(Creature creature, HitContext hitCtx)
        {
            if(hitCtx.Attacker != null && hitCtx.Attacker.Team != Teams.Player)
                return;
            
            
        }

        private void OnDestroy()
        {
            AchievementProgress.Save();
        }

        private void OnCreatureDied(Creature creature, DeathContext ctx)
        {
            var killerCreature = ctx.Killer as Creature;
            var killedCreature = ctx.KilledEntity as Creature;

            if (killedCreature is null || killerCreature is null)
            {
                HandleEventHandling();
                return;
            }

            _achievementProgress.EnemiesKilled++;
            if (killedCreature.Boss)
            {
                _achievementProgress.BossesKilled++;
            }

            var killedId = killedCreature.GetIdentifier().ToLowerInvariant().Replace(" ", "");
            _achievementProgress.EnemiesKilledByType.TryAdd(killedId, 0);
            _achievementProgress.EnemiesKilledByType[killedId]++;

            HandleEventHandling();
        }


        private void HandleEventHandling()
        {
            AchievementProgress.Update(_achievementProgress);
            _achievementsManager.CheckForAchievements();
        }
    }
}