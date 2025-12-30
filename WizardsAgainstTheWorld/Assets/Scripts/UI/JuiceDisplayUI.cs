using Managers;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class JuiceDisplayUI : MonoBehaviour
    {
        [Inject] IJuiceManager _juiceManager;
        
        [SerializeField] private TextMeshProUGUI juiceText;

        [SerializeField] private Color okColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color alertColor = Color.red;
        [SerializeField] private GameObject warning;
        [SerializeField] private float warningAmount = 35f;
        [SerializeField] private float alertAmount = 10f;
        
        private void Start()
        {
            _juiceManager.JuiceChanged += OnJuiceChanged;
            OnJuiceChanged();
        }

        private void OnJuiceChanged()
        {
            if (!GameSettings.Instance.Preferences.UseJuiceMechanic)
            {
                juiceText.gameObject.SetActive(false);
                warning.SetActive(false);
                return;
            }
            else
            {
                juiceText.gameObject.SetActive(true);
            }
            
            juiceText.text = $"{_juiceManager.Juice:F2}";
            
            if ((float)_juiceManager.Juice / _juiceManager.ConsumptionRate > warningAmount)
            {
                juiceText.color = okColor;
            }
            else if ((float)_juiceManager.Juice / _juiceManager.ConsumptionRate > alertAmount)
            {
                juiceText.color = warningColor;
            }
            else
            {
                juiceText.color = alertColor;
            }
            
            warning.SetActive((float)_juiceManager.Juice / _juiceManager.ConsumptionRate <= alertAmount);
        }
    }
}