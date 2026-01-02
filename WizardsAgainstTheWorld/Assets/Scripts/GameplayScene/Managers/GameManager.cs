using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Building;
using CrewUpgrades;
using Data;
using Items;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Services.MapGenerators;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using VictoryConditions;
using Zenject;

namespace Managers
{
    public class GameSetup
    {
        public GenerateMapSettings Settings { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public bool IsTutorial { get; set; } = false;
        [CanBeNull] public GameData GameData { get; set; }
        public int MapSeed { get; set; }
    }

    public class GameResult
    {
        public ICollection<VictoryConditionResult> VictoryConditionsResults { get; set; } =
            new List<VictoryConditionResult>();
    }

    public class VictoryConditionResult
    {
        public string ConditionId { get; set; }
        public bool IsAchieved { get; set; }
    }

    public class GameManager : MonoBehaviour
    {
        public static GameSetup GameSetup { get; set; }
        public static GameResult GameResult { get; set; }

        [Inject] private DiContainer _diContainer;
        [Inject] private IMapGenerator _mapGenerator;
        [Inject] private DiContainer _container;
        [Inject] private ISpawnerManager _spawnerManager;
        [Inject] private ICameraController _cameraController;
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private IDataManager _dataManager;
        [Inject] private IVictoryConditionManager _victoryConditionManager;
        [Inject] private IInputManager _inputManager;
        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IJuiceManager _juiceManager;
        [Inject] private IAstarManager _astarManager;
        [Inject] private ITutorialManager _tutorialManager;
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private ICrewUpgradeManager _crewUpgradeManager;
        [Inject] private ISoundManager _soundManager;
        [Inject] private GridSystem _gridSystem;
    
        [SerializeField] private Creature playerPrefab;
        [SerializeField] private Creature enemyPrefab;

        [SerializeField] private List<ItemBehaviour> startingItems;

        [SerializeField] private SceneReference levelSelectorScene;
        [SerializeField] private SceneReference mainMenuScene;

        [SerializeField] private float delayToGoBackToLevelSelector = 4f;

        [SerializeField] private float maxDistanceFromExit = 3f;
        [SerializeField] private float minDistanceFromExit = 1f;

        [SerializeField] private VictoryCondition[] defaultVictoryConditions;
        [SerializeField] private VictoryCondition[] bossVictoryConditions;

        private MapData _map;
        private GameData _data;

        private void Awake()
        {
            GameResult = new GameResult();
        }

        private void Start()
        {
            _data = GameSetup?.GameData ?? _dataManager.LoadData() ?? new GameData();
            GameSetup ??= new GameSetup();
            
            StartCoroutine(WaitToCreateGrid());

            _inputManager.GoBackToMenu += GoBackToLevelSelector;
        }

        private void OnDestroy()
        {
            _inputManager.GoBackToMenu -= GoBackToLevelSelector;
        }

        #region Private Methods

        private IEnumerator WaitToCreateGrid()
        {
            // TODO: HACK, i mean the loading after 1 second, should be done in a better way

            if (GameSetup?.Settings is not null)
            {
                GameLogger.Log($"Using settings from level selector for map {GameSetup.Name}...");
                _mapGenerator.Settings = GameSetup.Settings;
            }
            else
            {
                GameLogger.Log("Using settings set in the inspector...");
            }
            
            _mapGenerator.SafeGenerateMap();
            var wallPositions = _mapGenerator.MapData.GetAllTilePositionsOfType(TileType.Wall);
            
            foreach (var wallPosition in wallPositions)
            {
                _gridSystem.GetCell(new GridPosition(wallPosition)).SetTerrainBlocked(true);
            }

            yield return new WaitForSeconds(0.1f);
            _astarManager.Scan();


            yield return new WaitForSeconds(1);
            GridGenerator.FindObjectOfType<GridGenerator>().CreateGrid();

            yield return new WaitForSeconds(0.4f);

            SpawnUnits(mapData: _map);
            SpawnUpgrades();
            _enemySpawner.Initialize(_mapGenerator.MapData);

            if (GameSettings.Instance.Preferences.UseJuiceMechanic)
            {
                _juiceManager.Initialize(
                    FindObjectsOfType<Creature>().Where(x => x.Team == Teams.Player).ToArray(),
                    _data.Resources.Juice
                );
            }

            if (GameSetup?.IsTutorial == true)
            {
                _tutorialManager.StartTutorial();
            }

            yield return new WaitForSeconds(0.5f);
        }

