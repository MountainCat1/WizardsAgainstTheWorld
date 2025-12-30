using System.Collections.Generic;
using Items.PassiveItems;
using Skills;
using UnityEngine;

public class SkillContext
{
    public Creature Creature { get; private set; }
    public SkillDescriptor SkillDescriptor { get; private set; }

    public SkillContext(SkillDescriptor skillDescriptor, Creature creature)
    {
        Creature = creature;
    }
}

namespace Skills
{
    public class SkillDescriptor : MonoBehaviour
    {
        public string GetIdentifier() => Name;
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Sprite Icon { get; set; }

        [field: TextArea]
        [field: SerializeField]
        private string BaseDescription { get; set; } // TODO: should be removed maybe?

        [field: SerializeField] public int Cost { get; set; } = 1;
        public ICollection<PassiveEffect> GetEffects() => 
            GetComponents<PassiveEffect>();
        public string NameKey => $"UI.Skills.{Name}.Name";

        public SkillData ToData()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new SkillData
            {
                SkillID = GetIdentifier(),
                Cost = Cost
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void ApplySkill(Creature creature)
        {
            foreach (var effect in GetComponents<PassiveEffect>())
            {
                ApplyEffect(creature, effect);
            }
        }

        private void ApplyEffect(Creature creature, PassiveEffect effect)
        {
            if (effect == null)
            {
                Debug.LogError("SkillDescriptor: ApplyEffect - effect is null.");
                return;
            }

            creature.EffectReceiver.AddEffect(effect);
        }

        // public string GetDescription(int count = 0)
        // {
        //     var skillApplies = GetComponents<PassiveEffect>();
        //
        //     string description = $"{Name}\n---";
        //
        //     foreach (var descriptor in skillApplies)
        //     {
        //         description += $"\n{descriptor.GetDescription(count)}";
        //     }
        //
        //     return description;
        // }

        // public ICollection<SkillApply.SkillRow> GetSkillRows()
        // {
        //     var skillApplies = GetComponents<SkillApply>();
        //     var skillRows = new List<SkillApply.SkillRow>();
        //
        //     foreach (var descriptor in skillApplies)
        //     {
        //         skillRows.AddRange(descriptor.GetSkillRows());
        //     }
        //
        //     return skillRows;
        // }
    }
}