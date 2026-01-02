using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using Managers;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ICreatureManager
    {
        event Action<Creature> CreatureSpawned;
        bool IsAliveAndActive(Creature creature);
        ICollection<Creature> GetCreatures();
        IEnumerable<Creature> GetCreaturesAliveActive();
        IEnumerable<Creature> GetAliveCreatures();
        public Creature SpawnCreature(Creature creaturePrefab, Vector3 position, Transform parent = null);
        public Creature SpawnCreature(Creature creaturePrefab, Vector2Int position, Transform parent = null)
            => SpawnCreature(creaturePrefab, (Vector2)position, parent);
        ICollection<Creature> PlayerCreatures { get; }
    }
    
    public class CreatureManager : MonoBehaviour, ICreatureManager
    {
        [Inject] private IEntityManager _entityManager;
        [Inject] private ITeamManager _teamManager;
        
        public event Action<Creature> CreatureSpawned;

        private void Start()
        {
            _entityManager.EntitySpawned += OnEntitySpawned;

            foreach (var spawnedCreature in _entityManager.GetEntities().OfType<Creature>())
            {
                OnEntitySpawned(spawnedCreature);
            }
        }
        
        public bool IsAliveAndActive(Creature creature)
        {
            return _entityManager.IsAliveAndActive(creature);
        }

        public ICollection<Creature> GetCreatures()
        {
            return _entityManager.GetEntities().OfType<Creature>().ToArray();
        }

        public IEnumerable<Creature> GetCreaturesAliveActive()
        {
            return _entityManager.GetEntities().OfType<Creature>()
                .Where(c => _entityManager.IsAliveAndActive(c));
        }

        public IEnumerable<Creature> GetAliveCreatures()
        {
            return _entityManager.GetAliveEntities().OfType<Creature>();
        }

        public Creature SpawnCreature(Creature creaturePrefab, Vector3 position, Transform parent = null)
        {
            return _entityManager.SpawnEntity(creaturePrefab, position, parent) as Creature;
        }

        public ICollection<Creature> PlayerCreatures => _entityManager.PlayerEntities.OfType<Creature>().ToArray();
        
        
        // Callbacks
        private void OnEntitySpawned(Entity obj)
        {
            var creature = obj as Creature;
            if (creature is null)
                return;
            
            if (_teamManager.GetAttitude(creature.Team, Teams.Player) == Attitude.Hostile)
            {
                DifficultyApplier.ApplyDifficulty(creature);
            }
        }
    }
}