using System.Collections.Generic;
using Components.Creatures;
using UnityEngine.Serialization;

public interface IModifiable
{
    public ModifierReceiver ModifierReceiver { get; }
}

public enum ModifierType
{
    Speed,
    Damage,
    AttackSpeed,
    AccuracyFlat,
    ArmorFlat,
    ArmorPercentage
}

public class Modifier
{
    public float? SpeedModifier { get; set; }
    public float? DamageModifier { get; set; }
    public float? AttackSpeedModifier { get; set; }
    public float? AccuracyFlatModifier { get; set; }
    public float? ArmorFlatModifier { get; set; }
    public float? ArmorPercentageModifier { get; set; }
    
    public Dictionary<ModifierType, float> ToDictionary()
    {
        var dict = new Dictionary<ModifierType, float>();
        
        if (SpeedModifier.HasValue) dict[ModifierType.Speed] = SpeedModifier.Value;
        if (DamageModifier.HasValue) dict[ModifierType.Damage] = DamageModifier.Value;
        if (AttackSpeedModifier.HasValue) dict[ModifierType.AttackSpeed] = AttackSpeedModifier.Value;
        if (AccuracyFlatModifier.HasValue) dict[ModifierType.AccuracyFlat] = AccuracyFlatModifier.Value;
        if (ArmorFlatModifier.HasValue) dict[ModifierType.ArmorFlat] = ArmorFlatModifier.Value;
        if (ArmorPercentageModifier.HasValue) dict[ModifierType.ArmorPercentage] = ArmorPercentageModifier.Value;

        return dict;
    }

    public static StatsType GetModifierStatType(ModifierType key)
    {
        return key switch
        {
            ModifierType.Speed => StatsType.Percentage,
            ModifierType.Damage => StatsType.Percentage,
            ModifierType.AttackSpeed => StatsType.Percentage,
            ModifierType.AccuracyFlat => StatsType.Percentage,
            ModifierType.ArmorFlat => StatsType.Flat,
            ModifierType.ArmorPercentage => StatsType.Percentage,
            _ => throw new System.ArgumentOutOfRangeException(nameof(key), key, "Unknown ModifierType")
        };
    }
}

[System.Serializable]
public class ModifierTemplate
{
    public float speedModifier;
    public float damageModifier;
    public float attackSpeedModifier;
    [FormerlySerializedAs("accuracyModifier")] public float accuracyFlatModifier;
    public float armorFlatModifier;
    public float armorPercentageModifier;
    
    public Modifier ToModifier()
    {
        return new Modifier
        {
            SpeedModifier = speedModifier != 0 ? speedModifier : null,
            DamageModifier = damageModifier != 0 ? damageModifier : null,
            AttackSpeedModifier = attackSpeedModifier != 0 ? attackSpeedModifier : null,
            AccuracyFlatModifier = accuracyFlatModifier != 0 ? accuracyFlatModifier : null,
            ArmorFlatModifier = armorFlatModifier != 0 ? armorFlatModifier : null,
            ArmorPercentageModifier = armorPercentageModifier != 0 ? armorPercentageModifier : null
        };
    }
}

public class ModifierReceiver
{
    private List<Modifier> _modifiers = new List<Modifier>();

    public float SpeedModifier { get; private set; }
    public float DamageModifier { get; private set; }
    public float AttackSpeedModifier { get; private set; }
    public float AccuracyFlatModifier { get; private set; }
    public float ArmorFlatModifier { get; private set; }
    public float ArmorPercentageModifier { get; private set; }

    public ModifierReceiver()
    {
        UpdateModifierValues();
    }
    
    public void AddModifier(Modifier modifier)
    {
        if (modifier == null)
        {
            throw new System.ArgumentNullException(nameof(modifier));
        }

        _modifiers.Add(modifier);   
        
        UpdateModifierValues();
    }
    
    public void RemoveModifier(Modifier modifier)
    {
        if (modifier == null)
        {
            throw new System.ArgumentNullException(nameof(modifier));
        }

        _modifiers.Remove(modifier);
        
        UpdateModifierValues();
    }

    private void UpdateModifierValues()
    {
        SpeedModifier = 1;
        DamageModifier = 1;
        AttackSpeedModifier = 1;
        AccuracyFlatModifier = 1;
        ArmorFlatModifier = 0;
        ArmorPercentageModifier = 0; // yeee this is weird that percentage armor starts at 0, but it does 
        
        foreach (var modifier in _modifiers)
        {
            if (modifier.SpeedModifier.HasValue) SpeedModifier += modifier.SpeedModifier.Value;
            if (modifier.DamageModifier.HasValue) DamageModifier += modifier.DamageModifier.Value;
            if (modifier.AttackSpeedModifier.HasValue) AttackSpeedModifier += modifier.AttackSpeedModifier.Value;
            if (modifier.AccuracyFlatModifier.HasValue) AccuracyFlatModifier += modifier.AccuracyFlatModifier.Value;
            if (modifier.ArmorFlatModifier.HasValue) ArmorFlatModifier += modifier.ArmorFlatModifier.Value;
            if (modifier.ArmorPercentageModifier.HasValue) ArmorFlatModifier += modifier.ArmorPercentageModifier.Value;
        }
    }

    public bool HasModifier(Modifier modifier)
    {
        return _modifiers.Contains(modifier);
    }
}