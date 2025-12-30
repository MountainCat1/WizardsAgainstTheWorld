using Data;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using UI;
using Zenject;

namespace Installer
{
    public class LevelSelectorInstaller : MonoInstaller<LevelSelectorInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IDataManager>().To<DataManager>().FromNew().AsSingle().NonLazy();
            
            Container.Bind<IDataResolver>().To<DataResolver>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ISpawnerManager>().To<SpawnerManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IDynamicPoolingManager>().To<DynamicPoolingManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ISoundManager>().To<SoundManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ISoundPlayer>().To<SoundPlayer>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IGameResultConsumer>().To<GameResultConsumer>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<IYesNoDialogController>().To<YesNoDialogController>().FromComponentsInHierarchy().AsSingle();
            
            
            Container.Bind<IItemManager>().To<ItemManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<ICrewUpgradeManager>().To<CrewUpgradeManager>().FromComponentsInHierarchy().AsSingle();
            
            Container.Bind<IUIManager>().To<UIManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            
            Container.Bind<IEffectDescriptionProvider>().To<EffectDescriptionProvider>().FromNew().AsSingle();
            Container.Bind<IItemDescriptionManager>().To<ItemDescriptionManager>().FromNew().AsSingle();
            Container.Bind<IUpgradeManager>().To<UpgradeManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ICrewManager>().To<CrewManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IShopGenerator>().To<ShopGenerator>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ILocationGenerator>().To<LocationGenerator>().FromComponentsInHierarchy().AsSingle()
                .NonLazy();
            Container.Bind<IRegionGenerator>().To<RegionGenerator>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IRegionManager>().To<RegionManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ITravelManager>().To<TravelManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ILocationInteractionManager>().To<LocationInteractionManager>().FromComponentsInHierarchy()
                .AsSingle().NonLazy();
            Container.Bind<IGameEventManager>().To<GameEventManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ICrewGenerator>().To<CrewGenerator>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ISkillManager>().To<SkillManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IPostLevelHandler>().To<PostLevelHandler>().FromComponentsInHierarchy().AsSingle().NonLazy();

            // UI
            Container.Bind<ILevelSelectorSlideManagerUI>().To<LevelSelectorSlideManagerUI>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<IFloatingTextServiceUI>().To<FloatingTextServiceUI>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<ILevelSelectorUI>().To<LevelSelectorUI>().FromComponentsInHierarchy().AsSingle();
        }
    }
}