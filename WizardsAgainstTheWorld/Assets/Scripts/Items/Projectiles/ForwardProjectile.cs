namespace Items.Weapons
{
    using UnityEngine;

    namespace Items.Weapons
    {
        [RequireComponent(typeof(Rigidbody2D))]
        public class ForwardProjectile : Projectile
        {
            [SerializeField] private ColliderEventProducer colliderEventProducer;
            [SerializeField] private ColliderEventProducer obstacleColliderEventProducer;
            
            [SerializeField] private float lifeTime = 4f;

            private Rigidbody2D _rb;
            
            protected override void Awake()
            {
                base.Awake();
                
                _rb = GetComponent<Rigidbody2D>();
            }

            public override void Launch(AttackContext ctx)
            {
                base.Launch(ctx);

                colliderEventProducer.TriggerEnter += OnProjectileCollision;

                if (obstacleColliderEventProducer)
                    obstacleColliderEventProducer.TriggerEnter += OnProjectileCollision;
             
                Destroy(gameObject, lifeTime);
            }
            
            private void FixedUpdate()
            {
                if (!IsLaunched) return;

                Vector2 nextPos = _rb.position + AttackContext.Direction * (Speed * Time.fixedDeltaTime);
                _rb.MovePosition(nextPos);
            }
        }
    }
}