using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public class GameEventBuilder
{
    private readonly GameEvent _gameEvent;

    private GameEventBuilder(string id)
    {
        _gameEvent = new GameEvent
        {
            Id = id,
            Options = new List<GameEventOption>()
        };
    }

    public static GameEventBuilder Create([CallerMemberName] string callerName = "")
    {
        return new GameEventBuilder(callerName);
    }

    public GameEventBuilder WithTitle(string title)
    {
        _gameEvent.TitleKey = $"GameEvents.{title}";
        return this;
    }

    public GameEventBuilder WithDescription(string description)
    {
        _gameEvent.DescriptionKey = $"GameEvents.{description}";
        return this;
    }

    public GameEventBuilder WithImage(string imagePath)
    {
        _gameEvent.Image = imagePath;
        return this;
    }

    public GameEventBuilder WithOption(string name, Action<GameEventOptionBuilder> configure)
    {
        var optionBuilder = new GameEventOptionBuilder(name);
        configure(optionBuilder);
        _gameEvent.Options.Add(optionBuilder.Build());
        return this;
    }

    public GameEventBuilder WithOption(string name, OptionEffect effect)
    {
        var optionBuilder = new GameEventOptionBuilder(name);
        optionBuilder.AddAction(effect);
        _gameEvent.Options.Add(optionBuilder.Build());
        return this;
    }

    public GameEventBuilder WithOption(string name)
    {
        var optionBuilder = new GameEventOptionBuilder(name);
        _gameEvent.Options.Add(optionBuilder.Build());
        return this;
    }

    public GameEvent Build()
    {
        return _gameEvent;
    }
}