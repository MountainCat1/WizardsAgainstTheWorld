using System;
using System.Collections.Generic;
using UnityEngine;

public enum TimeScaleModifier
{
    Pause = 100,          // Highest priority
    JuiceOverdrive = 10    // Lower priority
}

public interface ITimeManager
{
    void AddTimeScaleChange(TimeScaleModifier type, float timeScale);
    void RemoveTimeScaleChange(TimeScaleModifier type);
}

public class TimeManager : MonoBehaviour, ITimeManager
{
    private readonly Dictionary<TimeScaleModifier, float> _activeModifiers = new();

    private void Awake()
    {
        ApplyTimeScale();
    }

    public void AddTimeScaleChange(TimeScaleModifier type, float timeScale)
    {
        _activeModifiers[type] = timeScale;
        ApplyTimeScale();
    }

    public void RemoveTimeScaleChange(TimeScaleModifier type)
    {
        if (_activeModifiers.Remove(type))
        {
            ApplyTimeScale();
        }
    }

    private void ApplyTimeScale()
    {
        if (_activeModifiers.Count == 0)
        {
            Time.timeScale = 1f; // Default
            return;
        }

        // Pick the modifier with the highest enum value (priority)
        TimeScaleModifier highestPriority = TimeScaleModifier.JuiceOverdrive;
        foreach (var modifier in _activeModifiers.Keys)
        {
            if ((int)modifier > (int)highestPriority)
                highestPriority = modifier;
        }

        Time.timeScale = _activeModifiers[highestPriority];
    }

    private void OnDestroy()
    {
        // Reset time scale when the manager is destroyed
        Time.timeScale = 1f;
    }
}