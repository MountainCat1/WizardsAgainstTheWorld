using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Managers.LevelSelector;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace LevelSelector.Managers
{
    public class UITutorialStep
    {
        public GameObject TutorialPanel { set; get; }
    }

    public class LevelSelectorTutorialManager : MonoBehaviour
    {
        [Inject] private IInputManager _inputManager;
        [Inject] private ILevelSelectorUI _levelSelectorUI;
        [Inject] private IRegionManager _regionManager;
        [Inject] private ICrewManager _crewManager;

        private Queue<UITutorialStep> _tutorialSteps = new();
        private UITutorialStep _currentStep;

        [SerializeField] private Transform tutorialStepsContainer;
        [SerializeField] private GameObject uiBlocker;

        private void Start()
        {
            if (GameSettings.Instance.DisplayLevelSelectorTutorial == false)
            {
                Destroy(gameObject);
                return;
            }

            _inputManager.UI.TutorialContinue += OnTutorialContinue;

            SetupTutorial();

            ShowNextTutorialStep();
        }

        private void SetupTutorial()
        {
            foreach (Transform child in tutorialStepsContainer)
            {
                var go = child.gameObject;
                go.SetActive(false);
                AddTutorialStep(go);
            }

            var currentLocation = _regionManager.Region.Locations
                .First(x => x.Id == _crewManager.CurrentLocationId);
            
            var representativeLocation = _regionManager.Region.Locations
                .First(x => _regionManager.GetDistance(currentLocation, x) == 1);

            _levelSelectorUI.SelectLevel(representativeLocation);
        }

        private void AddTutorialStep(GameObject o)
        {
            if (o == null)
            {
                Debug.LogError("Tutorial step GameObject is null.");
                return;
            }

            UITutorialStep step = new UITutorialStep
            {
                TutorialPanel = o
            };

            _tutorialSteps.Enqueue(step);

            o.SetActive(false); // Initially hide the tutorial panel
        }

        private void OnTutorialContinue()
        {
            ShowNextTutorialStep();
        }

        private void ShowNextTutorialStep()
        {
            uiBlocker.SetActive(true);

            if (_currentStep != null)
            {
                _currentStep.TutorialPanel.SetActive(false);
            }

            if (_tutorialSteps.Count == 0)
            {
                Debug.Log("Tutorial completed.");
                _inputManager.UI.TutorialContinue -= OnTutorialContinue;

                GameSettings.Instance.DisplayLevelSelectorTutorial = false;
                GameSettings.Save();

                Destroy(gameObject);
                return;
            }

            _currentStep = _tutorialSteps.Dequeue();
            _currentStep.TutorialPanel.SetActive(true);
        }
    }
}