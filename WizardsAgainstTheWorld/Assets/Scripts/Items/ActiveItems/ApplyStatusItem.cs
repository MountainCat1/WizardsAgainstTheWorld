using Managers;
using UnityEngine;
using Zenject;

namespace Items.ActiveItems
{
    public class ApplyStatusItem : ActiveItemBehaviour
    {
        [Inject] private IStatusEffectManager _statusEffectManager;     
        
        [SerializeField] private StatusEffect statusEffectPrefab;
        
        public override void UseActiveAbility(AbilityUseContext context)
        {
            base.UseActiveAbility(context);
            
            _statusEffectManager.ApplyStatusEffect(
                statusEffectPrefab: statusEffectPrefab,
                ctx: new StatusEffectContext(context.User, context.User)
            );
            
            Inventory.RemoveItems(GetIdentifier(), 1);
        }
    }
}