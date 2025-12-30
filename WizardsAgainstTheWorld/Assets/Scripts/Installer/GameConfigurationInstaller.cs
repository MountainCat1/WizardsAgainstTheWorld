using ScriptableObjects;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameConfigurationInstaller", menuName = "Installers/GameConfigurationInstaller")]
public class GameConfigurationInstaller : ScriptableObjectInstaller<GameConfigurationInstaller>
{
    [SerializeField] private GameConfiguration gameConfiguration;

    public override void InstallBindings()
    {
        Container.Bind<IGameConfiguration>().To<GameConfiguration>().FromInstance(gameConfiguration);
    }
}