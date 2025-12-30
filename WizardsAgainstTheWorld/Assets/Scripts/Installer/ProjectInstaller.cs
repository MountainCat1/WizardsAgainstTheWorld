using System.Collections.Generic;
using Data;
using Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Steam;
using UnityEngine;
using UnityEngine.Audio;
using Utilities;
using Zenject;

namespace Installer
{
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        public const string AudioMixerGroupSfx = "AudioMixerGroup_SFX";
        public const string AudioMixerGroupMusic = "AudioMixerGroup_Music";
        public const string AudioMixerGroupUi = "AudioMixerGroup_Ui";
        public const string AudioMixerGroupMaster = "AudioMixerGroup_Master";

        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        [SerializeField] private AudioMixerGroup masterMixerGroup;

        [SerializeField] private AudioMixer audioMixer;
        
        private JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new CustomSerializationBinder(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Vector2Converter(),
                new Vector2IntConverter(), 
                new StringEnumConverter()
            } 
        };

        public override void InstallBindings()
        {
            // Container.Bind<IStatsDisplayService>().To<StatsDisplayService>().FromNew().AsSingle();
            Container.Bind<IInputManager>().To<InputManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<SteamManager>().To<SteamManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<IItemManager>().To<ItemManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<IAchievementsManager>().To<AchievementsManager>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<JsonSerializerSettings>().FromInstance(_serializerSettings).AsTransient();
            Container.Bind<IJsonService>().To<JsonService>().FromNew().AsSingle();
            Container.Bind<ISceneLoader>().To<SceneLoader>().FromComponentsInHierarchy().AsSingle();

            Container.Bind<AudioMixer>().To<AudioMixer>().FromInstance(audioMixer).AsSingle();

            Container.Bind<AudioMixerGroup>().WithId(AudioMixerGroupSfx).FromInstance(sfxMixerGroup).AsCached();
            Container.Bind<AudioMixerGroup>().WithId(AudioMixerGroupMusic).FromInstance(musicMixerGroup).AsCached();
            Container.Bind<AudioMixerGroup>().WithId(AudioMixerGroupUi).FromInstance(uiMixerGroup).AsCached();
            Container.Bind<AudioMixerGroup>().WithId(AudioMixerGroupMaster).FromInstance(masterMixerGroup).AsCached();
        }
    }
}