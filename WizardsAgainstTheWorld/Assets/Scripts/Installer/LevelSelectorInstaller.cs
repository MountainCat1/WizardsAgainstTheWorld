using Data;
using Managers;
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
            Container.Bind<IYesNoDialogController>().To<YesNoDialogController>().FromComponentsInHierarchy().AsSingle();
            
            
            Container.Bind<IItemManager>().To<ItemManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<ICrewUpgradeManager>().To<CrewUpgradeManager>().FromComponentsInHierarchy().AsSingle();
            
            Container.Bind<IUIManager>().To<UIManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            
            Container.Bind<IEffectDescriptionProvider>().To<EffectDescriptionProvider>().FromNew().AsSingle();
            Container.Bind<IItemDescriptionManager>().To<ItemDescriptionManager>().FromNew().AsSingle();

            // UI
            Container.Bind<IFloatingTextServiceUI>().To<FloatingTextServiceUI>().FromComponentsInHierarchy().AsSingle();
        }
    }
}