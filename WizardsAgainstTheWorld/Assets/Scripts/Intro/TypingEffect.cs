using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TypingEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    [TextArea] [SerializeField] private string fullText;

    private TMP_Text _textComponent;
    private Coroutine _typingCoroutine;

    void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        _textComponent.text = "";
        foreach (char c in fullText)
        {
            _textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}