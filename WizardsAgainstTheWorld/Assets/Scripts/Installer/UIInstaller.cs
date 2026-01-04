using Zenject;

namespace UI
{
    public class UIInstaller : MonoInstaller<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SelectionDisplayEntryUI>().FromComponentInHierarchy().AsSingle();
        }
    }
}