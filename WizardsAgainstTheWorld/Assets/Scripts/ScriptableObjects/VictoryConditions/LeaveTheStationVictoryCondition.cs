using System.Linq;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace VictoryConditions
{
    [CreateAssetMenu(fileName = "VictoryCondition", menuName = "Custom/VictoryConditions/LeaveTheStation")]
    public class LeaveTheStationVictoryCondition : VictoryCondition
    {
        [Header("Available variables: \n<count>")] [SerializeField]
        private string description;

        [Inject] private ICreatureManager _creatureManager;
        [Inject] private ISignalManager _signalManager;

        public override string GetDescription()
        {
            return descriptionKey.Localize(GetCreatureCount());
        }

        public override bool Check()
        {
            return GetCreatureCount() == 0;
        }

        private int GetCreatureCount()
        {
            return _creatureManager
                .GetCreaturesAliveActive()
                .Count(creature => creature.Health.CurrentValue > 0 && creature.gameObject.activeInHierarchy &&
                                   creature.Team == Teams.Player);
        }
    }
}