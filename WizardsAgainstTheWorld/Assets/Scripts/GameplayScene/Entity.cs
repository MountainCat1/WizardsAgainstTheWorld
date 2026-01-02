using System;
using Components;
using Components.Creatures;
using Items.PassiveItems;
using Managers;
using UI;
using UnityEngine;
using Zenject;

public class Entity : MonoBehaviour, IDamageable
{
    public Entity Original { get; set; }
    
    public string GetIdentifier()
    {
        return Original ? Original.GetIdentifier() : gameObject.name;
    }
    
    // Events
    public event Action<Vector2> Moved;
    public event Action<Interaction> Interacted;
    public event Action<Interaction> InteractionCanceled;

    // Components
    public MovementComponent Movement { get; private set; }
    public HealthComponent Health { get; private set; }

    public Transform RootTransform { get; protected set; }
    public Transform DisplayTransform { get; protected set; }
    
    
    // Settings
    [field: SerializeField] public Teams Team { get; private set; }
    [field: SerializeField] public float ColliderSize { get; set; } = 0.5f;
    
    // Accessors
    public ModifierReceiver ModifierReceiver { get; } = new();
    public ILevelSystem LevelingComponent => _levelingComponent;

    
    // Private Referenes
    private ILevelSystem _levelingComponent;

    
    // Dependency Injections
    [Inject] private ITeamManager _teamManager;
    [Inject] private IFloatingTextManager _floatingTextManager;
    
    

    protected virtual void Awake()
    {
        RootTransform = transform;
        DisplayTransform = GetComponentInChildren<CreatureAnimator>()?.transform ?? RootTransform;

        // Initialize MovementComponent
        Movement = GetComponent<MovementComponent>();
        if (Movement)
        {
            Movement.Moved += OnMoved;
        }

        // Initialize HealthComponent
        Health = GetComponent<HealthComponent>();
        if (Health)
        {
            Health.Death += HandleDeath;
        }
        
        _levelingComponent = GetComponent<LevelingComponent>();
        _levelingComponent ??= new DisabledLevelingSystem();
    }

    protected virtual void Start()
    {
    }
    
    protected virtual void Update()
    {
    }

    public void SetMovement(Vector2 direction)
    {
        Movement?.SetMovement(direction);
    }

    public void Push(Vector2 push)
    {
        Movement?.Push(push);
    }

    public void Damage(HitContext ctx)
    {
        Health.Damage(ctx);
    }
    
    public void Move(Vector2 vector2)
    {
        Movement?.Move(vector2);
    }

    public void Heal(HealContext ctx)
    {
        Health.Heal(ctx);
    }

    private void OnMoved(Vector2 velocity)
    {
        Moved?.Invoke(velocity);
    }

    private void HandleDeath(DeathContext context)
    {
        // Additional death-related logic for Entity
        Destroy(gameObject);
    }
    
    public Attitude GetAttitudeTowards(Entity other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other == this)
            return Attitude.Friendly;

        return _teamManager.GetAttitude(Team, other.Team); // TODO: throws error?
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

}