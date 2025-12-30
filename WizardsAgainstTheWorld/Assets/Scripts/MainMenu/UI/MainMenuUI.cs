using Data;
using Managers;
using UI.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Zenject;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Inject] private IDataManager _dataManager;
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private IYesNoDialogController _yesNoDialogController;
        [Inject] private IUIManager _uiManager;

        [SerializeField] private SceneReference tutorialScene;
        [SerializeField] private TutorialStartup tutorialStartup;

        [SerializeField] private Button loadGameButton;
        
        [SerializeField] private PageUI difficultySelectionPage;

        private void Start()
        {
            if (loadGameButton)
            {
                loadGameButton.interactable = _dataManager.LoadData() != null;
            }

            if (GameSettings.Instance.DisplayDifficultySelection)
            {
                GameSettings.Instance.DisplayDifficultySelection = false;
                GameSettings.Update(GameSettings.Instance);
                GameSettings.Save();
                _uiManager.Show(difficultySelectionPage);
            }
        }

        public void LoadGame()
        {
            _sceneLoader.LoadScene(Scenes.LevelSelector);
        }

        public void StartNewGame()
        {
            // If there is no other save, just start the game
            if (!_dataManager.HasData())
            {
                _dataManager.DeleteData();
                _sceneLoader.LoadScene(Scenes.LevelSelector);
                return;
            }

            // Otherwise, show a confirmation dialog
            _yesNoDialogController.Show(
                message: LocalizationHelper.L("UI.MainMenu.Dialogue.NewGameConfirm"),
                onYes: () =>
                {
                    _dataManager.DeleteData();
                    _sceneLoader.LoadScene(Scenes.LevelSelector);
                },
                onNo: () => { }
            );
        }

        public void GoToTheMenu()
        {
            _sceneLoader.LoadScene(Scenes.MainMenu);
        }

        public void QuitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void LaunchTutorial()
        {
            tutorialStartup.LaunchTutorial();
        }
    }
}