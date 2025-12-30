using UnityEngine;
using Zenject;

namespace Managers
{
    public interface IStatusEffectManager
    {
        public StatusEffect ApplyStatusEffect(StatusEffect statusEffectPrefab, StatusEffectContext ctx);
    }

    public class StatusEffectManager : MonoBehaviour, IStatusEffectManager
    {
        [Inject] private ISpawnerManager _spawnerManager;

        public StatusEffect ApplyStatusEffect(StatusEffect statusEffectPrefab, StatusEffectContext ctx)
        {
            var existingStatusEffects = ctx.Target.GetComponentsInChildren<StatusEffect>();
            var existingStatusEffect = System.Array.Find(
                array: existingStatusEffects,
                match: effect => effect.Name == statusEffectPrefab.Name
            );
            
            if (existingStatusEffect is not null)
            {
                existingStatusEffect.ExtendDuration(statusEffectPrefab.Duration);
                
                return existingStatusEffect;
            }

            var statusEffect = _spawnerManager.Spawn(
                prefab: statusEffectPrefab,
                parent: ctx.Target.transform
            );

            statusEffect.Initialize(ctx);

            return statusEffect;
        }
    }
}