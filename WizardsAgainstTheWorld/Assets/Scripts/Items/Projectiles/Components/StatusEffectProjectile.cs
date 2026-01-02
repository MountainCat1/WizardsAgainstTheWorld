using Components;
using Items.Weapons;
using Managers;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Projectile))]
public class StatusEffectProjectile : MonoBehaviour
{
    [Inject] private IStatusEffectManager _statusEffectManager;

    [SerializeField] private StatusEffect statusEffectPrefab;

    private Projectile _projectile;

    private void Awake()
    {
        _projectile = GetComponent<Projectile>();
        _projectile.Hit += OnHit;
    }

    private void OnHit(IDamageable target, AttackContext ctx)
    {
        if (target is not Creature targetCreature)
        {
            return;
        }
        
        if(ctx.Attacker is not Creature attacker)
        {
            return;
        }
        
        var statusEffectContext = new StatusEffectContext(attacker, targetCreature);

        _statusEffectManager.ApplyStatusEffect(statusEffectPrefab, statusEffectContext);
    }
}