using System.Collections;
using UnityEngine;

namespace CreatureControllers
{
    public class AiController : EntityController
    {
        private Coroutine _thinkCoroutine;
        
        protected virtual void Start()
        {
            _thinkCoroutine = StartCoroutine(ThinkCoroutine());
        }

        private IEnumerator ThinkCoroutine()
        {
            while (true)
            {
                Think();
                yield return new WaitForEndOfFrame();
            }
        }

        protected virtual void Think()
        {
        }
    }
}