using Managers.Visual;
using Markers;
using UnityEngine;
using Zenject;

namespace Items.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GrenadeProjectile : Projectile
    { 
        [Inject] ICameraShakeService _cameraShakeService;
        
        [SerializeField] private float fuseTime = 2.0f; // Time before explosion
        [SerializeField] private float radius = 3f; // Radius of the explosion
        [SerializeField] private float maxDistanceSpeedCorrection = 2f; // distance where the speed correction starts

        [SerializeField] private GameObject explosionPrefab; // Prefab for the explosion effect
        [SerializeField] private AudioClip explosionSound; // Sound to play when the grenade explodes
        [SerializeField] private float cameraShakeFactor = 1; // Sound to play when the grenade explodes
        
        private Rigidbody2D _rb;
        private float _timeUntilExplosion;

        protected override void Awake()
        {
            base.Start();
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                GameLogger.LogError("GrenadeProjectile requires a Rigidbody2D component!");
                enabled = false; // Disable the script if no Rigidbody2D is found
            }
        }

        public override void Launch(AttackContext ctx)
        {
            base.Launch(ctx); // Call the base class's Launch method

            _timeUntilExplosion = fuseTime;

            // Calculate initial velocity based on angle and speed
            var distance = Vector2.Distance(transform.position, AttackContext.TargetPosition);
            var speed = Mathf.Min(CalculateInitialVelocity(distance, _rb.linearDamping), Speed);
            Vector2 initialVelocity = AttackContext.Direction.normalized * speed;

            // Apply the initial velocity to the Rigidbody2D
            _rb.linearVelocity = initialVelocity;
        }

        protected override void Update()
        {
            if (!IsLaunched) return;

            _timeUntilExplosion -= Time.deltaTime;

            if (_timeUntilExplosion <= 0)
            {
                Explode();
            }
        }

        private void FixedUpdate()
        {
            if(!IsLaunched)
                return;
            
            // Correct the speed so grenade will stop at the target
            var distance = Vector2.Distance(transform.position, AttackContext.TargetPosition);
            if (distance < maxDistanceSpeedCorrection)
            {
                _rb.linearVelocity = Mathf.Lerp(_rb.linearVelocity.magnitude, 0f, distance - maxDistanceSpeedCorrection) * _rb.linearVelocity.normalized;
            }
        }
        
        float CalculateInitialVelocity(float distance, float linearDrag)
        {
            if (linearDrag <= 0)
                return distance; // If no drag, velocity equals distance

            return distance * linearDrag;
        }


        private void Explode()
        {
            // Instantiate explosion prefab
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            // Play explosion sound
            if (explosionSound != null)
            {
                SoundPlayer.PlaySound(explosionSound, transform.position);
            }
            
            _cameraShakeService.ShakeCamera(transform.position, Damage * cameraShakeFactor);

            // Apply damage to nearby creatures (example)
            Collider2D[] results = new Collider2D[50];
            var size = Physics2D.OverlapCircleNonAlloc(transform.position, radius, results);

            for (int i = 0; i < size; i++)
            {
                var hitCollider = results[i];

                var hitDamageable = hitCollider.GetComponent<DamageableCollider>()?.Damagable;
                
                if(hitDamageable == null)
                    continue;
                
                HandleHitDamageable(hitDamageable);
                
                if (Creature.IsCreature(hitCollider.gameObject))
                {
                    
                    if (!ReferenceEquals(hitDamageable, AttackContext.Attacker))
                    {
                        // Apply damage to the creature
                        HandleHitDamageable(hitDamageable);
                    }
                }
            }


            // Destroy the grenade
            Destroy(gameObject);
        }
    }
}