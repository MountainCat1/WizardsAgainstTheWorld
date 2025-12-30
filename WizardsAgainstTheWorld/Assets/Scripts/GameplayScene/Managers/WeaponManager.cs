using System;
using System.Collections.Generic;using UnityEngine;

public interface IWeaponManager
{
    event Action<Weapon, AttackContext, Entity> WeaponAttackMissed;

    void RegisterWeapon(Weapon weapon);
    void UnregisterWeapon(Weapon weapon);
}

public class WeaponManager : MonoBehaviour, IWeaponManager
{
    public event Action<Weapon, AttackContext, Entity> WeaponAttackMissed;

    private readonly HashSet<Weapon> _registeredWeapons = new();
    private readonly Dictionary<Weapon, Action<AttackContext, Entity>> _weaponHandlers = new();

    public void RegisterWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (_registeredWeapons.Contains(weapon)) return;

        _registeredWeapons.Add(weapon);

        // Create and store a handler that captures the weapon reference
        Action<AttackContext, Entity> handler = (context, entity) => HandleWeaponMissed(weapon, context, entity);
        _weaponHandlers[weapon] = handler;

        weapon.Missed += handler;
    }

    public void UnregisterWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (!_registeredWeapons.Contains(weapon)) return;

        _registeredWeapons.Remove(weapon);

        if (_weaponHandlers.TryGetValue(weapon, out var handler))
        {
            weapon.Missed -= handler;
            _weaponHandlers.Remove(weapon);
        }
    }

    private void HandleWeaponMissed(Weapon weapon, AttackContext attackContext, Entity missedEntity)
    {
        WeaponAttackMissed?.Invoke(weapon, attackContext, missedEntity);
    }
}