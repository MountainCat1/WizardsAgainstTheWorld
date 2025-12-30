using System;
using LevelSelector.UI;
using Zenject;

namespace Items.ItemInteractions
{
    public class ItemInteractionContext
    {
        public ItemBehaviour Item { get; set; }
        public DiContainer DiContainer { get; set; }
        public CreatureData CreatureData { get; set; }

        public T Resolve<T>()
        {
            return DiContainer.Resolve<T>();
        }
    }
    
    
    public class ItemInteraction
    {
        public string Name { get; }
        public bool UseOnce { get; }
        public bool PerCharacter { get; }
        public string NameKey => $"Items.Interactions.{Name}.Name";
        public string DescriptionKey => $"Items.Interactions.{Name}.Description";
        public bool Enabled { get; }
        
        private Action<ItemInteractionContext> _interaction;
        
        public ItemInteraction(string name, Action<ItemInteractionContext> interaction, bool useOnce = true, bool perCharacter = false, bool enabled = true)
        {
            Name = name;
            UseOnce = useOnce;
            PerCharacter = perCharacter;
            Enabled = enabled;
            
            _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction), "Interaction action cannot be null");
        }
        
        public virtual void Interact(ItemInteractionContext ctx)
        {
            _interaction?.Invoke(ctx);
        }
    }
}