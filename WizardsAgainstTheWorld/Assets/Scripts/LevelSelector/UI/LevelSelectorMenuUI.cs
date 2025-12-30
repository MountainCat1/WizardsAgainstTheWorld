using UI;
using UI.Abstractions;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace LevelSelector.UI
{
    public class LevelSelectorMenuUI : MonoBehaviour
    {
        [Inject] private IInputManager _inputManager;
        [Inject] private ILevelSelectorSlideManagerUI _levelSelectorSlideManager;
        [Inject] private IUIManager _uiManager;

        [SerializeField] private PageManager panelManager;
        // [SerializeField] private Volume postProcessingVolume;

        private void Start()
        {
            gameObject.SetActive(false);
            
            _uiManager.WentBack += OnGoBack;
        }

        private void OnGoBack(IClosableUI uiElement)
        {
            if (gameObject.activeSelf)
            {
                if ((PageUI)uiElement == panelManager.InitialPage)
                {
                    gameObject.SetActive(false);
                }
                
                return;
            }

            if (uiElement == null)
            {
                gameObject.SetActive(true);
                panelManager.ShowPage(panelManager.InitialPage);               
            }
        }
    }
}