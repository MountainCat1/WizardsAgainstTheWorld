using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using Zenject;

namespace Components
{
    [RequireComponent(typeof(Creature))]
    public class SoundComponent : MonoBehaviour
    {
        [Inject] ISoundPlayer _soundPlayer = null!;

        [SerializeField] private List<AudioClip> deathSounds = new();

        [SerializeField] private List<AudioClip> hitSounds = new();
        [SerializeField] private List<AudioClip> armoredHit = new();
        [SerializeField] private float chanceToPlayHitSound = 0.5f;

        private void Awake()
        {
            var creature = GetComponent<Creature>();

            creature.Health.Death += OnDeath;
            creature.Health.Hit += OnHit;
        }

        private void OnHit(HitContext ctx)
        {
            if (armoredHit.Any() && ctx.Damage / ctx.OriginalDamage < 0.5f)
            {
                PlayRandomSound(armoredHit);
                return;
            }

            if (Random.value > chanceToPlayHitSound)
                return;
            
            PlayRandomSound(hitSounds);
        }

        private void OnDeath(DeathContext ctx)
        {
            PlayRandomSound(deathSounds);
        }

        private void PlayRandomSound(IList<AudioClip> clips)
        {
            if (clips.Count == 0)
                return;

            var clip = clips[Random.Range(0, clips.Count)];
            _soundPlayer.PlaySound(clip, transform.position, SoundType.Sfx);
        }
    }
}