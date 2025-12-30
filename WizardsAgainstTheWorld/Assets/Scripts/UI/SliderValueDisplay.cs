using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SliderValueDisplay : MonoBehaviour  
    {
        [SerializeField] private Slider slider;
        [SerializeField] private string format = "{0}";
        [SerializeField] private bool showAsPercentage = false;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(UpdateText);
                UpdateText(slider.value);
            }
        }

        private void UpdateText(float value)
        {
            if (showAsPercentage)
            {
                int percentage = Mathf.RoundToInt(value * 100f);
                _text.text = string.Format(format, percentage + "%");
            }
            else
            {
                float rounded = Mathf.Round(value * 100f) / 100f; // Round to 2 decimal places
                _text.text = string.Format(format, rounded);
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(UpdateText);
            }
        }
    }
}