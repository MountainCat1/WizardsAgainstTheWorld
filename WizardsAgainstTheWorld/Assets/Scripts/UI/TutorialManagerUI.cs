using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using Utilities;
using Zenject;

namespace UI
{
    public class TutorialManagerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tutorialText;
        
        [Inject] private ITutorialManager _tutorialManager;

        [SerializeField] private List<GameObject> objectsToDisableOnTutorial;

        private void Start()
        {
            if (_tutorialManager == null)
            {
                GameLogger.LogError("TutorialManager is not assigned in the inspector.");
                return;
            }

            _tutorialManager.TutorialChanged += UpdateTutorialText;
            UpdateTutorialText(_tutorialManager.CurrentStep);
            
            if(GameManager.GameSetup.IsTutorial)
                foreach (var obj in objectsToDisableOnTutorial)
                    obj.SetActive(false);
        }

        private void UpdateTutorialText(ITutorialManager.TutorialStep step)
        {
            if (step == null)
            {
                tutorialText.text = "";
                return;
            }

            if (step.Description != null)
            {
                tutorialText.text = step.Description.Localize();
            }
            else
            {
                GameLogger.LogWarning("Tutorial step text is not assigned.");
                tutorialText.text = "No text available for this step.";
            }
        }
    }
}