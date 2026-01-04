using System;
using UI;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface IInGamePauseManager
    {
        event Action OnGameResumed;
        event Action OnGamePaused;
    }

    public class InGamePauseManager : MonoBehaviour, IInGamePauseManager
    {
        public event Action OnGameResumed;
        public event Action OnGamePaused;
        
        [Inject] private IInputManager _inputManager;
        [Inject] private ITimeManager _timeManager;
        [Inject] private IUIInteractionStack _uiInteractionStack;

        private bool _isPaused;
        
        private void OnEnable()
        {
            _inputManager.TogglePause += TogglePause;
        }
        
        private void OnDisable()
        {
            _inputManager.TogglePause -= TogglePause;
        }

        private void OnDestroy()
        {
            _inputManager.TogglePause -= TogglePause;
        }

        private void TogglePause()
        {
            if(_uiInteractionStack.IsBlocked())
                return;
            
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            _isPaused = true;
            _timeManager.AddTimeScaleChange(TimeScaleModifier.Pause, 0f); 
            OnGamePaused?.Invoke();
        }

        private void ResumeGame()
        {
            _isPaused = false;
            _timeManager.RemoveTimeScaleChange(TimeScaleModifier.Pause); 
            OnGameResumed?.Invoke();
        }
    }
}