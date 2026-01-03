using System;
using System.Collections;
using UnityEngine;

namespace GameplayScene.Managers
{
    public interface IDayNightManager
    {
        public int CurrentDay { get; }
        public float TimeTillNextPhase { get; }
        public bool IsDaytime { get; }
        public float DayDuration { get; }
        public float NightDuration { get; }
        
        event Action DayNightPhaseChanged;
        event Action OnDayStarted;
        event Action OnNightStarted;
        
        void Initialize();
    }
    
    public class DayNightManager : MonoBehaviour, IDayNightManager
    {
        // Events
        public event Action DayNightPhaseChanged;
        public event Action OnDayStarted;
        public event Action OnNightStarted;

        // Public Properties
        public int CurrentDay { get; private set; } = 1;
        public float TimeTillNextPhase { get; private set; }
        public bool IsDaytime { get; private set; }
        
        // Settings
        [field: SerializeField] public float DayDuration { get; private set; } = 30f; // 30s
        [field: SerializeField] public float NightDuration { get; private set; } = 60f; // 60s

        // Private Fields
        private Coroutine _dayNightCycleCoroutine;
        
        public void Initialize()
        {
            _dayNightCycleCoroutine = StartCoroutine(DayNightCycleCoroutine());
        }

        private IEnumerator DayNightCycleCoroutine()
        {
            while (true)
            {
                // Daytime
                IsDaytime = true;
                TimeTillNextPhase = DayDuration;
                DayNightPhaseChanged?.Invoke();
                OnDayStarted?.Invoke();

                while (TimeTillNextPhase > 0)
                {
                    yield return null;
                    TimeTillNextPhase -= Time.deltaTime;
                }

                // Nighttime
                IsDaytime = false;
                TimeTillNextPhase = NightDuration;
                DayNightPhaseChanged?.Invoke();
                OnNightStarted?.Invoke();

                while (TimeTillNextPhase > 0)
                {
                    yield return null;
                    TimeTillNextPhase -= Time.deltaTime;
                }

                CurrentDay++;
            }
        }

    }
}