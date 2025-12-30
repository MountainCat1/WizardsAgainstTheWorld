using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
using Managers;
using UI;
using UnityEngine;
using Zenject;

public class InteractionBehavior : MonoBehaviour, IInteractable, IHoverable
{
    public event Action<Creature> InteractCompleted;

    [Inject] private ISoundPlayer _soundPlayer = null!;
    [Inject] private ICursorManager _cursorManager = null!;
    [Inject] private IFloatingTextManager _floatingTextManager = null!;

    [SerializeField] private float interactionTime = 1f;
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioClip startInteractionSound;
    [SerializeField] private bool useOnce = true;
    [SerializeField] private bool multiUse = false;
    [SerializeField] private string message = "Interacting...";
    [SerializeField] public List<ItemBehaviour> requiredItems;
    [SerializeField] public Texture2D cursor;
    [SerializeField] public int priority = 0;

    public bool IsInteractable => GetIsInteractable();
    public Vector2 Position => transform.position;
    public bool Occupied => _activeInteractions.Count > 0;
    public int Priority => priority;

    public bool Used { get; protected set; } = false;

    private readonly Dictionary<Creature, Interaction> _activeInteractions = new();

    protected ICollection<IInteractable> OtherInteractables { get; private set; }
    public ICollection<IInteractable> Container { get; private set; }

    protected virtual void Awake()
    {
        OtherInteractables = GetComponents<IInteractable>().Where(x => !ReferenceEquals(x, this)).ToArray();
        Container = GetComponents<IInteractable>().OrderByDescending(x => x.Priority).ToArray();
    }

    protected virtual void Start()
    {
    }

    private void OnMouseEnter()
    {
        if (OtherInteractables.Any(x => x.IsInteractable))
        {
            var maxOtherPrirority = OtherInteractables
                .Where(x => x.IsInteractable)
                .Max(x => x.Priority);

            if (maxOtherPrirority == Priority)
            {
                GameLogger.LogError(
                    $"Multiple interactables with the same priority ({Priority}) at {Position}, object {name} " +
                    "This can cause unexpected behavior. Please ensure unique priorities for each interactable."
                );
            }

            if (maxOtherPrirority > Priority)
            {
                // If another interactable has a higher priority, do not set the cursor
                _cursorManager.RemoveCursor(this);
                return;
            }
        }

        if (cursor && IsInteractable && (!Used || !useOnce))
        {
            _cursorManager.SetCursor(this, cursor, CursorPriority.Interactable);
        }
    }

    private void OnMouseExit()
    {
        _cursorManager.RemoveCursor(this);
    }

    private bool GetIsInteractable()
    {
        if (Used && useOnce)
            return false;

        return true;
    }

    public virtual bool CanInteract(Creature creature)
    {
        if (!IsInteractable)
            return false;

        if (requiredItems != null)
        {
            foreach (var requiredItem in requiredItems)
            {
                if (!creature.Inventory.HasItem(requiredItem.GetIdentifier()))
                    return false;
            }
        }

        if (!multiUse && _activeInteractions.Count > 0 && !_activeInteractions.ContainsKey(creature))
            return false;

        return true;
    }

    /// <summary>
    /// To be used if you want to prevent the interaction in some circumstances
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="interaction"></param>
    /// <param name="messageKey"></param>
    /// <returns>Should the interaction proceed?</returns>
    protected virtual bool ShouldContiniueInteraction(Creature creature, Interaction interaction, [CanBeNull] out string messageKey)
    {
        messageKey = null;
        return true;
    }

    public virtual Interaction Interact(Creature creature, float deltaTime)
    {
        if (_activeInteractions.TryGetValue(creature, out var interaction))
        {
            var cancel = !ShouldContiniueInteraction(creature, interaction, out var messageKey);

            if (cancel)
            {
                interaction.Cancel();
                _activeInteractions.Remove(creature);

                if (messageKey != null)
                    _floatingTextManager.SpawnFloatingText(
                        Position,
                        messageKey,
                        FloatingTextType.InteractionCancelled
                    );

                return interaction;
            }

            interaction.Progress((decimal)deltaTime);
            return interaction;
        }

        if (!multiUse && _activeInteractions.Count > 0)
        {
            // Single-use at a time: cancel others
            foreach (var other in _activeInteractions.Values)
                other.Cancel();

            _activeInteractions.Clear();
        }

        var newInteraction = CreateInteraction(creature);
        _activeInteractions[creature] = newInteraction;
        return newInteraction;
    }

    private Interaction CreateInteraction(Creature creature)
    {
        var interaction = new Interaction(creature, interactionTime, message);

        interaction.Completed += () =>
        {
            if (interactionSound)
                _soundPlayer.PlaySound(interactionSound, Position);

            if (useOnce)
                Used = true;

            OnInteractionComplete(interaction);
            _activeInteractions.Remove(creature);

            if (useOnce && !multiUse)
            {
                foreach (var other in _activeInteractions.Values)
                    other.Cancel();

                _activeInteractions.Clear();
            }
        };

        interaction.Canceled += () => { _activeInteractions.Remove(creature); };

        if (startInteractionSound)
            _soundPlayer.PlaySound(startInteractionSound, Position);

        return interaction;
    }

    protected virtual void OnInteractionComplete(Interaction interaction)
    {
        if (useOnce)
            _cursorManager.RemoveCursor(this);

        InteractCompleted?.Invoke(interaction.Creature);
    }
}