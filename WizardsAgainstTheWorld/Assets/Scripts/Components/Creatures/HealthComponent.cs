using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Components
{
    using System;
    using UnityEngine;

    public interface IDamageable
    {
        HealthComponent Health { get; }
    }

    public struct HealContext
    {
        public Creature Healer { get; set; }
        public Creature Target { get; set; }
        public float HealAmount { get; set; }
    }


    public class HealthComponent : MonoBehaviour, IReadonlyRangedValue
    {
        public const float MinDamagePerHit = 0.1f;

        // Events
        public event Action<DeathContext> Death;
        public event Action ValueChanged;
        public event Action<HitContext> Hit;
        public event Action<HealContext> Healed;

        // Settings
        [Header("Health Settings")] [SerializeField]
        private float maxHealth = 10;

        // Properties

        public List<IArmorSource> ArmorSources { get; } = new();

        // Accessors
        public float CurrentValue => _rangeValue.CurrentValue;
        public float MinValue => _rangeValue.MinValue;
        public float MaxValue => _rangeValue.MaxValue;

        public bool Alive => _rangeValue.CurrentValue > _rangeValue.MinValue;

        // Fields
        private RangedValue _rangeValue;
        [CanBeNull] private IModifiable _modifiable;

        private void Awake()
        {
            _rangeValue = new RangedValue(maxHealth, 0f, maxHealth);
            _modifiable = GetComponent<IModifiable>();
        }

        public void Damage(HitContext ctx)
        {
            if (!gameObject.activeInHierarchy)
            {
                GameLogger.LogError("Trying to damage a disabled object");
                return;
            }

            ctx.ValidateAndLog();


            ctx.Damage = CalculateFinalDamage(ctx);

            _rangeValue.CurrentValue -= ctx.Damage;
            ValueChanged?.Invoke();

            Hit?.Invoke(ctx);

            if (_rangeValue.CurrentValue <= _rangeValue.MinValue)
            {
                InvokeDeath(ctx.Attacker);
            }
        }

        private float CalculateFinalDamage(HitContext ctx)
        {
            var finalDamage = ctx.Damage;

            var percentArmor =
                ArmorSources.Sum(armor => armor.PercentArmor); // + //_modifiable.ModifierReceiver.ArmorModifier;

            finalDamage -= percentArmor; // Apply percent armor

            if (_modifiable != null)
            {
                var flatArmor = ArmorSources.Sum(armor => armor.FlatArmor) + _modifiable.ModifierReceiver.ArmorFlatModifier;
                finalDamage -= flatArmor; // Apply flat armor
                
                var percentageArmor = ArmorSources.Sum(armor => armor.PercentArmor) + _modifiable.ModifierReceiver.ArmorPercentageModifier;
                finalDamage *= (1f - percentageArmor); // Apply percentage armor
            }

            return Mathf.Max(finalDamage, MinDamagePerHit); // Clamp to min damage
        }

        public void Heal(HealContext ctx)
        {
            if (!gameObject.activeInHierarchy)
            {
                GameLogger.LogError("Trying to heal a disabled object");
                return;
            }

            _rangeValue.CurrentValue += ctx.HealAmount;
            ValueChanged?.Invoke();

            Healed?.Invoke(ctx);
        }
        
        private void InvokeDeath(Creature lastAttackedBy)
        {
            var deathContext = new DeathContext()
            {
                Killer = lastAttackedBy,
                KilledEntity = GetComponent<Entity>() // Optional: Associate with owning entity
            };

            Death?.Invoke(deathContext);
            Destroy(gameObject); // Optional: Destroy the GameObject
        }


        public void SetMaxHealth(float value)
        {
            _rangeValue.MaxValue = value;
            _rangeValue.CurrentValue = value;
            _rangeValue.BaseValue = value;
        }

        public void Kill()
        {
            _rangeValue.CurrentValue = _rangeValue.MinValue;
            ValueChanged?.Invoke();
            InvokeDeath(null);
        }
    }
}