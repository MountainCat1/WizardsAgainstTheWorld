using Building.Managers;
using UnityEngine;
using Zenject;

namespace Building
{
    public class BuildingModuleInstaller : MonoInstaller<BuildingModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GridSystem>().FromInstance(new GridSystem(100, 100, 1, Vector3.zero)).AsSingle();
            Container.Bind<IBuilderManager>().To<BuilderManager>().FromComponentsInHierarchy().AsSingle();
        }
    }
}