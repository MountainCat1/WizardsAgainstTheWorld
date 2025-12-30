using TMPro;
using UnityEngine;

namespace Utilities
{
    public class LocalizedText : TextMeshProUGUI
    {
        private string _key;

        protected override void Awake()
        {
            base.Awake();

            if(!Application.isPlaying)
                return;
            
            // Get the key from the TMP_Text component
            _key = text.Trim();
        }

        protected override void Start()
        {
            base.Start();
            
            if(!Application.isPlaying)
                return;

            // Ensure the text is updated at the start
            UpdateText();

            // Subscribe to language change event
            CsvLocalizationManager.Instance.LanguageChanged += UpdateText;
        }

        protected override void OnDestroy()
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
            string localizedText = LocalizationHelper.L(_key);

            if (localizedText == null)
            {
                Debug.LogWarning($"No localization found for key: {_key}");
                return;
            }

            // Update the TMP_Text component with the localized text
            text = localizedText;
        }
    }
}