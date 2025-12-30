using UnityEngine;

namespace ScriptableActions
{
    public class DestroyObjectAction : ScriptableAction
    {
        [SerializeField] private GameObject target;
        
        public override void Execute()
        {
            base.Execute();
            Destroy(target);
        }
    }
}