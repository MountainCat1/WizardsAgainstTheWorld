using System;

namespace Items.PassiveItems
{
    public interface IWeaponable
    {
        public Weapon Weapon { get; }
        event Action WeaponChanged;
    }
}