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

        // Small safety tail in case of scheduling/latency
        private const float TailSeconds = 2f;
        
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
            var duration = Mathf.Max(0f, _source.clip.length / Mathf.Max(0.0001f, _source.pitch));
            _returnRoutine = StartCoroutine(ReturnAfter(duration + TailSeconds));
        }

        private IEnumerator ReturnAfter(float seconds)
        {
            var t = 0f;
            // Handle pauses / stopping early: if stopped, return immediately.
            while (t < seconds)
            {
                if (!_source.isPlaying) break;
                t += Time.unscaledDeltaTime; // unaffected by timescale
                yield return null;
            }

            _returnRoutine = null;
            _free?.Invoke();
        }
    }
}