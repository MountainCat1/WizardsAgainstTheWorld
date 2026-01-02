using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface IEntityManager
    {
        event Action<Entity> EntitySpawned;
        bool IsAliveAndActive(Entity entity);
        ICollection<Entity> GetEntitys();
        IEnumerable<Entity> GetEntitysAliveActive();
        IEnumerable<Entity> GetAliveEntitys();
        public Entity SpawnEntity(Entity entityPrefab, Vector3 position, Transform parent = null);
        public Entity SpawnEntity(Entity entityPrefab, Vector2Int position, Transform parent = null)
            => SpawnEntity(entityPrefab, (Vector2)position, parent);
        public void ScanForEntitys();
        ICollection<Entity> PlayerEntities { get; }
    }

    public class EntityManager : MonoBehaviour, IEntityManager
    {
        public event Action<Entity> EntitySpawned;

        [Inject] private DiContainer _diContainer;
        [Inject] private ISpawnerManager _spawnerManager;
        [Inject] private ITeamManager _teamManager;

        public ICollection<Entity> PlayerEntities { get; private set; } = new List<Entity>();
        
        private List<Entity> _entitys = new List<Entity>();
        private List<Entity> _visibleEntitys = new List<Entity>();

        private void Start()
        {
            var preSpawnedEntitys = FindObjectsOfType<Entity>();
            foreach (var entity in preSpawnedEntitys)
            {
                _diContainer.Inject(entity.gameObject);

                HandleNewEntity(entity);
            }
            
            
        }
        
        public void ScanForEntitys()
        {
            var preSpawnedEntitys = FindObjectsOfType<Entity>();
            foreach (var entity in preSpawnedEntitys)
            {
                if (!_entitys.Contains(entity))
                {
                    HandleNewEntity(entity);
                }
            }
        }

        public IEnumerable<Entity> GetEntitysAliveActive()
        {
            return _entitys.Where(x => x.Health.Alive && x.gameObject.activeInHierarchy);
        }

        public IEnumerable<Entity> GetAliveEntitys()
        {
            return _entitys.Where(x => x.Health.Alive && x.gameObject.activeInHierarchy);
        }
        
        public Entity SpawnEntity(Entity entityPrefab, Vector3 position, Transform parent = null)
        {
            var entity = _spawnerManager.Spawn(
                prefab: entityPrefab,
                position: position
            );
            
            entity.Original = entityPrefab;

            HandleNewEntity(entity);

            return entity;
        }

        private void HandleNewEntity(Entity entity)
        {
            _entitys.Add(entity);

            entity.Health.Death += (DeathContext ctx) => { _entitys.Remove(entity); };
            
            if (entity.Team == Teams.Player)
            {
                PlayerEntities.Add(entity);
                entity.Health.Death += (DeathContext ctx) =>
                {
                    PlayerEntities.Remove(entity);
                };
            }
            
            EntitySpawned?.Invoke(entity);
        }

        public bool IsAliveAndActive(Entity entity)
        {
            return entity.Health.Alive && entity.gameObject.activeInHierarchy;
        }

        public ICollection<Entity> GetEntitys()
        {
            return _entitys;
        }
    }
}