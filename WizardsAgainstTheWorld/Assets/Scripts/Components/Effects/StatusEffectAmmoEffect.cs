using System;
using Managers;
using UnityEngine;
using Zenject;

namespace Items.PassiveItems
{
    public class StatusEffectAmmoEffect : PassiveEffect
    {
        [Inject] private IStatusEffectManager _statusEffectManager;
        
        public StatusEffect StatusEffectPrefab => statusEffectPrefab;
        
        [SerializeField] private StatusEffect statusEffectPrefab;

        protected override void OnActivation()
        {
            base.OnActivation();
            
            var creature = Creature ?? throw new NullReferenceException("Creature is null");
            
            creature.WeaponHit += OnWeaponHit;
        }

        protected override void OnDeactivation()
        {
            base.OnDeactivation();

            var creature = Creature;
            
            if(creature is null)
                return;
            
            creature.WeaponHit -= OnWeaponHit;
        }
        
        private void OnWeaponHit(HitContext ctx)
        {
            if (ctx.Target is not Creature targetEntity)
            {
                return;
            }

            var statusEffectContext = new StatusEffectContext(ctx.Attacker, targetEntity);
            
            _statusEffectManager.ApplyStatusEffect(statusEffectPrefab, statusEffectContext);
        }
    }
}