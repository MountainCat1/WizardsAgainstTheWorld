using UnityEngine;
using TMPro;

namespace UI
{
    public class TextWordWrapperUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private int maxCharactersPerLine = 30;

        public void SetText(string text)
        {
            textComponent.text = InsertLineBreaks(text, maxCharactersPerLine);
        }

        private string InsertLineBreaks(string text, int maxChars)
        {
            string[] words = text.Split(' ');
            string result = "";
            string currentLine = "";

            foreach (string word in words)
            {
                if (currentLine.Length + word.Length > maxChars)
                {
                    result += currentLine.TrimEnd() + "\n"; // Add line break
                    currentLine = ""; // Reset line
                }

                currentLine += word + " ";
            }

            result += currentLine.TrimEnd(); // Add last line

            return result;
        }
    }
}