using UI;
using UI.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MainMenu
{
    [RequireComponent(typeof(PageUI))]
    public class DifficultySettings: MonoBehaviour
    {
        [Inject] private IUIManager _uiManager;
        
        private PageUI _pageUI;
        private GameSettings _gameSettings;
        
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;

        private void Awake()
        {
            _pageUI = GetComponent<PageUI>();
            _pageUI.OnShow += OnShow;
            _gameSettings = GameSettings.Instance;
        }

        private void OnShow()
        {
            _gameSettings = GameSettings.Instance;
            
            easyButton.onClick.AddListener(EasySelected);
            normalButton.onClick.AddListener(NormalSelected);
            hardButton.onClick.AddListener(HardSelected);
        }

        private void HardSelected()
        {
            _gameSettings.Preferences.FriendlyFire = true;
            _gameSettings.Difficulty = 1.2f;
            GameSettings.Update(_gameSettings);
            GameSettings.Save();
            _uiManager.GoBack();
        }

        private void NormalSelected()
        {
            _gameSettings.Preferences.FriendlyFire = false;
            _gameSettings.Difficulty = 0.7f;
            GameSettings.Update(_gameSettings);
            GameSettings.Save();
            _uiManager.GoBack();
        }

        private void EasySelected()
        {
            _gameSettings.Preferences.FriendlyFire = false;
            _gameSettings.Difficulty = 0.4f;
            GameSettings.Update(_gameSettings);
            GameSettings.Save();
            _uiManager.GoBack();
        }
    }
}