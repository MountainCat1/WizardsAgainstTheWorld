using System;
using UnityEngine;

[System.Serializable]
public class RangedValue : IReadonlyRangedValue
{
    // Events
    public event Action ValueChanged;

    // Serialized Private Variables
    [SerializeField] private float maxValue;

    // Private Variables
    private float? _baseValue = null;
    private float _currentValue;
    private float _minValue = 0;

    // Properties
    public float BaseValue
    {
        get { return _baseValue ?? maxValue; }
        set
        {
            _baseValue = value;
            UpdateCurrentValue();
        }
    }

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            UpdateCurrentValue();
            ValueChanged?.Invoke();
        }
    }

    public float MinValue
    {
        get { return _minValue; }
        set
        {
            _minValue = value;
            UpdateCurrentValue();
        }
    }

    public float MaxValue
    {
        get { return maxValue; }
        set
        {
            maxValue = value;
            UpdateCurrentValue();
        }
    }

    // Public Methods
    public RangedValue(float baseValue, float minValue, float maxValue)
    {
        this._minValue = minValue;
        this.maxValue = maxValue;
        this._currentValue = baseValue;
        UpdateCurrentValue();
    }

    // Private Methods
    private void UpdateCurrentValue()
    {
        _currentValue = Mathf.Clamp(_currentValue, _minValue, maxValue);
    }
}