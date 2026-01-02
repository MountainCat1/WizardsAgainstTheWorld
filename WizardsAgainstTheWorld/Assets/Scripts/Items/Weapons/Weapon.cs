using System;
using Components;
using Items;
using Items.Reload;
using JetBrains.Annotations;
using Managers;
using Managers.Visual;
using UnityEngine;
using Zenject;

public enum WeaponVisual
{
    None = 0,
    RangedLong = 2,
    RangedShort = 3,
    RangedPistol = 4,
    MeleeSword = 100,
    MeleeEnergySword = 101,
    MeleeEnergyMace = 102,
}

public abstract class Weapon : ItemBehaviour
{
    public event Action<AttackContext> Attacked;
    public event Action<HitContext> Hit;
    public event Action<AttackContext, Entity> Missed;
    public event Action Reloaded;
    public event Action Stroked;

    [Inject] private ISoundPlayer _soundPlayer;
    [Inject] private ICameraShakeService _cameraShakeService;
    [Inject] protected ITeamManager TeamManager;
    [Inject] protected IWeaponManager WeaponManager;

    [field: SerializeField] public float BaseAttackSpeed { get; set; }
    [field: SerializeField] public float BaseRange { get; set; }
    [field: SerializeField] public float BaseDamage { get; set; }
    [field: SerializeField] public float BaseAccuracyPercent { get; set; } = 100f; // Base accuracy percentage
    [field: SerializeField] public float PushFactor { get; set; }
    [field: SerializeField] public float ShakeFactor { get; set; }
    [field: SerializeField] public AudioClip HitSound { get; set; }
    [field: SerializeField] public AudioClip AttackSound { get; set; }
    [field: SerializeField] public WeaponVisual VisualType { get; private set; } = WeaponVisual.RangedLong;
    [field: SerializeField] public WeaponReloadComponent ReloadComponent { get; private set; }
    public virtual bool AllowToMoveOnCooldown => false;
    public virtual bool NeedsLineOfSight => false;
    public virtual bool ShootThroughAllies => false;
    public override bool Stackable => false;
    public override EquipmentSlot EquipmentSlot => EquipmentSlot.Weapon;

    public ItemData WeaponItemData { get; private set; } = null!;

    public bool IsOnCooldown => GetOnCooldown(new AttackContext());
    public float Range => WeaponItemData.GetApplied(WeaponPropertyModifiers.Range, BaseRange);


    private float _lastAttackTime = -1;

    private void Start()
    {
        if (ReloadComponent)
            ReloadComponent.Reloaded += OnReload;
    }

    private void OnReload()
    {
        Reloaded?.Invoke();
    }

    private void OnEnable()
    {
        WeaponManager.RegisterWeapon(this);
    }

    private void OnDisable()
    {
        WeaponManager.UnregisterWeapon(this);
    }

    public bool GetOnCooldown(AttackContext ctx)
    {
        return Time.time - _lastAttackTime < 1f / CalculateAttackSpeed(ctx);
    }

    public float CooldownPassedTime()
    {
        return Time.time - _lastAttackTime;
    }

    public void PerformAttack(AttackContext ctx)
    {
        if (ReloadComponent is not null)
        {
            if (ReloadComponent.CurrentAmmo <= 0)
            {
                ReloadComponent.DoReloading(ctx.Attacker);
                return;
            }

            ReloadComponent.ConsumeAmmo();
        }

        if (ctx.Weapon != null)
            GameLogger.LogError("PerformAttack: ctx.Weapon is not null, this should never happen");

        ctx.Weapon = this;

        _lastAttackTime = Time.time;

        _cameraShakeService.ShakeCamera(ctx.Attacker.transform.position, ShakeFactor);

        if (AttackSound != null)
            _soundPlayer.PlaySound(AttackSound, transform.position);

        Attacked?.Invoke(ctx);
        Attack(ctx);
    }

    public void ContinuousAttack(AttackContext ctx)
    {
        if (GetOnCooldown(ctx))
            return;

        PerformAttack(ctx);
    }

    // !!!
    // Should always ONLY be invoked by PerformAttack, to ensure that the weapon is on cooldown and events are invoked
    // !!!
    protected abstract void Attack(AttackContext ctx);

    protected virtual void OnHit(IDamageable damageable, HitContext hitContext)
    {
        if (damageable != null)
        {
            damageable.Health.Damage(hitContext);
        }

        if (HitSound != null)
        {
            _soundPlayer.PlaySound(
                HitSound,
                hitContext.TargetPosition,
                SoundType.Sfx,
                true
            ); // TODO: throws exception if null
        }

        Hit?.Invoke(hitContext);
    }

    protected Vector2 CalculatePushForce(Creature target)
    {
        var direction = (target.transform.position - transform.position).normalized;
        // var pushForce = direction * (PushFactor * (BaseDamage / target.Health.MaxValue));
        // return pushForce;

        return direction;
    }

    public override void Use(ItemUseContext ctx)
    {
        base.Use(ctx);

        ctx.Creature.Inventory.EquipItem(this);
    }

    public override void Equip(ItemUseContext ctx)
    {
        base.Equip(ctx);

        if (ctx.Creature.Weapon != this)
        {
            ctx.Creature.StartUsingWeapon(this);
        }
    }

