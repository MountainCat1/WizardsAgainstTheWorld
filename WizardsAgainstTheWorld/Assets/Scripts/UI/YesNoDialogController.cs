using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public interface IYesNoDialogController
    {
        void Show(string message, Action onYes = null, Action onNo = null);
    }

    public class YesNoDialogController : MonoBehaviour, IYesNoDialogController
    {
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action _onYes;
        private Action _onNo;

        private void Start()
        {
            dialogPanel.SetActive(false);
        }

        public void Show(string message, Action onYes = null, Action onNo = null)
        {
            messageText.text = message;
            this._onYes = onYes;
            this._onNo = onNo;

            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();

            yesButton.onClick.AddListener(() =>
            {
                this._onYes?.Invoke();
                Close();
            });

            noButton.onClick.AddListener(() =>
            {
                this._onNo?.Invoke();
                Close();
            });

            dialogPanel.SetActive(true);
        }

        private void Close()
        {
            dialogPanel.SetActive(false);
        }
    }
}