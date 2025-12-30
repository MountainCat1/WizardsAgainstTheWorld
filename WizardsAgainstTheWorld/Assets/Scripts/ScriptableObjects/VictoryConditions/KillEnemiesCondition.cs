using System.Linq;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace VictoryConditions
{
    [CreateAssetMenu(fileName = "VictoryCondition", menuName = "Custom/VictoryConditions/KillEnemiesCondition")]
    public class KillEnemiesCondition : VictoryCondition
    {
        [Inject] private ICreatureManager _creatureManager;

        [SerializeField] private Teams[] teamsOfEnemies;
        [Range(0,1)]
        [SerializeField] private float percentageToKill = 0.75f;

        [Header("Available variables: \n<count>\n<current_count>")]
        [SerializeField] private string description = "Have <current_count>/<count> of item <item_name>";

        private int _currentCount;
        private int _goalCount = 0;

        
        [Inject]
        public void Construct([Inject] ICreatureEventProducer eventProducer)
        {
            eventProducer.CreatureDied += OnCreatureDied;
        }

        private void OnCreatureDied(Creature creature, DeathContext deathContext)
        {
            if (creature.Team == Teams.Player)
                return;

            if (teamsOfEnemies.Contains(creature.Team))
            {
                _currentCount++;
            }
        }
        
        public override string GetDescription()
        {
            var goalCount = _goalCount;
            var currentCount = _currentCount;
            
            return descriptionKey.Localize(currentCount, goalCount);
        }

        public override bool Check()
        {
            return _currentCount >= _goalCount;
        }

        public override void Start()
        {
            base.Start();

            var enemyCount = _creatureManager.GetCreatures()
                .Count(x => teamsOfEnemies.Contains(x.Team));
            
            _goalCount = Mathf.CeilToInt(enemyCount * percentageToKill);
        }
    }
}