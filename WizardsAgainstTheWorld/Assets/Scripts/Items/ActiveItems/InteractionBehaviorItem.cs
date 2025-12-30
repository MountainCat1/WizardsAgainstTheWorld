using System;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using Zenject;

namespace Items.ActiveItems
{
    public class InteractionBehaviorItem : ActiveItemBehaviour, IInteractable 
    {
        public event Action<Creature> InteractCompleted;
    
        [Inject] private ISoundPlayer _soundPlayer = null!;

        [CanBeNull] private Interaction _interaction;

        [SerializeField] private float interactionTime = 1f;
        [SerializeField] private AudioClip interactionSound;
        [SerializeField] private AudioClip startInteractionSound;
        
        public string MessageKey => $"Items.Interactions.{Name}";

        public Vector2 Position => transform.position;
        public virtual bool IsInteractable => true;
        public int Priority => 0; // Default priority, not useful for items in inventory i think...

        public ICollection<IInteractable> Container { get; private set; } 

        private AbilityUseContext _context;

        protected override void Awake()
        {
            base.Awake();
            
            Container = GetComponents<IInteractable>().OrderByDescending(x => x.Priority).ToArray();
        }

        public virtual bool CanInteract(Creature creature)
        {
            return true;
        }

        public sealed override void UseActiveAbility(AbilityUseContext context)
        {
            base.UseActiveAbility(context);

            if (!CanInteract(context.User))
                throw new InvalidOperationException("Creature cannot interact with this item");

            var controller = context.User.Controller as UnitController;
            if(controller == null)
                throw new InvalidOperationException("Creature must have a UnitController to interact with this item");
            
            _context = context;
            
            controller.SetTarget(this);
        }

        public Interaction Interact(Creature creature, float deltaTime)
        {
            if (_interaction == null)
            {
                _interaction = CreateInteraction(creature);
                return _interaction;
            }

            if (_interaction.Creature != creature)
            {
                _interaction.Cancel();
                _interaction = CreateInteraction(creature);
                return _interaction;
            }

            var returnInteraction = _interaction; // We make this temp so when it becomes null on OnCancel or OnComplete
            // we won't lose the reference 

            _interaction.Progress((decimal)deltaTime);

            return returnInteraction;
        }

        private Interaction CreateInteraction(Creature creature)
        {
            _interaction = new Interaction(creature, interactionTime, MessageKey);

            _interaction.Completed += () =>
            {
                if (interactionSound)
                    _soundPlayer.PlaySound(interactionSound, Position);
            
                try
                {
                    OnInteractionCompleteCallback(_interaction);
                }
                finally
                {
                    _interaction = null;
                }
            };
            _interaction!.Canceled += () => { _interaction = null; };

            if (startInteractionSound)
                _soundPlayer.PlaySound(startInteractionSound, Position);
        
            return _interaction;
        }

        private void OnInteractionCompleteCallback(Interaction interaction)
        {
            InteractCompleted?.Invoke(interaction.Creature);
            
            OnInteractionCompleted(interaction, _context);
        }
        
        protected virtual void OnInteractionCompleted(Interaction interaction, AbilityUseContext context)
        {
            // To be overridden by inheriting classes
        }
    }
}