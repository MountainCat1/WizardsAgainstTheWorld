using Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace VictoryConditions
{
    [CreateAssetMenu(fileName = "VictoryCondition", menuName = "Custom/VictoryConditions/Signal")]
    public class SignalCondition : VictoryCondition
    {
        [SerializeField] private Signal signal;
        [SerializeField] private int count = 1;

        [Header("Available variables: \n<count>\n<current_count>")] [SerializeField]
        private string description;

        [Inject] private ISignalManager _signalManager;

        public override string GetDescription()
        {
            var goalCount = _signalManager.GetSignalCount(signal);
            var currentCount = _signalManager.GetSignalCount(signal);

            return descriptionKey.Localize(currentCount, goalCount);
        }

        public override bool Check()
        {
            return _signalManager.GetSignalCount(signal) >= count;
        }
    }
}