    public override void UnEquip(ItemUnUseContext ctx)
    {
        base.UnEquip(ctx);

        if (ctx.Creature.Weapon == this)
        {
            ctx.Creature.StartUsingWeapon(null);
        }
    }

    private float CalculateAttackSpeed(AttackContext ctx)
    {
        var attackSpeed = WeaponItemData.GetApplied(WeaponPropertyModifiers.AttackSpeed, BaseAttackSpeed);

        if (ctx.Attacker == null)
            return attackSpeed;

        attackSpeed *= ctx.Attacker.ModifierReceiver.AttackSpeedModifier;

        var dexterityModifier = ctx.Attacker.LevelingComponent.CharacteristicsLevels[Characteristics.Dexterity] *
                                CharacteristicsConsts.AttackSpeedAdditiveMultiplierPerDexterity;

        return attackSpeed * (1 + dexterityModifier);
    }

    public static float CalculateDamage(float baseDamage, AttackContext ctx)
    {
        var strengthModifier = ctx.Attacker.LevelingComponent.CharacteristicsLevels[Characteristics.Strength] *
                               CharacteristicsConsts.DamageAdditiveMultiplierPerStrength;

        var modifier = ctx.Attacker.ModifierReceiver.DamageModifier;

        return baseDamage * modifier + strengthModifier;
    }

    public static float CalculateAccuracy(float baseAccuracy, AttackContext ctx)
    {
        var intelligenceModifier = ctx.Attacker.LevelingComponent.CharacteristicsLevels[Characteristics.Intelligence] *
                                   CharacteristicsConsts.AccuracyAdditiveMultiplierPerIntelligence;

        var modifier = ctx.Attacker.ModifierReceiver.AccuracyFlatModifier;

        return baseAccuracy * modifier + intelligenceModifier;
    }

    public override void SetData(ItemData itemData)
    {
        base.SetData(itemData);

        WeaponItemData = itemData;
    }

    protected void InvokeStroked()
    {
        Stroked?.Invoke();
    }

    protected void InvokeMissed(AttackContext ctx, Entity missedEntity)
    {
        Missed?.Invoke(ctx, missedEntity);
    }

    public bool IsInRange(AttackContext attackContext)
    {
        var attacker = attackContext.Attacker;
        var target = attackContext.Target;
        
        if (attacker is null || target is null)
            return false;
        
        var distance = Vector2.Distance(attacker.transform.position, target.transform.position);
        return distance <= Range + attacker.ColliderSize + target.ColliderSize;
    }
}


public struct AttackContext
{
    public Vector2 Direction { get; set; }
    public Vector2 TargetPosition { get; set; }
    [CanBeNull] public Entity Target { get; set; }
    [CanBeNull] public Entity Attacker { get; set; }
    public Teams Team { get; set; }
    public Weapon Weapon { get; set; }

    public AttackContext(
        Vector2 direction,
        Vector2 targetPosition,
        [CanBeNull] Creature target,
        [CanBeNull] Creature attacker,
        Weapon weapon,
        Teams team
    )
    {
        Direction = direction;
        TargetPosition = targetPosition;
        Target = target;
        Attacker = attacker;
        Weapon = weapon;
        Team = team;
    }
}

public struct HitContext
{
    public HitContext(Creature attacker,
        IDamageable target,
        float damage,
        Vector2 position,
        float pushFactor = 1
    )
    {
        Attacker = attacker;
        Target = target;

        if (target == null)
        {
            GameLogger.LogError("HitContext: Target is null");
            throw new ArgumentNullException(nameof(target), "Target cannot be null in HitContext");
        }

        _damage = damage;
        PushFactor = pushFactor;
        OriginalDamage = damage;
        TargetPosition = position;
    }

    public Entity Attacker { get; set; }
    public IDamageable Target { get; set; }
    public Vector2 TargetPosition { get; set; }

    public float Damage
    {
        readonly get => _damage;
        set
        {
            OriginalDamage ??= value;
            _damage = value;
        }
    }

    private float _damage;

    public float? OriginalDamage { get; private set; }
    public float PushFactor { get; set; }
    public Vector2 Push => GetPushForce();

    public Vector3 Direction => Target == null || Attacker == null
        ? Vector3.zero
        : (Target.Health.transform.position - Attacker.transform.position).normalized;

    public void ValidateAndLog()
    {
        // If the attacker is dead then the attacker is null, so we can't check for null
        // TODO: Save dead creatures instead of removing them from memory
        // if (Attacker == null)
        //     GameLogger.LogError("Attacker is null");

        if (Target == null)
            GameLogger.LogError("Target is null");

        if (Damage <= 0)
            GameLogger.LogError("Damage is less than or equal to 0");
    }

    public Vector2 GetPushForce()
    {
        if (Attacker == null)
        {
            GameLogger.LogWarning("Attacker is null, cannot calculate push force");
            return Vector2.zero;
        }
        
        var direction = (Target.Health.transform.position - Attacker.transform.position).normalized;
        var pushForce = direction * (PushFactor * (Damage / Target.Health.MaxValue));
        return pushForce;
    }
}