        private void SpawnUpgrades()
        {
            var upgrades = _data.Upgrades;

            foreach (var upgradeData in upgrades)
            {
                var upgradePrefab = _crewUpgradeManager.Instantiate(upgradeData.Id);

                var upgrade = _diContainer.InstantiatePrefabForComponent<CrewUpgrade>(
                    upgradePrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    null
                );

                upgrade.InitializeGameScene(_diContainer);
            }
        }

        private void SpawnUnits(MapData mapData)
        {
            var startingPosition = new Vector2(0, 0);
            
            var data = _data;

            if (GameSetup.GameData != null)
            {
                GameLogger.Log("OVERRIDING GAME DATA");
                data = GameSetup.GameData;
            }

            var playerUnits = new List<Creature>();

            _dataManager.DoStuffToDataSoItWorks(data);
            
            var spawnPosition = startingPosition;
            
            foreach (var creatureData in data.Creatures)
            {
                var creature = _creatureManager.SpawnCreature(playerPrefab, spawnPosition);

                creature.Initialize(creatureData);

                if (!creatureData.Selected)
                    creature.gameObject.SetActive(false);

                foreach (var item in creature.Inventory.Items)
                {
                    item.Use(
                        new ItemUseContext()
                        {
                            Creature = creature
                        }
                    );
                }

                playerUnits.Add(creature);
            }
            
            _cameraController.MoveTo(spawnPosition);

            _victoryConditionManager.VictoryAchieved += () =>
            {
                GameLogger.Log("Victory Achieved!");
                // StartCoroutine(WaitToGoBackToLevelSelector());
            };

            _victoryConditionManager.EndGameAchieved += OnEndGameAchieved;
        }

        private void OnEndGameAchieved()
        {
            GameLogger.Log(
                $"End Game Achieved! Result:\n{JsonConvert.SerializeObject(_victoryConditionManager.VictoryConditions)}"
            );

            var victoryConditionResultss = _victoryConditionManager.VictoryConditions
                .Select(vc => new VictoryConditionResult
                    {
                        ConditionId = vc.Key.GetIdentifier(),
                        IsAchieved = vc.Value
                    }
                )
                .ToArray();

            GameResult.VictoryConditionsResults.AddRange(victoryConditionResultss);

            StartCoroutine(WaitToGoBackToLevelSelector());
        }

        IEnumerator WaitToGoBackToLevelSelector()
        {
            float elapsed = 0f;
            float delay = delayToGoBackToLevelSelector;
            bool skip = false;

            void SkipWait(InputAction.CallbackContext ctx)
            {
                skip = true;
            }

            _inputManager.Cancel += SkipWait;

            while (elapsed < delay)
            {
                if (skip)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            _inputManager.Cancel -= SkipWait;

            GoBackToLevelSelector();
        }

        private void GoBackToLevelSelector()
        {
            if (GameSetup?.IsTutorial ?? false)
            {
                var settings = GameSettings.Instance;
                settings.LoadGameTutorial = false;
                SaveLoadManager.Save(settings);
                _sceneLoader.LoadScene(Scenes.MainMenu);
            }
            else
            {
                SaveCurrentData();
                _sceneLoader.LoadScene(Scenes.LevelSelector);
            }
        }

        private void SaveCurrentData()
        {
            var currentGameData = _dataManager.GetData();

            // Save the current level progress
            currentGameData.Creatures = FindObjectsOfType<Creature>(true)
                .Where(x => x.Team == Teams.Player)
                .Select(CreatureData.FromCreature)
                .ToList();

            currentGameData.Resources.Juice = _juiceManager.Juice;

            _dataManager.SaveData();
        }

        #endregion
    }
}