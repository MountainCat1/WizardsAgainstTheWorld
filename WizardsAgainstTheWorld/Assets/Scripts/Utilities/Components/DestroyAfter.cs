using UnityEngine;

namespace DefaultNamespace
{
    public class DestroyAfter : MonoBehaviour
    {
        [SerializeField] private float delay = 1f;

        private void Start()
        {
            if (delay <= 0)
            {
                GameLogger.LogWarning("DestroyAfter delay is set to 0 or negative, object will not be destroyed.");
                return;
            }
            
            Destroy(gameObject, delay);
        }
    }
}