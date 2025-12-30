using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers.Visual
{
    public interface IBloodManager
    {
        void SpawnBlood(Vector3 position, Vector3 direction, float amount);
    }

    public class BloodManager : MonoBehaviour, IBloodManager
    {
        [Inject] private ICreatureManager _creatureManager;

        [SerializeField] private GameObject bloodPrefab;
        [SerializeField] private GameObject bloodParticlesPrefab;
        
        [SerializeField] private int bloodPoolSize = 100;
        [SerializeField] private const float PixelsPerUnit = 16;
        private float GridSize => 1 / PixelsPerUnit;

        private Queue<GameObject> _bloodPool = new Queue<GameObject>();

        private void Start()
        {
            _creatureManager.CreatureSpawned += OnCreatureSpawned;
            foreach (var creature in _creatureManager.GetCreatures())
            {
                OnCreatureSpawned(creature);
            }

            for (int i = 0; i < bloodPoolSize; i++)
            {
                var bloodGo = Instantiate(bloodPrefab, Vector3.zero, Quaternion.identity, transform);
                bloodGo.SetActive(false);
                _bloodPool.Enqueue(bloodGo);
            }
        }

        private void OnCreatureSpawned(Creature creature)
        {
            creature.Health.Hit += OnCreatureHit;
        }

        private void OnCreatureHit(HitContext ctx)
        {
            var position = ctx.Target?.Health.transform.position ?? ctx.TargetPosition;
            
            if(ctx.Target is Creature targetCreature)
            {
                position = targetCreature.DisplayTransform.position;
            }
            
            SpawnBlood(position, Vector3.zero, 
                ctx.Damage);
        }

        public void SpawnBlood(Vector3 position, Vector3 direction, float amount)
        {
            if (_bloodPool.Count == 0)
            {
                return;
            }
            Vector2 snappedPosition = GridSnappingUtility.SnapToGrid(position, GridSize);


            var bloodGo = _bloodPool.Dequeue();
            bloodGo.transform.position = snappedPosition;
            bloodGo.SetActive(true);
            _bloodPool.Enqueue(bloodGo);
            
            Instantiate(bloodParticlesPrefab, snappedPosition, Quaternion.identity);
        }
    }
}