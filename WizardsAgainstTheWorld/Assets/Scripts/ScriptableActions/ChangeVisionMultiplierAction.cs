using Managers;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class ChangeVisionMultiplierAction : ScriptableAction
    {
        [Inject] private IVisionManager _visionManager;
        
        [SerializeField] private float changeAmount = 0.1f;
        
        public override void Execute()
        {
            base.Execute();
            
            _visionManager.VisionRangeMultiplier += changeAmount;
        }
    }
}