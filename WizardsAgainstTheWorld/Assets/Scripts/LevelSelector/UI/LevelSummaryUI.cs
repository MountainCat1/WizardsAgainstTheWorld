using System.Linq;
using LevelSelector.Managers;
using Managers;
using TMPro;
using UnityEngine;
using Zenject;
using static Utilities.LocalizationHelper;

namespace LevelSelector.UI
{
    public class LevelSummaryUI : MonoBehaviour
    {
        [Inject] private IPostLevelHandler _postLevelHandler;
        [Inject] private IInputManager _inputManager;
        [Inject] private IGameEventManager _gameEventManager;
        [Inject] private IItemManager _itemManager;

        [SerializeField] private TMP_Text summaryText;

        private void Start()
        {
            _inputManager.UI.GoBack += OnGoBack;

            gameObject.SetActive(false);

            _postLevelHandler.SummaryGenerated += OnSummaryGenerated;
            if (_postLevelHandler.GeneratedSummary != null)
            {
                OnSummaryGenerated(_postLevelHandler.GeneratedSummary);
            }

            _gameEventManager.EventTriggered += OnGameEventTriggered;
        }

        private void OnDestroy()
        {
            _inputManager.UI.GoBack -= OnGoBack;
            _postLevelHandler.SummaryGenerated -= OnSummaryGenerated;
            _gameEventManager.EventTriggered -= OnGameEventTriggered;
        }

        private void OnGameEventTriggered(GameEvent obj)
        {
            Hide();
        }

        private void OnGoBack()
        {
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnSummaryGenerated(Summary generatedSummary)
        {
            if (!generatedSummary.SummaryRows.Any())
            {
                GameLogger.LogWarning("Summary is empty, nothing to display.");
                Hide();
                return;
            }

            summaryText.text = string.Empty;

            gameObject.SetActive(true);

            foreach (var summaryRow in generatedSummary.SummaryRows)
            {
                var type = summaryRow.Key.Item1;
                var id = summaryRow.Key.Item2;
                var value = summaryRow.Value;

                switch (type)
                {
                    case Summary.Type.AutoSell:
                        var item = _itemManager.GetItemPrefab(id);
                        summaryText.text += L("UI.PostGameSummary.AutoSell", L(item.NameKey), value) + "\n";
                        break;
                    case Summary.Type.TotalProfit:
                        summaryText.text += "\n" + L("UI.PostGameSummary.TotalProfit", value) + "\n";
                        break;
                    default:
                        GameLogger.LogError($"Unknown summary type: {type}");
                        break;
                }
            }
        }
    }
}