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
        
        var statusEffectContext = new StatusEffectContext(ctx.Attacker, targetCreature);

        _statusEffectManager.ApplyStatusEffect(statusEffectPrefab, statusEffectContext);
    }
}