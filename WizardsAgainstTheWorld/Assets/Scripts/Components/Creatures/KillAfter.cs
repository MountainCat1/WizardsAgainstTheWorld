using System.Collections;
using UnityEngine;

namespace Components.Creatures
{
    public class KillAfter : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private float delay = 1f;

        private void Start()
        {
            if (delay <= 0)
            {
                GameLogger.LogWarning("DestroyAfter delay is set to 0 or negative, object will not be destroyed.");
                return;
            }

            if (health == null)
            {
                GameLogger.LogError("HealthComponent is not assigned in KillAfter component.");
                return;
            }
            
            StartCoroutine(WaitToKill());
        }

        private IEnumerator WaitToKill()
        {
            yield return new WaitForSeconds(delay);
            
            health.Kill();
        }
    }
}