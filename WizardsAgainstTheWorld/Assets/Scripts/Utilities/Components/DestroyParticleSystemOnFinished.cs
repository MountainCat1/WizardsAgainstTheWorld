using System;
using UnityEngine;

public class DestroyParticleSystemOnFinished : MonoBehaviour
{
    public event Action ParticleSystemFinished; 
    
    private ParticleSystem _particleSystem;

    private bool _scheduledForDestruction;
    [SerializeField] private float destructionDelay = 0f; // Optional delay before destruction
    
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
        {
            GameLogger.LogError(
                message: "DestroyParticleSystemOnFinished script requires a ParticleSystem component!"
            );
            enabled = false; // Disable the script if no ParticleSystem is found
            return;
        }
    }

    void Update()
    {
        if (!_scheduledForDestruction && !_particleSystem.IsAlive())
        {
            _scheduledForDestruction = true;
            Destroy(gameObject, destructionDelay);
            ParticleSystemFinished?.Invoke();
        }
    }
}