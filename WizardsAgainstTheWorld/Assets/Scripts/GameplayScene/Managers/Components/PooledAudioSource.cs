using System;
using System.Collections;
using UnityEngine;

namespace GameplayScene.Managers.Components
{
    /// <summary>
    /// Attach to the same GameObject as AudioSource in the prefab.
    /// Automatically returns the AudioSource to the pool after playback.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class PooledAudioSource : MonoBehaviour, IFreeable
    {
        private AudioSource _source;
        private Action _free;
        private Coroutine _returnRoutine;

        
        public AudioSource AudioSource => _source;

        private void Awake() => _source = GetComponent<AudioSource>();

        public void Initialize(Action free)
        {
            _free = free ?? throw new ArgumentNullException(nameof(free));
            // If something already started playing, arm the return now.
            ArmReturn();
        }

        public void Deinitialize()
        {
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }
            _free = null;
        }

        /// <summary>
        /// Call this right after you call Play() (or when clip is swapped).
        /// </summary>
        public void ArmReturn()
        {
            if (_free == null) return; // not pooled yet
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            if (_source.clip == null) return;
            _returnRoutine = StartCoroutine(ReturnCoroutine());
        }

        private IEnumerator ReturnCoroutine()
        {
            while (_source != null && _source.isPlaying)
                yield return null;

            _free?.Invoke();

        }
    }
}