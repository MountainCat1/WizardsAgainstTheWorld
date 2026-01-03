using Data;
using GameplayScene.Managers;
using GameplayScene.Managers.Pathfinding;
using Managers;
using Managers.Visual;
using Services.MapGenerators.GenerationSteps;
using UI;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<IDataManager>().To<DataManager>().FromNew().AsSingle().NonLazy();
        
        Container.Bind<ITimeManager>().To<TimeManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IUnderCursorManager>().To<UnderCursorManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICursorManager>().To<CursorManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IItemManager>().To<ItemManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICrewUpgradeManager>().To<CrewUpgradeManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IEffectDescriptionProvider>().To<EffectDescriptionProvider>().FromNew().AsSingle();
        Container.Bind<IItemDescriptionManager>().To<ItemDescriptionManager>().FromNew().AsSingle();
        Container.Bind<ISignalManager>().To<SignalManager>().FromNew().AsSingle().NonLazy();
        Container.Bind<ISpawnerManager>().To<SpawnerManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IDynamicPoolingManager>().To<DynamicPoolingManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICyclingPoolingManager>().To<CyclingPoolingManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IProjectileManager>().To<ProjectileManager>().FromNew().AsSingle().NonLazy();
        Container.Bind<IResourceManager>().To<ResourceManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
        Container.Bind<IDataResolver>().To<DataResolver>().FromComponentsInHierarchy().AsSingle();
        // Container.Bind<IInputManager>().To<InputManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IInputMapper>().To<InputMapper>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ISelectionManager>().To<SelectionManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ISelectionInspectionManager>().To<SelectionInspectionManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IDayNightManager>().To<DayNightManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IPathfinding>().To<CachedPathfinding>().FromInstance(new CachedPathfinding(FindObjectOfType<OldPathfinding>(), FindObjectOfType<GridGenerator>())).AsSingle();
        // Container.Bind<IFlagManager>().To<FlagManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ISoundPlayer>().To<SoundPlayer>().FromComponentsInHierarchy().AsSingle().NonLazy();
        Container.Bind<ITeamManager>().To<TeamManager>().FromComponentsInHierarchy().AsSingle();
        // Container.Bind<IPopupManager>().To<PopupManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IEntityManager>().To<EntityManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICreatureManager>().To<CreatureManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IEntityEventProducer>().To<EntityEventProducer>().FromNew().AsSingle().NonLazy();
        Container.Bind<ICreatureEventProducer>().To<CreatureEventProducer>().FromNew().AsSingle().NonLazy();
        Container.Bind<IEntityFinder>().To<EntityFinder>().FromNew().AsSingle().NonLazy();
        Container.Bind<ILootManager>().To<LootManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<PathfindingRecalculator>().FromNew().AsSingle().NonLazy();
        Container.Bind<IMapGenerator>().To<StepDungeonGenerator>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IAstarManager>().To<AstarManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICameraController>().To<CameraController>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ICameraShakeService>().To<CameraShakeService>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IVictoryConditionManager>().To<VictoryConditionManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IEnemySpawner>().To<EnemySpawner>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IJuiceManager>().To<JuiceManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IVisionManager>().To<VisionManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ISoundManager>().To<SoundManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<ITutorialManager>().To<TutorialManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IStatusEffectManager>().To<StatusEffectManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IWeaponManager>().To<WeaponManager>().FromComponentsInHierarchy().AsSingle();
        Container.Bind<IInGamePauseManager>().To<InGamePauseManager>().FromComponentsInHierarchy().AsSingle();

        Container.Bind<IFloatingTextManager>().To<FloatingTextManager>()
            .FromInstance(FindFirstObjectByType<FloatingTextManager>());
    }
}
