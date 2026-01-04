using System.Collections.Generic;
using GameplayScene.Managers.Components;
using Installer;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Managers
{
    public enum SoundType
    {
        Sfx,
        Music,
        UI
    }

    public interface ISoundPlayer
    {
        void PlaySound(AudioClip clip, Vector2 position, SoundType soundType = SoundType.Sfx);
        void PlaySoundGlobal(AudioClip clip, SoundType soundType = SoundType.Sfx);
        PooledAudioSource CreateSound(AudioClip clip, SoundType soundType);
    }

    public sealed class SoundPlayer : MonoBehaviour, ISoundPlayer
    {
        // Positional Z offset so 2D sounds don't overlap camera
        private const float ZOffset = 0.1f;

        private const float MinDistanceDefault = 1f;
        private const float MaxDistanceDefault = 15f;
        private const float GlobalMaxDistance = float.MaxValue;

        private const string AudioPoolKey = "SoundPlayer.AudioSources";

        private readonly Dictionary<SoundType, AudioMixerGroup> _mixerGroups = new();

        [SerializeField] private PooledAudioSource audioSourcePrefab;

        private Camera _camera;

        // Pooling
        [Inject] private IDynamicPoolingManager _poolingManager = null!;
        private IPoolAccess<PooledAudioSource> _audioPool;

        [Inject]
        public void InjectMixerGroups(
            [Inject(Id = ProjectInstaller.AudioMixerGroupSfx)] AudioMixerGroup sfxGroup,
            [Inject(Id = ProjectInstaller.AudioMixerGroupMusic)] AudioMixerGroup musicGroup,
            [Inject(Id = ProjectInstaller.AudioMixerGroupUi)] AudioMixerGroup uiGroup)
        {
            _mixerGroups[SoundType.Sfx] = sfxGroup;
            _mixerGroups[SoundType.Music] = musicGroup;
            _mixerGroups[SoundType.UI] = uiGroup;
        }

        private void Awake()
        {
            _audioPool = _poolingManager.GetPoolAccess<PooledAudioSource>(AudioPoolKey);
            _camera = Camera.main;
        }

        public void PlaySound(AudioClip clip, Vector2 position, SoundType soundType = SoundType.Sfx)
        {
            if (!ValidateClip(clip)) return;

            var pooledAudio = CreateSound(clip, soundType);

            pooledAudio.transform.position = new Vector3(
                position.x,
                position.y,
                _camera.transform.position.z + ZOffset);

            var gs = GameSettings.Instance;

            // if (pitchRandomness || gs.Sound.RandomPitch)
            // {
            //     pooledAudio.AudioSource.pitch = 1f + Random.Range(0f, DefaultPitchRandomness);
            // }

            pooledAudio.AudioSource.spatialBlend = gs.Sound.SpatialBlend;
            pooledAudio.AudioSource.minDistance = MinDistanceDefault;
            pooledAudio.AudioSource.maxDistance = MaxDistanceDefault;

            // Arm pooled return now that we've configured and started playback.
            pooledAudio.AudioSource.Play();
            pooledAudio.ArmReturn();
        }

        public void PlaySoundGlobal(AudioClip clip, SoundType soundType = SoundType.Sfx)
        {
            if (!ValidateClip(clip)) return;

            var pooledAudio = CreateSound(clip, soundType);
            pooledAudio.AudioSource.maxDistance = GlobalMaxDistance;
            pooledAudio.AudioSource.spatialBlend = 0f;
            pooledAudio.ArmReturn();
        }

        /// <summary>
        /// Spawns (or reuses) an AudioSource from pool, configures clip + mixer, and starts playback.
        /// </summary>
        public PooledAudioSource CreateSound(AudioClip clip, SoundType soundType)
        {
            var pooledAudio = _audioPool.SpawnObject(audioSourcePrefab, Vector2.zero, 0f, null);
            var audioSource = pooledAudio.AudioSource;

            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = _mixerGroups[soundType];
            audioSource.pitch = 1f;
            audioSource.spatialBlend = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.Stop();
            audioSource.time = 0f;

            return pooledAudio;
        }

        private static bool ValidateClip(AudioClip clip)
        {
            if (clip != null) return true;
            GameLogger.LogWarning("Missing sound!");
            return false;
        }
    }
}