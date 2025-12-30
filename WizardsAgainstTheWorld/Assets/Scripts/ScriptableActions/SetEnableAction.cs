using UnityEngine;

namespace ScriptableActions
{
    public class SetEnableAction : ScriptableAction
    {
        [SerializeField] private GameObject target;
        [SerializeField] private bool enable = true;
        
        public override void Execute()
        {
            base.Execute();
            target.SetActive(enable);
        }
    }
}