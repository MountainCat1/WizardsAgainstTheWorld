using System;
using UnityEngine;
using Cinemachine;

namespace Managers.Visual
{
    public interface ICameraShakeService
    {
        void ShakeCamera(Vector2 position, float amount);
    }

    public class CameraShakeServiceDisabled : ICameraShakeService
    {
        public void ShakeCamera(Vector2 position, float amount)
        {
            // No operation, camera shake is disabled
            GameLogger.Log("Camera shake is disabled.");
        }        
    }

    public class CameraShakeService : MonoBehaviour, ICameraShakeService
    {
        private CinemachineImpulseSource _impulseSource;
        
        [SerializeField] private float shakeMultiplier = 0.03f;
        [SerializeField] private float shakeDistanceMax = 10f;
        

        private void Start()
        {
            if (Camera.main == null)
            {
                throw new NullReferenceException("Main camera is not found");
            }
            
            _impulseSource = Camera.main.GetComponent<CinemachineImpulseSource>();
        }

        // ShakeCamera method
        public void ShakeCamera(Vector2 position, float amount)
        {
            // Adjust the impulse source position and generate impulse
            var distance = Vector2.Distance(position, Camera.main.transform.position);
            var distanceMultiplier = Mathf.Clamp01(shakeDistanceMax / distance);
            _impulseSource.GenerateImpulse(amount * shakeMultiplier * distanceMultiplier * GameSettings.Instance.Visual.CameraSettings.ShakeIntensity);
            GameLogger.Log($"ShakeCamera: {position} {amount} {distanceMultiplier}");
        }
    }
}