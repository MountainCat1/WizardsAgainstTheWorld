using System;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Components.Creatures
{
    [RequireComponent(typeof(Creature))]
    public class SimpleCorpseComponent : MonoBehaviour
    {
        [Inject] private ICyclingPoolingManager _poolManager;
        [SerializeField] private SpriteRenderer corpsePrefab;
        [SerializeField] private Sprite corpseSprite;

        private IPoolAccess<SpriteRenderer> _poolAccess;
        private Creature _creature;

        private void Awake()
        {
            _creature = GetComponent<Creature>();
            if (_creature == null)
            {
                Debug.LogError("CorpseComponent requires a Creature component.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            _creature.Health.Death += OnDeath;

            int poolCap = CalculatePoolCapacity(_creature.XpAmount);
            _poolAccess = _poolManager.GetOrCreatePool<SpriteRenderer>(
                $"{_creature.GetIdentifier()}_Corpse", poolCap);
        }

        private static int CalculatePoolCapacity(int xpAmount)
        {
            return Mathf.Max(1, 500 / Mathf.Max(1, xpAmount));
        }

        private void OnDeath(DeathContext ctx)
        {
            if (_poolAccess == null || corpsePrefab == null)
                return;

            var corpse = _poolAccess.SpawnObject(
                corpsePrefab,
                position: ctx.KilledEntity.DisplayTransform.position,
                rotation: 0,
                parent: null);

            corpse.flipX = Random.value < 0.5f; 
            
            if (corpseSprite != null)
            {
                corpse.sprite = corpseSprite; 
            }
            else
            {
                Debug.LogWarning("Corpse sprite is not assigned. Using default sprite.");
            }
        }
    }
}