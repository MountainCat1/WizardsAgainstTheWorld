using Managers;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class SpawnCreatureAction : ScriptableAction
    {
        [Inject] private ICreatureManager _creatureManager;
        
        [SerializeField] private Creature creaturePrefab;
        [SerializeField] private Transform spawnPoint;
        
        public override void Execute()
        {
            base.Execute();
            
            var spawnPosition = spawnPoint.position;
            
            _creatureManager.SpawnCreature(creaturePrefab, spawnPosition);
        }
    }
}