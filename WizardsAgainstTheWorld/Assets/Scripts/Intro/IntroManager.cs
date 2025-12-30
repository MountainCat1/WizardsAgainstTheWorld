using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Transform slidesParent; // Assign your slide GameObjects in the inspector
    [SerializeField] private List<GameObject> slides; // Assign your slide GameObjects in the inspector
    [SerializeField] private KeyCode nextSlideKey = KeyCode.Space;

    [SerializeField] private SceneReference secondScene;
    [SerializeField] private TutorialStartup tutorialStartup;
    
    [Inject] private ISceneLoader _sceneLoader;
    
    private int _currentSlideIndex = 0;

    void Start()
    {
        foreach (var go in slidesParent.GetDirectChildrenIncludingInactive())
        {
            slides.Add(go);
            if (go.activeSelf)
            {
                go.SetActive(false);
            }
        }

        ShowSlide(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(nextSlideKey))
        {
            GoToNextSlide();
        }
    }

    void ShowSlide(int index)
    {
        for (int i = 0; i < slides.Count; i++)
        {
            slides[i].SetActive(i == index);
        }
    }

    void GoToNextSlide()
    {
        _currentSlideIndex++;
        if (_currentSlideIndex < slides.Count)
        {
            ShowSlide(_currentSlideIndex);
        }
        else
        {
            // All slides are done — optionally disable manager or load next scene
            GameLogger.Log("Slides finished!");
            gameObject.SetActive(false);
            
            if (GameSettings.Instance.LoadGameTutorial)
            {
                tutorialStartup.LaunchTutorial();
                return;
            }
            
            _sceneLoader.LoadScene(secondScene.ScenePath);
        }
    }
}
