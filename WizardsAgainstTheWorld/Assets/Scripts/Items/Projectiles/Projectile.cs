using System;
using Combat;
using Components;
using Managers;
using Markers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Items.Weapons
{
    public abstract class Projectile : MonoBehaviour
    {
        private const float Lifetime = 16f;

        public event Action<IDamageable, AttackContext> Hit;
        public event Action<AttackContext, Entity> Missed;

        [Inject] protected ISoundPlayer SoundPlayer;
        [Inject] protected ISpawnerManager SpawnerManager;
        [Inject] protected ITeamManager TeamManager;

        [SerializeField] private AudioClip hitSound;
        [SerializeField] private GameObject effectPrefab;

        public float Speed { get; protected set; }
        public float Damage { get; protected set; }
        public bool DestroyOnHit { get; protected set; } = true;
        public float AccuracyPercentage { get; protected set; }
        public float AoeRadius { get; protected set; } = 0f;

        protected bool IsLaunched = false;
        protected bool Initialized = false;
        protected AttackContext AttackContext;

        private bool _hit = false;

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            Destroy(gameObject, Lifetime);
        }

        public virtual void Initialize(float speed,
            float damage,
            float accuracy,
            float aoeRadius = 0f,
            bool destroyOnHit = true
        )
        {
            Speed = speed;
            Damage = damage;
            DestroyOnHit = destroyOnHit;
            AccuracyPercentage = accuracy;
            AoeRadius = aoeRadius;
            if (AoeRadius > 0f)
            {
                GameLogger.LogError("AoeRadius is not supported yet. Please implement AOE handling.");
            }

            Initialized = true;
        }

        public virtual void Launch(AttackContext ctx)
        {
            if (!Initialized) throw new Exception("Projectile not initialized");

            AttackContext = ctx;

            IsLaunched = true;
        }

        protected virtual void OnProjectileCollision(Collider2D other)
        {
            if (CollisionUtility.IsWall(other.gameObject))
            {
                HandleHitWall();
                return;
            }

            if (AoeRadius == 0f && _hit)
            {
                GameLogger.LogWarning("Projectile already hit something, ignoring further collisions.");
                return;
            }


            if (CollisionUtility.IsObstacle(other.gameObject))
            {
                if (TryMiss(HitChanceSettings.Obstacle, other.GetComponent<Entity>()))
                    return;

                HandleHitObstacle();
                return;
            }


            if (Creature.IsCreature(other.gameObject) == false)
            {
                var damageableCollider = other.GetComponent<DamageableCollider>();

                if (damageableCollider == null)
                {
                    GameLogger.LogWarning(
                        $"Projectile hit something that is not a creature or damageable {other.gameObject.name}"
                    );
                    return;
                }

                if (damageableCollider.Damagable != null)
                {
                    HandleHitDamageable(damageableCollider.Damagable);
                }

                return;
            }

            var creatureCollider = other.GetComponent<CreatureCollider>();

            if (creatureCollider == null)
            {
                GameLogger.LogWarning($"Projectile hit something that is not a creature {other.gameObject.name}");
                return;
            }

            var hitCreature = creatureCollider.Creature;

            if (hitCreature == null)
            {
                GameLogger.LogError("Projectile hit something that is not a creature");
                return;
            }

            if (hitCreature == AttackContext.Attacker) return;

            if (TeamManager.GetAttitude(AttackContext.Team, hitCreature.Team) == Attitude.Friendly)
            {
                if (TryMiss(HitChanceSettings.Friendly, hitCreature)) return;
            }

            if (TryMiss(HitChanceSettings.Enemy, hitCreature)) return;

            HandleHitDamageable(hitCreature);
        }

        protected virtual void HandleHitWall()
        {
            _hit = true;

            PlayHitSound();
            Hit?.Invoke(null, AttackContext);
            Destroy(gameObject);
        }

        protected virtual void HandleHitObstacle()
        {
            _hit = true;

            PlayHitSound();
            Hit?.Invoke(null, AttackContext);
            Destroy(gameObject);
        }

        protected virtual void HandleHitDamageable(IDamageable damageable)
        {
            _hit = true;

            try
            {
                PlayHitSound();
                Hit?.Invoke(damageable, AttackContext);
            }
            catch (Exception e)
            {
                GameLogger.LogException(e);
            }

            if (DestroyOnHit)
                Destroy(gameObject);
        }

        protected virtual void PlayHitSound()
        {
            if (hitSound) SoundPlayer.PlaySound(hitSound, transform.position, SoundType.Sfx);
        }

        protected virtual void Update()
        {
        }

        protected bool TryMiss(HitChanceProfile hitChance, Entity entity)
        {
            var miss = HitChanceCalculator.ShouldMiss(hitChance, AccuracyPercentage / 100f);

            if (miss)
            {
                Missed?.Invoke(AttackContext, entity);
                GameLogger.Log("Projectile missed");
                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            if (effectPrefab != null)
            {
                SpawnerManager.Spawn(effectPrefab, transform.position);
            }
            else
            {
                GameLogger.LogWarning("Effect prefab is not set for projectile " + name);
            }
        }
    }
}