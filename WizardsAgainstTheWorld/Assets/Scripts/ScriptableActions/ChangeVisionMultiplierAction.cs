using UnityEngine;

namespace ScriptableActions
{
    public class ChangeVisionMultiplierAction : ScriptableAction
    {
        [SerializeField] private float changeAmount = 0.1f;
        
        public override void Execute()
        {
            base.Execute();
        }
    }
}