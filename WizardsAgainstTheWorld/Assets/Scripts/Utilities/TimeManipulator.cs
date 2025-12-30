using UnityEngine;

namespace Utilities
{
    public class TimeManipulator : MonoBehaviour
    {
        [SerializeField] private float timeScale = 1f;
        
        private void Update()
        {
            Time.timeScale = timeScale;
        }
        
    }
}