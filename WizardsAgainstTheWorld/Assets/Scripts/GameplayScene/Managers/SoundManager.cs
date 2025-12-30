using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Utilities;
using Zenject;

namespace Managers
{
    public interface ISoundManager
    {
        void SetSoundtrack(List<AudioClip> newSoundtracks);
        void UpdateVolumes();
        void PauseSoundtrack();
        void ResumeSoundtrack();
    }

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Animator))]
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        private static readonly int IsPaused = Animator.StringToHash("IsPaused");
        [Inject] private AudioMixer _audioMixer;
        
        [SerializeField] private List<AudioClip> soundtracks;

        private AudioClip _lastSoundtrack;
        private AudioSource _soundtrackAudioSource;
        private Animator _soundtrackAnimator;

        private void Awake()
        {
            _soundtrackAudioSource = GetComponent<AudioSource>();
            _soundtrackAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            UpdateVolumes();
            
            PlaySoundtrack(soundtracks.RandomElement());
        }

        private void Update()
        {
            // check if the soundtrack ended by comparing the length of the audio clip and the time passed
            if (_soundtrackAudioSource.clip != null &&
                _soundtrackAudioSource.clip.length - _soundtrackAudioSource.time < 0.1f)
                PlayNextSoundtrack();
        }

        private void PlayNextSoundtrack()
        {
            if (soundtracks.Count == 0)
                return;
            if (soundtracks.Count == 1)
            {
                PlaySoundtrack(soundtracks.First());
                return;
            }

            var nextSoundtrack = soundtracks.Except(new[] { _lastSoundtrack }).RandomElement();
            PlaySoundtrack(nextSoundtrack);
        }

        private void PlaySoundtrack(AudioClip audioClip)
        {
            _soundtrackAudioSource.clip = audioClip;
            _soundtrackAudioSource.Play();
            _lastSoundtrack = audioClip;
        }

        public void SetSoundtrack(List<AudioClip> newSoundtracks)
        {
            if (newSoundtracks == null || newSoundtracks.Count == 0)
            {
                GameLogger.LogWarning("Soundtrack list is empty or null.");
                return;
            }

            soundtracks = newSoundtracks;

            PlaySoundtrack(soundtracks.RandomElement());
        }

        public void UpdateVolumes()
        {
            var gameSettings = GameSettings.Instance;
            
            _audioMixer.SetFloat("SFX_Volume", gameSettings.Sound.SfxVolume * 20f);
            _audioMixer.SetFloat("Music_Volume", gameSettings.Sound.MusicVolume * 20f);
            _audioMixer.SetFloat("UI_Volume", gameSettings.Sound.UiVolume * 20f);
            _audioMixer.SetFloat("Master_Volume", gameSettings.Sound.MasterVolume * 20f);
        }

        public void PauseSoundtrack()
        {
            if (_soundtrackAudioSource.isPlaying)
            {
                _soundtrackAudioSource.Pause();
                _soundtrackAnimator.SetBool(IsPaused, true);
            }   
        }

        public void ResumeSoundtrack()
        {
            if (!_soundtrackAudioSource.isPlaying)
            {
                _soundtrackAudioSource.UnPause();
                _soundtrackAnimator.SetBool("IsPaused", false);
            }
        }
    }
}