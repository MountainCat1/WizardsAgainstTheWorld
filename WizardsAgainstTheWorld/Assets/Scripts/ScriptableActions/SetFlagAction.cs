using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class SetFlagAction : ScriptableAction
    {
        [Inject] private IFlagManager _flagManager;
        
        [SerializeField] private GameFlag flag = GameFlag.None;
        [SerializeField] private bool value = true;
        public override void Execute()
        {
            base.Execute();
            
            _flagManager.SetFlag(flag, value);
        }
    }
}