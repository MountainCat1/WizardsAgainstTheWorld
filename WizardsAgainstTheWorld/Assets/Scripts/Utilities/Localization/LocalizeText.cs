using TMPro;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizeText : MonoBehaviour
    {
        private TMP_Text _text;

        private string _key;

        [Tooltip(
            "If true, the text will not be localized on Awake. Mostly useful in situations where you would set the text value via other script and the enable the object. Coz in this case the method Start() would be called and override it with localization based on the text value"
        )]
        [SerializeField]
        private bool doNotLocalizeOnAwake = true;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (_text == null)
            {
                Debug.LogError("TMP_Text component not found on LocalizeText.");
                return;
            }

            // Get the key from the TMP_Text component
            _key = _text.text.Trim();
        }

        private void Start()
        {
            if (!doNotLocalizeOnAwake)
            {
                // Ensure the text is updated at the start
                UpdateText();
                CsvLocalizationManager.Instance.LanguageChanged += UpdateText;
            }
            else
            {
                CsvLocalizationManager.Instance.FontChanged += UpdateFont;
                UpdateFont();
            }

            // Subscribe to language change event
        }

        private void UpdateFont()
        {
            if (CsvLocalizationManager.Instance != null)
            {
                _text.font = CsvLocalizationManager.Instance.CurrentFontAsset;
            }
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
            string localizedText = LocalizationHelper.L(_key);

            if (localizedText == null)
            {
                Debug.LogWarning($"No localization found for key: {_key}");
                return;
            }

            // Update the TMP_Text component with the localized text
            _text.text = localizedText;

            UpdateFont();
        }
    }
}