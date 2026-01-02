using Data;
using Managers;
using Managers.Visual;
using UI;
using Zenject;

namespace Installer
{
    public class MenuInstaller : MonoInstaller<MenuInstaller>
    {
        public override void InstallBindings()
        {
            // Bindings

            Container.Bind<IItemManager>().To<ItemManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IYesNoDialogController>().To<YesNoDialogController>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IDataManager>().To<DataManager>().FromNew().AsSingle().NonLazy();
            Container.Bind<ISoundManager>().To<SoundManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IUIManager>().To<UIManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ITeamManager>().To<TeamManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ISpawnerManager>().To<SpawnerManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ICyclingPoolingManager>().To<CyclingPoolingManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IDynamicPoolingManager>().To<DynamicPoolingManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IProjectileManager>().To<ProjectileManager>().FromNew().AsSingle().NonLazy();
            Container.Bind<ICreatureEventProducer>().To<CreatureEventProducer>().FromNew().AsSingle().NonLazy();
            Container.Bind<IEntityManager>().To<EntityManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ICreatureManager>().To<CreatureManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IFloatingTextManager>().To<FloatingTextManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IPathfinding>().To<CachedPathfinding>().FromInstance(new CachedPathfinding(FindObjectOfType<OldPathfinding>(), FindObjectOfType<GridGenerator>())).AsSingle();
            Container.Bind<ISoundPlayer>().To<SoundPlayer>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ICameraShakeService>().To<CameraShakeServiceDisabled>().FromNew().AsSingle().NonLazy();
            Container.Bind<ILootManager>().To<LootManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<IWeaponManager>().To<WeaponManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
        }
    }
}