using Triggers;
using UnityEngine;

namespace ScriptableActions.Conditions
{
    public class TriggerRunCondition : ConditionBase
    {
        [SerializeField] private TriggerBase trigger;
        
        protected override bool Check()
        {
            return trigger.HasFired;
        }
    }
}