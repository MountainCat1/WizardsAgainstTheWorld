using System.Collections.Generic;
using Components;
using Items.ItemInteractions;
using Managers;
using UnityEngine;
using Zenject;

namespace Items.ActiveItems
{
    public class MedkitItem : InteractionBehaviorItem
    {
        [Inject] private ISoundPlayer _soundPlayer;

        [SerializeField] private float healAmount;
        [SerializeField] private AudioClip useSound;

        public override bool Stackable => true;
        
        protected override void OnInteractionCompleted(Interaction interaction, AbilityUseContext context)
        {
            base.OnInteractionCompleted(interaction, context);
            
            var health = context.User.Health;

            health.Heal(new HealContext()
            {
                Healer = context.User,
                Target = context.User,
                HealAmount = healAmount
            });

            Inventory.RemoveItems(GetIdentifier(), 1);

            if (useSound)
                _soundPlayer.PlaySound(useSound, context.User.transform.position, SoundType.Sfx);
        }

        // TODO: uncomment this when you will be implementing persistant health
        // public override IEnumerable<ItemInteraction> Interactions => new []
        // {
        //     new ItemInteraction("Medkit", HealItemInteraction, useOnce: true, perCharacter: true)
        // };

        private void HealItemInteraction(ItemInteractionContext obj)
        {
            GameLogger.Log($"Using Medkit on {obj.CreatureData.Name}.");
        }
    }
}