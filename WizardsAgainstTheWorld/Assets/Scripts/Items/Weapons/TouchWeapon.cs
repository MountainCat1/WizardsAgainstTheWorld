using Combat;
using Components;
using Items;
using UnityEngine;

public class TouchWeapon : Weapon, IArmorSource
{
    [SerializeField] private float defense = 0;

    public override void Use(ItemUseContext ctx)
    {
        base.Use(ctx);
    }

    public override void Equip(ItemUseContext ctx)
    {
        base.Equip(ctx);
        
        if (Creature != null)
        {
            Creature?.Health.ArmorSources.Add(this);
        }
    }

    public override void UnEquip(ItemUnUseContext ctx)
    {
        base.UnEquip(ctx);

        if (Creature != null)
        {
            Creature?.Health.ArmorSources.Remove(this);
        }
    }

    protected override void Attack(AttackContext ctx)
    {
        var damage = WeaponItemData.GetApplied(WeaponPropertyModifiers.Damage, BaseDamage);
        var accuracyPercentage = WeaponItemData.GetApplied(WeaponPropertyModifiers.Accuracy, BaseAccuracyPercent);
        
        var miss = HitChanceCalculator.ShouldMiss(HitChanceSettings.Enemy, accuracyPercentage / 100f);

        if (miss)
        {
            InvokeMissed(ctx, ctx.Target);
            return;
        }
        
        // TODO: add missing logic
        var hitCtx = new HitContext()
        {
            Attacker = ctx.Attacker,
            Damage = CalculateDamage(damage, ctx),
            Target = ctx.Target,
            PushFactor = PushFactor
        };

        OnHit(ctx.Target, hitCtx);

        InvokeStroked();
    }

    public float FlatArmor => 0f;
    public float PercentArmor => defense;
}