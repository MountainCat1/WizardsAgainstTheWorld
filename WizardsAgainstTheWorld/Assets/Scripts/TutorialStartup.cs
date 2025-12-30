using System.Collections.Generic;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;

public class TutorialStartup : MonoBehaviour
{
    [Inject] private ISceneLoader _sceneLoader;
    
    [SerializeField] private LocationFeatureData locationFeatureData;
    [SerializeField] private SceneReference tutorialScene;

    public void LaunchTutorial()
    {
        GameManager.GameSetup = new GameSetup()
        {
            Level = 0,
            IsTutorial = true,
            Settings = null,
            Location = new LocationData()
            {
                Features = new List<LocationFeatureData>()
                {
                    locationFeatureData
                },
            },
            GameData = new GameData()
            {
                Resources = new InGameResources()
                {
                    Juice = 999999
                },
                Creatures = new List<CreatureData>()
                {
                    new CreatureData()
                    {
                        Level = new LevelData()
                        {
                        },
                        Inventory = new InventoryData()
                        {
                            Items = new List<ItemData>()
                            {
                            }
                        },
                        Selected = true,
                        SightRange = 5f,
                        Name = "Jeff",
                        Color = ColorData.FromColor(new Color(0.5f, 0.5f, 0.5f)),
                    }
                }
            },
        };

        _sceneLoader.LoadScene(tutorialScene);
    }
}