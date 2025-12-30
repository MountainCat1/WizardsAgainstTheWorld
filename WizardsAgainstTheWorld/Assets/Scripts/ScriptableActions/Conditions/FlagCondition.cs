using UnityEngine;
using Zenject;

namespace ScriptableActions.Conditions
{
    public class FlagCondition : ConditionBase
    {
        [Inject] private IFlagManager _flagManager;
        
        [SerializeField] private GameFlag flag;
        
        protected override bool Check()
        {
            return _flagManager.GetFlag(flag) == true;
        }
    }
}