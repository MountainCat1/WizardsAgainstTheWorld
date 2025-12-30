using System;
using Components;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Entity Original { get; set; }
    
    public string GetIdentifier()
    {
        return Original ? Original.GetIdentifier() : gameObject.name;
    }
    
    public event Action<Vector2> Moved;

    // Components
    public MovementComponent Movement { get; private set; }
    public HealthComponent Health { get; private set; }

    public Transform RootTransform { get; protected set; }
    public Transform DisplayTransform { get; protected set; }

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
}