using UnityEngine;

namespace ScriptableActions
{
    public class SetParentAction : ScriptableAction
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform parent;
        
        public override void Execute()
        {
            base.Execute();
            target.SetParent(parent);
        }
    }
}