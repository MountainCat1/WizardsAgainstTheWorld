using Managers;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class SendSignalAction : ScriptableAction
    {
        [SerializeField] private Signal signal;
        
        [Inject] private ISignalManager _signalManager;
        
        public override void Execute()
        {
            base.Execute();
            
            _signalManager.Signal(signal);
        }
    }
}