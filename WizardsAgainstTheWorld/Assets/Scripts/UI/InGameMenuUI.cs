using Managers;
using UnityEngine;
using Zenject;

namespace UI
{
    public class InGameMenuUI : MonoBehaviour
    {
        [Inject] private IInGamePauseManager _inGamePauseManager;
        [Inject] private ISoundManager _soundManager;
        
        [SerializeField] private GameObject menuPanel;

        private void Start()
        {
            _inGamePauseManager.OnGamePaused += ShowMenu;
            _inGamePauseManager.OnGameResumed += HideMenu;
            
            if(menuPanel == null)
            {
                GameLogger.LogError("Menu Panel is not assigned in the InGameMenuUI script.");
                return;
            }
            
            menuPanel.SetActive(false); // Ensure the menu is hidden at start
        }

        private void HideMenu()
        {
            menuPanel.SetActive(false);
            _soundManager.ResumeSoundtrack();
        }

        private void ShowMenu()
        {
            menuPanel.SetActive(true);
            _soundManager.PauseSoundtrack();
        }
    }
}