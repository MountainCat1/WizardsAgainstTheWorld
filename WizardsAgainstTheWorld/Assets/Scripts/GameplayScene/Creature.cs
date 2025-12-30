using System;
using Components;
using Components.Creatures;
using Items;
using Items.PassiveItems;
using Managers;
using UI;
using UnityEngine;
using Zenject;

public enum CreatureState
{
    Idle,
    Moving,
    Attacking,
    Utility
}

[RequireComponent(typeof(Rigidbody2D))]
public class Creature : Entity, IDamageable, IModifiable, IColorable, IEffectable
{
    public static float ColliderSize => 0.5f;

    // Events
    public event Action WeaponChanged;
    public event Action ArmorChanged;
    public event Action<CreatureState> StateChanged;
    public event Action<Interaction> Interacted;
    public event Action<Interaction> InteractionCanceled;
    public event Action<AttackContext> WeaponAttacked;
    public event Action<HitContext> WeaponHit;
    public event Action WeaponReloaded;
    public event Action Disabled;

    // Injected Dependencies (using Zenject)
    [Inject] private ITeamManager _teamManager;
    [Inject] private DiContainer _diContainer;
    [Inject] private ICreatureManager _creatureManager;
    [Inject] private IFloatingTextManager _floatingTextManager;

    // Public Variables
    public CreatureState State
    {
        get => _state;
        private set
        {
            if (_state == value)
                return;

            _state = value;
            StateChanged?.Invoke(_state);
        }
    }

    private CreatureState _state = CreatureState.Idle;

    // Serialized Private Variables
    [field: Header("References")] [field: SerializeField]
    private Transform inventoryRoot;

    [field: SerializeField] private Weapon weapon;
    [field: SerializeField] private ArmorItem armorItem;

    public Weapon Weapon
    {
        get => weapon;
        private set
        {
            if (weapon != null)
            {
                weapon.Reloaded -= OnWeaponReloaded;
                weapon.Attacked -= OnWeaponAttacked;
                weapon.Hit -= OnWeaponHit;
            }

            weapon = value;

            if (value != null)
            {
                weapon.Reloaded += OnWeaponReloaded;
                weapon.Attacked += OnWeaponAttacked;
                weapon.Hit += OnWeaponHit;
            }

            WeaponChanged?.Invoke();
        }
    }

    private void OnWeaponReloaded()
    {
        WeaponReloaded?.Invoke();
    }

    public ArmorItem Armor
    {
        get => armorItem;
        private set
        {
            Health.ArmorSources.Remove(armorItem);

            armorItem = value;
            if (value is not null)
                Health.ArmorSources.Add(value);

            ArmorChanged?.Invoke();
        }
    }

    [field: SerializeField] public float SightRange { get; private set; } = 13f;
    [field: SerializeField] public int XpAmount { get; private set; }

    [field: Header("Other")]
    [field: SerializeField]
    public Teams Team { get; private set; }

    [field: SerializeField] public bool Boss { get; private set; }

    [field: SerializeField] public float InteractionRange { get; private set; } = 1.5f;
    [field: SerializeField] public Color Color { get; set; }


    // Accessors
    public ModifierReceiver ModifierReceiver { get; } = new();
    public CreatureController Controller => _controller;
    public Inventory Inventory => _inventory;
    public ILevelSystem LevelingComponent => _levelingComponent;
    public IEffectReceiver EffectReceiver => new CreatureEffectReceiver(this);

    // Private Referenes

    private CreatureController _controller;
    private ILevelSystem _levelingComponent;
    private Inventory _inventory;


    // Unity Callbacks
    protected override void Awake()
    {
        base.Awake();

        if (weapon != null)
        {
            Weapon = weapon;
        }

        if (armorItem != null)
        {
            Armor = armorItem;
        }

        if (inventoryRoot == null)
        {
            inventoryRoot = new GameObject("Inventory").transform;
            inventoryRoot.SetParent(RootTransform);
            inventoryRoot.localPosition = Vector3.zero;
            GameLogger.LogWarning("Inventory root is not set, creating a new one");
        }

        _controller = GetComponent<CreatureController>();

        _inventory = new Inventory(inventoryRoot, this);
        _diContainer.Inject(Inventory);

        Health.Hit += OnHit;

        _levelingComponent = GetComponent<LevelingComponent>();
        _levelingComponent ??= new DisabledLevelingSystem();
    }

    protected override void Update()
    {
        base.Update();

        if (weapon is not null && weapon.IsOnCooldown)
        {
            if (weapon.CooldownPassedTime() >= 1f && Movement is not null && Movement.MoveDirection.magnitude > 0)
            {
                State = CreatureState.Moving;
            }
            else
            {
                State = CreatureState.Attacking;
            }
        }
        else if (Movement is not null && Movement.MoveDirection.magnitude > 0)
            State = CreatureState.Moving;
        else
            State = CreatureState.Idle;
    }

    private void OnDestroy()
    {
        StartUsingWeapon(null);
    }

    private void OnDisable()
    {
        Disabled?.Invoke();
    }

    // Public Methods
    public Attitude GetAttitudeTowards(Creature other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other == this)
            return Attitude.Friendly;

        return _teamManager.GetAttitude(Team, other.Team); // TODO: throws error?
    }

    public void StartUsingWeapon(Weapon newWeapon)
    {
        Weapon = newWeapon;
    }

    public void StartUsingArmor(ArmorItem newArmor)
    {
        Armor = newArmor;
    }

    public void UseItem(ItemBehaviour item)
    {
        item.Use(new ItemUseContext()
        {
            Creature = this
        });
    }

    public void AwardXp(int amount)
    {
        _levelingComponent.AddXp(amount);
    }

    public void Initialize(CreatureData data)
    {
        name = data.Name;
        SightRange = data.SightRange;
        Color = data.Color.ToColor();
        Inventory.SetData(data.Inventory);
        LevelingComponent.SetData(data.Level);
    }

    public Interaction Interact(IInteractable interactionTarget)
    {
        var interaction = interactionTarget.Interact(this, Time.deltaTime); // TODO: throws exception???
        if (interaction.Status == InteractionStatus.Created)
        {
            _floatingTextManager.SpawnFloatingText(RootTransform.position, interaction.MessageKey,
                FloatingTextType.Interaction);
        }

        Interacted?.Invoke(interaction);
        interaction.Canceled += () => { InteractionCanceled?.Invoke(interaction); };
        return interaction;
    }

    // Static Methods

    public static bool IsCreature(GameObject go)
    {
        return go.CompareTag("Player") || go.CompareTag("Creature");
    }

    // Virtual Methods

    // Abstract Methods

    // Private Methods

    private void OnHit(HitContext ctx)
    {
        if (ctx.Attacker is not null && ctx.Push.magnitude > 0)
            Push(ctx.Push);
    }

    private void OnWeaponHit(HitContext ctx)
    {
        WeaponHit?.Invoke(ctx);
    }

    private void OnWeaponAttacked(AttackContext ctx)
    {
        WeaponAttacked?.Invoke(ctx);
    }
}

public struct DeathContext
{
    public Entity Killer { get; set; }
    public Entity KilledEntity { get; set; }
}