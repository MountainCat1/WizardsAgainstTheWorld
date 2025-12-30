using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ModifiableFloat
{
    // Events
    public event Action Changed;

    // Properties
    public float CurrentValue => GetCurrentSpeed();

    [SerializeField] private float baseValue = 0f;

    public float BaseValue
    {
        get => baseValue;
        set
        {
            if (Math.Abs(baseValue - value) > 0.01f)
            {
                baseValue = value;
                CalculateSpeed();
            }
        }
    }

    private Dictionary<Object, float> _modifiers = new();
    private float _currentValue;
    private bool _initialized;

    // Methods
    private float GetCurrentSpeed()
    {
        if (!_initialized)
        {
            CalculateSpeed();
            _initialized = true;
        }

        return _currentValue;
    }

    private void CalculateSpeed()
    {
        _currentValue = baseValue + _modifiers.Values.Sum();
        Changed?.Invoke();
    }

    public void AddModifier(Object key, float value)
    {
        _modifiers[key] = value;
        CalculateSpeed();
    }

    public void RemoveModifier(Object key)
    {
        if (_modifiers.Remove(key))
        {
            CalculateSpeed();
        }
    }
}