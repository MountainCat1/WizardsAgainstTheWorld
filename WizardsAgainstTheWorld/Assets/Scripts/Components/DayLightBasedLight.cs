using System;
using GameplayScene.Managers;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Components
{
    [RequireComponent(typeof(Light2D))]
    public sealed class DayLightBasedLight : MonoBehaviour
    {
        [Inject] private IDayNightManager _dayNightManager;

        [Header("Light Settings")]
        [SerializeField] private float dayIntensity = 1.2f;
        [SerializeField] private float nightIntensity = 0.2f;

        [Tooltip("Seconds before phase end when interpolation begins")]
        [SerializeField] private float transitionDuration = 10f;

        private Light2D _light;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
        }

        private void OnEnable()
        {
            _dayNightManager.OnDayStarted += ApplyDayInstant;
            _dayNightManager.OnNightStarted += ApplyNightInstant;
        }

        private void OnDisable()
        {
            _dayNightManager.OnDayStarted -= ApplyDayInstant;
            _dayNightManager.OnNightStarted -= ApplyNightInstant;
        }

        private void Update()
        {
            float timeLeft = _dayNightManager.TimeTillNextPhase;

            if (timeLeft > transitionDuration)
                return;

            float t = 1f - Mathf.Clamp01(timeLeft / transitionDuration);

            if (_dayNightManager.IsDaytime)
            {
                _light.intensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
            }
            else
            {
                _light.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
            }
        }

        private void ApplyDayInstant()
        {
            _light.intensity = dayIntensity;
        }

        private void ApplyNightInstant()
        {
            _light.intensity = nightIntensity;
        }
    }
}