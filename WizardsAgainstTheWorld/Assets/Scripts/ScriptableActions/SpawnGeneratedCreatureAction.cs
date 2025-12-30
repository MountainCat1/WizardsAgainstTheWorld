using LevelSelector.Managers;
using Managers;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class SpawnGeneratedCreatureAction : ScriptableAction
    {
        [Inject] private ICrewGenerator _crewGenerator;
        [Inject] private ICreatureManager _creatureManager;
        
        [SerializeField] private Creature creaturePrefab;
        [SerializeField] private Transform spawnPoint;
        
        public override void Execute()
        {
            base.Execute();
            
            var spawnPosition = spawnPoint.position;
            
            var creature = _creatureManager.SpawnCreature(creaturePrefab, spawnPosition);
            creature.Initialize(_crewGenerator.GenerateCrew());
        }
    }
}