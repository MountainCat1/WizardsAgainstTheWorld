using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Utilities;

public class SlowTypingOnAwake : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;

    [SerializeField] [TextArea] private string fullText;
    [SerializeField] private string textKey;
    private Coroutine _typingCoroutine;

    private void OnEnable()
    {
        StartTyping();
    }

    public void StartTyping()
    {
        textComponent.text = "";
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textComponent.text = "";
        Regex pauseRegex = new Regex(@"\[PAUSE=(\d+(\.\d*)?)\]");

        int lastIndex = 0;

        var localizedText = textKey.Localize(textKey);
        
        foreach (Match match in pauseRegex.Matches(localizedText))
        {
            string textBeforePause = localizedText.Substring(lastIndex, match.Index - lastIndex);
            yield return TypeCharacters(textBeforePause);

            float pauseTime = float.Parse(match.Groups[1].Value);
            yield return new WaitForSeconds(pauseTime);

            lastIndex = match.Index + match.Length;
        }

        // Type remaining text after last pause
        yield return TypeCharacters(localizedText.Substring(lastIndex));
    }

    private IEnumerator TypeCharacters(string text)
    {
        foreach (char c in text)
        {
            textComponent.text += c;

            // Skip delay for spaces
            if (char.IsWhiteSpace(c))
                continue;

            yield return new WaitForSeconds(typingSpeed);
        }
    }

}