using System;
using System.Collections.Generic;
using System.Linq;
using LevelSelector.GameEvents;
using Random = UnityEngine.Random;

public class OptionEffectGroup
{
    public float Chance { get; set; } = 1f;
    public List<OptionEffect> Effects { get; } = new();

    public OptionEffectGroup AddAction(OptionEffect effect)
    {
        if (effect != null)
            Effects.Add(effect);

        return this;
    }

    public void Invoke()
    {
        foreach (var effect in Effects)
        {
            effect.Effect?.Invoke();
        }
    }

    public OptionEffectGroup AddToolTip(string tooltipKey)
    {
        Effects.Add(new OptionEffect()
        {
            ToolTipKey = $"GameEvents.{tooltipKey}",
        });

        return this;
    }
}


public class OptionEffect
{
    public object Value { get; set; }
    public Action Effect { get; set; }
    public bool Hidden { get; set; } = false;

    public string PositiveLabelKey
    {
        get => _positiveLabelKey ?? ToolTipKey;
        set => _positiveLabelKey = value;
    }

    public string ToolTipKey { get; set; }

    public string NegativeLabelKey
    {
        get => _negativeLabelKey ?? PositiveLabelKey ?? ToolTipKey;
        set => _negativeLabelKey = value;
    }


    private string _negativeLabelKey;
    private string _positiveLabelKey;
}

public class GameEventOptionBuilder
{
    private readonly List<OptionEffect> _effects = new();
    private readonly List<OptionEffectGroup> _effectGroups = new();
    private readonly List<GameEventCondition> _conditions = new();
    private readonly List<GameEventCondition> _isVisibleConditions = new();
    private readonly string _name;
    private GameEventOptionType _type = GameEventOptionType.Normal;

    public GameEventOptionBuilder(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name), "Option name cannot be null.");
    }

    public GameEventOptionBuilder AddAction(OptionEffect effect)
    {
        if (effect != null)
            _effects.Add(effect);
        return this;
    }

    public GameEventOptionBuilder AddTooltipText(string tooltipKey)
    {
        _effects.Add(new OptionEffect()
        {
            ToolTipKey = $"GameEvents.{tooltipKey}",
        });
        return this;
    }

    public GameEventOptionBuilder AddActionGroup(Action<OptionEffectGroup> configure)
    {
        var group = new OptionEffectGroup();
        configure(group);
        _effectGroups.Add(group);
        return this;
    }

    public GameEventOptionBuilder AddCondition(GameEventCondition condition)
    {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition), "Condition cannot be null.");

        _conditions.Add(condition);
        return this;
    }
    
    public GameEventOptionBuilder AddIsVisibleCondition(GameEventCondition condition)
    {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition), "Condition cannot be null.");

        _isVisibleConditions.Add(condition);
        return this;
    }


    public void SetType(GameEventOptionType optionType)
    {
        _type = optionType;
    }

    public GameEventOption Build()
    {
        return new GameEventOption
        {
            LabelKey = $"GameEvents.{_name}",
            Effects = _effects,
            EffectGroups = _effectGroups,
            Invoke = InvokeAll,
            CheckEnabled = CheckAllConditions,
            CheckVisible = CheckAllIsVisibleConditions,
            Type = _type
        };
    }

    #region Invoker Methods

    private bool CheckAllConditions()
    {
        if (_conditions.Count == 0)
            return true;

        foreach (var condition in _conditions)
        {
            if (!condition.Check())
                return false;
        }

        return true;
    }
    
    private bool CheckAllIsVisibleConditions()
    {
        if (_isVisibleConditions.Count == 0)
            return true;

        foreach (var condition in _isVisibleConditions)
        {
            if (!condition.Check())
                return false;
        }

        return true;
    }

    private void InvokeAll()
    {
        // First invoke all standalone effects
        foreach (var effect in _effects)
        {
            effect?.Effect?.Invoke();
        }

        if (_effectGroups.Count == 0)
            return;

        float totalWeight = _effectGroups.Sum(g => g.Chance); 
        float roll = Random.Range(0f, totalWeight);
        float accum = 0f;

        foreach (var group in _effectGroups)
        {
            accum += group.Chance;
            if (roll <= accum)
            {
                group.Invoke();
                break;
            }
        }
    }

    #endregion
}