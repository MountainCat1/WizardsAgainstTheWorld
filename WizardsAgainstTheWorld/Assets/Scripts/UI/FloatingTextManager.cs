using System;
using System.Collections.Generic;
using Components;
using Managers;
using TMPro;
using UnityEngine;
using Utilities;
using Zenject;

using static Utilities.LocalizationHelper;

namespace UI
{
    public interface IFloatingTextManager
    {
        void SpawnFloatingText(Vector3 position, string localizationKey, FloatingTextType type, params object[] args);
    }

    public enum FloatingTextType
    {
        Damage,
        Heal,
        Miss,
        Interaction,
        InteractionCancelled,
        InteractionCompleted,
        LevelUp,
        XpGain,
        FriendlyFire
    }

    public struct FloatingTextSettings
    {
        public Color Color;
        public float Size;
        public FontStyles FontStyle;
    }

    public class FloatingTextManager : MonoBehaviour, IFloatingTextManager
    {
        [Inject] private IProjectileManager _projectileManager;
        [Inject] private ICreatureEventProducer _creatureEventProducer;
        [Inject] private IDynamicPoolingManager _dynamicPoolingManager;
        [Inject] private IWeaponManager _weaponManager;
        [Inject] private ITeamManager _teamManager;

        private const string MissKey = "Game.FloatingText.Miss";
        private const string DamageKey = "Game.FloatingText.Damage";
        private const string HealKey = "Game.FloatingText.Heal";
        private const string FriendlyFireKey = "Game.FloatingText.FriendlyFire";

        [SerializeField] private Transform popupParent;
        [SerializeField] private FloatingTextUI floatingTextPrefab;

        private readonly Dictionary<FloatingTextType, FloatingTextSettings> _fontStyles = new()
        {
            {
                FloatingTextType.Damage,
                new FloatingTextSettings { Color = Color.red, Size = 0.7f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.Heal,
                new FloatingTextSettings { Color = Color.green, Size = 0.7f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.Interaction,
                new FloatingTextSettings { Color = Color.white, Size = 1f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.InteractionCompleted,
                new FloatingTextSettings { Color = Color.green, Size = 1f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.InteractionCancelled,
                new FloatingTextSettings { Color = Color.red, Size = 1f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.Miss,
                new FloatingTextSettings { Color = Color.yellow, Size = 0.75f, FontStyle = FontStyles.Italic }
            },
            {
                FloatingTextType.LevelUp,
                new FloatingTextSettings { Color = Color.green, Size = 1f, FontStyle = FontStyles.Bold }
            },
            {
                FloatingTextType.XpGain,
                new FloatingTextSettings { Color = Color.green, Size = 0.3f, FontStyle = FontStyles.Normal }
            },
            {
                FloatingTextType.FriendlyFire,
                new FloatingTextSettings { Color = Color.red, Size = 0.7f, FontStyle = FontStyles.Italic }
            }
        };

        private void Start()
        {
            _creatureEventProducer.CreatureHit += OnCreatureHit;
            _creatureEventProducer.CreatureHeal += OnCreatureHeal;
            _weaponManager.WeaponAttackMissed += OnWeaponAttackMissed;
        }

        private void OnWeaponAttackMissed(Weapon weapon, AttackContext ctx, Entity target)
        {
            // skip friendly fire
            if (target is Creature c && ctx.Attacker != null &&
                _teamManager.GetAttitude(ctx.Attacker.Team, c.Team) == Attitude.Friendly)
                return;

            SpawnFloatingText(
                position: (target is Creature cr ? cr.Health.transform.position : target.transform.position),
                localizationKey: MissKey,
                type: FloatingTextType.Miss
            );
        }

        private void OnCreatureHeal(Creature healedCreature, HealContext ctx)
        {
            SpawnFloatingText(
                position: healedCreature.Health.transform.position,
                localizationKey: HealKey,
                type: FloatingTextType.Heal,
                ctx.HealAmount.ToLocalizedString2Decimals()
            );
        }

        private void OnCreatureHit(Creature creature, HitContext hitCtx)
        {
            if (hitCtx.Attacker?.Team == Teams.Player
                && hitCtx.Target is Creature { Team: Teams.Player })
            {
                // Basically, we don't want "Friendly Fire" floating text
                // to appear to close to the damage floating text.
                
                // Get value from 0.2 to 0.5 and randomly choose a positive or negative sign
                var offset = new Vector2(
                    UnityEngine.Random.Range(0.2f, 0.5f) * (UnityEngine.Random.value > 0.5f ? 1 : -1),
                    UnityEngine.Random.Range(0.2f, 0.5f) * (UnityEngine.Random.value > 0.5f ? 1 : -1
                    ));
                
                SpawnFloatingText(
                    position: hitCtx.Target.Health.transform.position + (Vector3)offset,
                    localizationKey: FriendlyFireKey,
                    type: FloatingTextType.FriendlyFire
                );
            }


            SpawnFloatingText(
                position: hitCtx.Target.Health.transform.position,
                localizationKey: DamageKey,
                type: FloatingTextType.Damage,
                hitCtx.Damage.ToLocalizedString2Decimals()
            );
        }

        public void SpawnFloatingText(
            Vector3 position,
            string localizationKey,
            FloatingTextType type,
            params object[] args
        )
        {
            // round early, before any pooling
            position = position.RoundToNearest(1f / 16f);
            
            var text = localizationKey.Localize(args);

            var popup = _dynamicPoolingManager.SpawnObject<FloatingTextUI>(
                prefabGameObject: floatingTextPrefab,
                position: position,
                parent: popupParent);

            var settings = _fontStyles[type];

            popup.Setup(text, settings.Color, settings.Size, settings.FontStyle);
            popup.Run();
        }

        // THE PROJECTILE MISS IS EVENT IS NOT USED ANYMORE
        // why?
        // coz not every attack has projectiles! but all attacks can miss (actually not all but fuck it)
        // void OnProjectileMissed(Projectile p, AttackContext ctx, Entity target)
        // {
        //     SpawnFloatingText(p.transform.position, MissKey, FloatingTextType.Miss);
        // }
    }
}