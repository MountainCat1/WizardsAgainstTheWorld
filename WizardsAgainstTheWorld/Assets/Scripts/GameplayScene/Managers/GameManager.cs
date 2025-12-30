using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
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
        public LocationData Location { get; set; }
        public GenerateMapSettings Settings { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public bool IsTutorial { get; set; } = false;
        [CanBeNull] public GameData GameData { get; set; }
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
        [Inject] private ISkillApplier _skillApplier;
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private ICrewUpgradeManager _crewUpgradeManager;
        [Inject] private ISoundManager _soundManager;

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

        private void Start()
        {
            GameResult = new GameResult();

            _data = GameSetup?.GameData ?? _dataManager.LoadData();

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

            var tileSetOverride = GameSetup?
                .Location
                .Features.Select(x => x.overrideMapTileSetOverride)
                .FirstOrDefault(x => x != MapTileSetOverrideType.None);

            if (tileSetOverride != null)
            {
                GameLogger.Log($"Using tile set override: {tileSetOverride}");
                _mapGenerator.Settings.mapTileSetOverrideType = tileSetOverride.Value;
            }
            else
            {
                GameLogger.Log("Using default tile set override.");
                _mapGenerator.Settings.mapTileSetOverrideType = MapTileSetOverrideType.Default;
            }

            if (!string.IsNullOrEmpty(GameSetup?.Location?.OverrideMap))
            {
                GameLogger.Log("Overriding map with prefab...");
                var mapPrefab = Resources.Load<GameObject>($"Maps/{GameSetup.Location.OverrideMap}");
                var instantiatedMap = _diContainer.InstantiatePrefab(mapPrefab);
                var mapDescriptor = instantiatedMap.GetComponent<MapOverrideDescriptor>();
                var room = new RoomData();
                {
                    room.Positions = TilemapUtilities.GetAllTilePositions(mapDescriptor.FloorTileMap)
                        .Select(Vector2Int.RoundToInt).ToList();
                    room.IsEntrance = true;
                }
                _map = new MapData(new Vector2Int(0, 0), new int[0, 0], 1f, new List<RoomData>() { room });

                _creatureManager.ScanForCreatures();

                var shadowGenerator = FindObjectOfType<TilemapShadowGenerator>();

                shadowGenerator.SetFloorTilemap(mapDescriptor.FloorTileMap);
                shadowGenerator.SetWallTilemap(mapDescriptor.WallTileMap);
            }
            else
            {
                GameLogger.Log("Generating map...");
                _mapGenerator.SafeGenerateMap();
                _map = _mapGenerator.MapData;
            }

            var musicOverride = GameSetup?.Location?.Features
                .FirstOrDefault(x => !string.IsNullOrEmpty(x.MusicOverride));

            if (musicOverride != null)
            {
                GameLogger.Log($"Using music override: {musicOverride}");

                var audioClip = Resources.Load<AudioClip>($"Music/{musicOverride.MusicOverride}");

                _soundManager.SetSoundtrack(new List<AudioClip>() { audioClip });
            }
            else
            {
                GameLogger.Log("Using default music.");
            }

            yield return new WaitForSeconds(0.1f);
            _astarManager.Scan();


            yield return new WaitForSeconds(1);
            GridGenerator.FindObjectOfType<GridGenerator>().CreateGrid();

            yield return new WaitForSeconds(0.4f);

            SpawnUnits(mapData: _map);
            SpawnUpgrades();

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
            var startingRoom = _map.GetAllRooms().First(x => x.IsEntrance);

            var data = _data;

            if (GameSetup.GameData != null)
            {
                GameLogger.Log("OVERRIDING GAME DATA");
                data = GameSetup.GameData;
            }

            var playerUnits = new List<Creature>();

            _dataManager.DoStuffToDataSoItWorks(data);

            var exitObject = FindObjectOfType<EnterObject>();
            if (exitObject == null)
            {
                GameLogger.LogError("EnterObject not found in the scene. Cannot spawn units.");
                return;
            }

            var spawnPosition = startingRoom.Positions
                .Select(x => (Vector2)x * _map.TileSize)
                .Where(x => Vector2.Distance(x, exitObject.transform.position) > minDistanceFromExit)
                .Where(x => Vector2.Distance(x, exitObject.transform.position) < maxDistanceFromExit)
                .ToArray();

            if (data is not null)
            {
                foreach (var creatureData in data.Creatures)
                {
                    var creature = _creatureManager.SpawnCreature(playerPrefab, spawnPosition.RandomElement());

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

                    _skillApplier.ApplySkillToCreature(creature, creatureData);

                    playerUnits.Add(creature);
                }
            }
            else
            {
                playerUnits.Add(
                    _creatureManager.SpawnCreature(
                        playerPrefab,
                        spawnPosition.RandomElement() * _map.TileSize
                    )
                );
                playerUnits.Add(
                    _creatureManager.SpawnCreature(
                        playerPrefab,
                        spawnPosition.RandomElement() * _map.TileSize
                    )
                );
                playerUnits.Add(
                    _creatureManager.SpawnCreature(
                        playerPrefab,
                        spawnPosition.RandomElement() * _map.TileSize
                    )
                );

                foreach (var unit in playerUnits)
                {
                    unit.name = $"{Names.Human.RandomElement()} {Surnames.Human.RandomElement()}";

                    foreach (var item in startingItems)
                    {
                        var itemInInventory = unit.Inventory.AddItemFromPrefab(item);
                        itemInInventory.Use(
                            new ItemUseContext()
                            {
                                Creature = unit
                            }
                        );
                    }
                }
            }

            // Only initialize enemy spawner on generated maps
            if (string.IsNullOrEmpty(GameSetup.Location.OverrideMap)) // check if we use dont use a prefab map
            {
                _enemySpawner.Initialize(mapData: mapData, location: GameSetup.Location);
            }

            var isBossLocation = GameSetup.Location.Type == LocationType.BossNode;
            var basicConditions = isBossLocation
                ? bossVictoryConditions
                : defaultVictoryConditions;

            var allConditions = Resources.LoadAll<VictoryCondition>("VictoryConditions");
            var featureConditions = GameSetup.Location.Features
                .SelectMany(feature => feature.VictoryConditionsIds)
                .Select(id => allConditions.FirstOrDefault(condition => condition.GetIdentifier() == id))
                .Where(condition => condition != null)
                .ToArray();

            _victoryConditionManager.SetVictoryAndDefeatConditions(
                victoryConditions: featureConditions,
                endGameCondition: basicConditions
            );

            _victoryConditionManager.Check();

            _cameraController.MoveTo(playerUnits.First().transform.position);

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