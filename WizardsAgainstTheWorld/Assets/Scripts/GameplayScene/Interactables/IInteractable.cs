using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool CanInteract(Creature creature);
    Interaction Interact(Creature creature, float deltaTime);
    public Vector2 Position { get; }
    bool IsInteractable { get; }
    int Priority { get; }
    ICollection<IInteractable> Container { get; }
}

public enum InteractionStatus
{
    Created,
    InProgress,
    Completed,
    Canceled
}

public class Interaction
{
    public event Action Completed;
    public event Action Canceled;
    public event Action Ended;
    
    public InteractionStatus Status { get; private set; } = InteractionStatus.Created;

    public decimal CurrentProgress { get; private set; }
    public Creature Creature { get; }
    public decimal InteractionTime { get; }
    public string MessageKey { get; }
    public Color Color { get; }

    public Interaction(Creature creature, float interactionTime, string messageKey = "", Color? color = null)
    {
        Creature = creature;
        MessageKey = messageKey;
        InteractionTime = (decimal)interactionTime;
        Color = color ?? new Color(0, 0.8784314f, 0);
    }
    
    /// <summary>
    /// <b>REMEMBER PLEASE</b> <br/>
    /// if you call this method, and it will finish the interaction, it will call OnComplete method.
    /// Which means most probably it will remove the interaction reference in your objects
    /// As mostly you will be doing that <code> interaction.OnComplete += () => { _interaction = null; }; </code>
    /// </summary>
    public Interaction Progress(decimal delta)
    {
        if(Status != InteractionStatus.Created && Status != InteractionStatus.InProgress)
            throw new InvalidOperationException("Cannot progress a completed or canceled interaction.");
        
        Status = InteractionStatus.InProgress;
        
        if(delta < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), "Delta must be positive.");
        
        CurrentProgress += delta;
        
        if (CurrentProgress >= InteractionTime)
        {
            OnComplete();
        }
        
        return this;
    }
    
    public void Cancel()
    {
        Status = InteractionStatus.Canceled;
        Canceled?.Invoke();
        Ended?.Invoke();
        
        GameLogger.Log($"{Creature.name} canceled the interaction.");
    }

    private void OnComplete()
    {
        Status = InteractionStatus.Completed;
        Completed?.Invoke();
        Ended?.Invoke();

        GameLogger.Log($"{Creature.name} completed the interaction.");
    }
}