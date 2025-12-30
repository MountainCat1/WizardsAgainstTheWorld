using System;
using System.Collections.Generic;
using Items.PassiveItems;
using UnityEngine;

namespace Items
{
    public class PassiveItemBehaviour : ItemBehaviour
    {
        public override bool Stackable => false;

        public bool Active { get; private set; } = false;

        public IReadOnlyCollection<PassiveEffect> Effects => effects;

        [SerializeField] private List<PassiveEffect> effects = new();

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Use(ItemUseContext ctx)
        {
            base.Use(ctx);

            if (!Active)
            {
                Activate(ctx);
            }
            else
            {
                Deactivate();
            }
        }

        public override void UnEquip(ItemUnUseContext ctx)
        {
            base.UnEquip(ctx);

            if (Active)
            {
                Deactivate();
            }
        }

        private void Deactivate()
        {
            if (!Active)
                return;

            Active = false;

            InvokeDeactivated();
            OnDeactivation();
        }

        private void Activate(ItemUseContext ctx)
        {
            Active = true;

            InvokeActivated();
            OnActivation(ctx);
        }

        protected virtual void OnDeactivation()
        {
            effects.ForEach(x => x.Deactivate());
        }

        protected virtual void OnActivation(ItemUseContext ctx)
        {
            effects.ForEach(x => x.Activate(new PassiveEffectContext(ctx.Creature)));
        }
    }
}