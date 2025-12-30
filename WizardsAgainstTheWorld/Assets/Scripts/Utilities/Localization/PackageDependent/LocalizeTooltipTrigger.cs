using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Utilities
{
    [RequireComponent(typeof(TooltipTrigger))]
    public class LocalizeTooltipTrigger : MonoBehaviour
    {
        private TooltipTrigger _tooltipTrigger;

        private string _key;

        private void Awake()
        {
            _tooltipTrigger = GetComponent<TooltipTrigger>();
            if (_tooltipTrigger == null)
            {
                Debug.LogError("TooltipTrigger component not found on LocalizeTooltipTrigger.");
                return;
            }

            // Get the key from the TMP_Text component
            _key = _tooltipTrigger.text.Trim();
        }

        private void Start()
        {
            // Ensure the text is updated at the start
            UpdateText();

            // Subscribe to language change event
            CsvLocalizationManager.Instance.LanguageChanged += UpdateText;
        }


        private void OnDestroy()
        {
            if (CsvLocalizationManager.Instance != null)
            {
                CsvLocalizationManager.Instance.LanguageChanged -= UpdateText;
            }
        }

        public void UpdateText()
        {
            if (string.IsNullOrWhiteSpace(_key))
            {
                Debug.LogWarning("Localization key is null or empty.");
                return;
            }

            // Get the localized text using the key
            string localizedText = CsvLocalizationManager.Instance.Get(_key);

            if (localizedText == null)
            {
                Debug.LogWarning($"No localization found for key: {_key}");
                return;
            }

            // Update the TMP_Text component with the localized text
            _tooltipTrigger.text = localizedText;
        }
    }
}