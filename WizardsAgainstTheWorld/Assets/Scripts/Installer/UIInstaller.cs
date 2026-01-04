using Zenject;

namespace UI
{
    public class UIInstaller : MonoInstaller<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SelectionDisplayEntryUI>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IUIManager>().To<UIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IUIInteractionStack>().To<UIInteractionStack>().FromComponentInHierarchy().AsSingle();
        }
    }
}