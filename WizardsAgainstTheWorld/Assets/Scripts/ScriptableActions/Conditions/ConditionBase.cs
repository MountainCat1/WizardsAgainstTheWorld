using UnityEngine;

namespace ScriptableActions.Conditions
{
    public abstract class ConditionBase : MonoBehaviour
    {
        [field: SerializeField] public bool Negate { get; private set; } = false;
        
        protected abstract bool Check();
        
        public bool Evaluate()
        {
            var result = Check();
            return Negate ? !result : result;
        }
    }
}