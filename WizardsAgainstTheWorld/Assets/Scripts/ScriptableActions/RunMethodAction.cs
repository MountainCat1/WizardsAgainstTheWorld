using UnityEngine;
using UnityEngine.Events;

namespace ScriptableActions
{
    public class RunMethodAction : ScriptableAction
    {
        [SerializeField] private UnityEvent methodToRun;
        
        public override void Execute()
        {
            base.Execute();
            
            if (methodToRun == null)
            {
                GameLogger.LogError("RunMethodAction: methodToRun is not assigned.");
                return;
            }
            
            try
            {
                methodToRun.Invoke();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"RunMethodAction: Error invoking method - {ex.Message}");
            }
        }
    }
}