using System.Collections.Generic;
using Managers;
using UnityEngine;
using Zenject;

namespace GameplayScene.Managers
{
    public interface IEntityFinder
    {
        public IEnumerable<Entity> GetByTeam(Teams team);
        Entity GetClosestEntity(Vector3 transformPosition, Teams enemyTeam);
    }
    
    public class EntityFinder : IEntityFinder
    {
        private readonly Dictionary<Teams, List<Entity>> _entitiesByTeam = new();
        
        [Inject]
        private void Construct(IEntityManager entityManager, IEntityEventProducer entityEventProducer)
        {
            var alreadySpawnedEntities = entityManager.GetEntities();
            foreach (var alreadySpawnedEntity in alreadySpawnedEntities)
            {
                HandleEntitySpawned(alreadySpawnedEntity);
            }
            
            entityManager.EntitySpawned += HandleEntitySpawned;
            entityEventProducer.EntityDied += HandleEntityDespawned;
        }

        public IEnumerable<Entity> GetByTeam(Teams team)
        {
            return _entitiesByTeam.TryGetValue(team, out var entities) ? entities : new List<Entity>();
        }

        public Entity GetClosestEntity(Vector3 transformPosition, Teams enemyTeam)
        {
            Entity closestEntity = null;
            float closestDistanceSqr = float.MaxValue;

            if (_entitiesByTeam.TryGetValue(enemyTeam, out var entities))
            {
                foreach (var entity in entities)
                {
                    var directionToTarget = entity.transform.position - transformPosition;
                    var dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closestEntity = entity;
                    }
                }
            }

            return closestEntity;
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (!_entitiesByTeam.ContainsKey(entity.Team))
            {
                _entitiesByTeam[entity.Team] = new List<Entity>();
            }
            _entitiesByTeam[entity.Team].Add(entity);
        }
        
        private void HandleEntityDespawned(Entity entity, DeathContext ctx)
        {
            if (_entitiesByTeam.ContainsKey(entity.Team))
            {
                _entitiesByTeam[entity.Team].Remove(entity);
            }
        }
    }


}