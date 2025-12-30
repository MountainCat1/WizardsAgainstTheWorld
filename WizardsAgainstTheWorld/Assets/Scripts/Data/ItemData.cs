using System;
using System.Collections.Generic;
using System.Linq;
using Items;

public enum ItemType
{
    Default,
    Weapon,
    Consumable,
    Armor,
    Quest,
    Junk
}

[Serializable]
public class ItemData
{
    public string Identifier;
    public int Count = 1;
    public bool Stackable;
    public ItemType Type;
    public bool Equipped;
    public bool Reloadable;

    // depracated
    [Obsolete("Only for serialization purposes, do not use")]
    public ItemData()
    {
        // This is for serialization, IT SHOULD NOT BE USED OTHWERWISE
    }
    
    public static ItemData FromItem(ItemBehaviour item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "ItemBehaviour cannot be null when creating ItemData");
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return new ItemData
        {
            Identifier = item.GetIdentifier(),
            Count = item.Original == null ? 1 : item.Count, // if we do it on a prefab, lets assume its 1
            Stackable = item.Stackable,
            Type = item is Weapon ? ItemType.Weapon : ItemType.Default,
            Prefab = item.Original ?? item,
            Modifiers = item.IsOriginal
                ? new List<ItemWeaponModifier>()
                : item.GetData()?.Modifiers ?? new List<ItemWeaponModifier>(),
            Reloadable = (item as Weapon)?.ReloadComponent != null,
        };
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    public static ItemData FromInstantiatedItem(ItemBehaviour item)
    {
        if(item.IsOriginal)
            throw new NotImplementedException($"Item {item} is original");

        return item.GetData();
    }
    
    public List<ItemWeaponModifier> Modifiers = new();
    
    [NonSerialized] public ItemBehaviour Prefab;
    
    public float GetApplied(WeaponPropertyModifiers property, float baseValue)
    {
        return baseValue + GetChange(property, baseValue);
    }

    public float GetChange(WeaponPropertyModifiers property, float baseValue)
    {
        var percentageModifier = Modifiers.OfType<WeaponValueModifier>()
            .Where(m => m.Type == property)
            .Sum(x => x.Value);

        if (IsPropertyInverted(property))
        {
            percentageModifier = -percentageModifier;
        }
        
        if (percentageModifier <= -1)
        {
            return -baseValue;
        }
            
        return baseValue * percentageModifier;
    }

    public bool IsPropertyInverted(WeaponPropertyModifiers property)
    {
        if (property == WeaponPropertyModifiers.ReloadTime)
        {
            return true;
        }

        return false;
    }
}

[Serializable]
public class ItemWeaponModifier
{
}

public enum WeaponPropertyModifiers
{
    Damage,
    AttackSpeed,
    Range,
    Accuracy,
    ReloadTime,
    AmmoCapacity
}

[Serializable]
public class WeaponValueModifier : ItemWeaponModifier
{
    public WeaponPropertyModifiers Type;
    public float Value;
}

[Serializable]
public class WeaponSpecialModifier : ItemWeaponModifier
{
    public string Identifier { get; set; }
}