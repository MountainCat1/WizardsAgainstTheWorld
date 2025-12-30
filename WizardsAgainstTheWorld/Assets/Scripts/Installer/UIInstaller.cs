using Zenject;

namespace UI
{
    public class UIInstaller : MonoInstaller<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ISelectionDisplayUI>().To<SelectionDisplayUI>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IInventoryDisplayUI>().To<InventoryDisplayUI>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<SelectionDisplayEntryUI>().FromComponentInHierarchy().AsSingle();
            Container.Bind<InventoryUI>().FromComponentInHierarchy().AsSingle();
        }
    }
}