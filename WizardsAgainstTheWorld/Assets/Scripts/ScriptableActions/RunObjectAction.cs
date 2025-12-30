using System.Collections;
using System.Linq;
using UnityEngine;

namespace ScriptableActions
{
    public class RunObjectAction : ScriptableAction
    {
        [SerializeField] private GameObject objectToRun;
        [SerializeField] private float delay = 0f;
        
        public override void Execute()
        {
            base.Execute();
            
            StartCoroutine(DelayedRun());
        }

        private IEnumerator DelayedRun()
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            var actions = objectToRun.GetComponents<ScriptableAction>();

            foreach (var action in actions.Where(x => x != this))
            {
                action.Execute();
            }
        }
    }
}