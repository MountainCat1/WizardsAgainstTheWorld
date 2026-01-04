using GameplayScene.Managers;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameplayScene.UI
{
    public class DayNightUI : MonoBehaviour
    {
        [Inject] private IDayNightManager _dayNightManager;
        
        [SerializeField] private TMP_Text timeTillNextPhaseText;
        [SerializeField] private Color dayColor = Color.greenYellow;
        [SerializeField] private Color nightColor = Color.darkRed;

        private void Start()
        {
            _dayNightManager.OnDayStarted += OnDayStarted;
            _dayNightManager.OnNightStarted += OnNightStarted;
        }

        private void OnNightStarted()
        {
            timeTillNextPhaseText.color = nightColor;
        }

        private void OnDayStarted()
        {
            timeTillNextPhaseText.color = dayColor;
        }

        void Update()
        {
            float timeTillNextPhase = _dayNightManager.TimeTillNextPhase;
            timeTillNextPhaseText.text = $"{timeTillNextPhase:F}";
        }
    }
}