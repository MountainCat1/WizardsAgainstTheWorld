using Managers.Visual;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class ShakeCameraOnStart : MonoBehaviour
    {
        [Inject] private ICameraShakeService _cameraShakeService;

        [SerializeField] private float amount = 1f;
        
        private void Start()
        {
            _cameraShakeService.ShakeCamera(transform.position, amount);
        }
    }
}