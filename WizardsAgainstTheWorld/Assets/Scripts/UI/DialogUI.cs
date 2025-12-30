using System.Collections;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using Zenject;

public class DialogUI : MonoBehaviour
{
    [Inject] private IGameConfiguration _configuration;

    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Color defaultDialogColor;
    [SerializeField] private FontStyles defaultDialogFontStyle;
    
    [SerializeField] private Color selfTalkColor;
    [SerializeField] private FontStyles selfTalkFontStyle;

    public bool Typing { get; private set; } = false;

    private Coroutine _typingCoroutine;
    private bool _dialogPanelShown;

    private bool _speedUp = false;

    private void Start()
    {
        dialogPanel.SetActive(false);
    }

    public void ShowSentence(DialogSentence sentence)
    {
        _speedUp = false;

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeSentenceCoroutine(sentence.Text));

        if (sentence.Type == DialogType.Default)
        {
            dialogText.color = defaultDialogColor;
            dialogText.fontStyle = defaultDialogFontStyle;
        }
        else
        {
            dialogText.color = selfTalkColor;
            dialogText.fontStyle = selfTalkFontStyle;
        }
    }

    private IEnumerator TypeSentenceCoroutine(string sentence)
    {
        Typing = true;

        dialogText.text = "";
        foreach (var letter in sentence)
        {
            dialogText.text += letter;

            if (_speedUp)
                yield return new WaitForSeconds(_configuration.FastTypingDelay);
            else
                yield return new WaitForSeconds(_configuration.TypingDelay);
        }

        Typing = false;
    }

    public void SpeedUp()
    {
        _speedUp = true;
    }

    public void HideDialogPanel(bool force = false)
    {
        if (!_dialogPanelShown && !force)
            return;

        dialogPanel.SetActive(false);
        _dialogPanelShown = false;
    }

    public void ShowDialogPanel()
    {
        if (_dialogPanelShown)
            return;

        dialogPanel.SetActive(true);
        _dialogPanelShown = true;
    }
}