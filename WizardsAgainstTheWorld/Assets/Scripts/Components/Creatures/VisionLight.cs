using Managers;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

// This script represents light sources that are basically used as vision in game
[RequireComponent(typeof(Light2D))]
public class VisionLight : MonoBehaviour
{
    [Inject] private IVisionManager _visionManager;

    [SerializeField] private Creature creature;

    private Light2D _light2D;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
    }

    private void Start()
    {
        _visionManager.Changed += OnVisionChanged;
        OnVisionChanged();
    }

    private void OnVisionChanged()
    {
        _light2D.pointLightInnerRadius = creature.SightRange * 0.5f * _visionManager.VisionRangeMultiplier;
        _light2D.pointLightOuterRadius = creature.SightRange * _visionManager.VisionRangeMultiplier;
    }
}