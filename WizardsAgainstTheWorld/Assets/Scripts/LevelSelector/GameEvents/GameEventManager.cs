using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using LevelSelector.GameEvents;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
using UnityEngine;
using Zenject;

public class GameEvent
{
    public string Id { get; set; }
    public string TitleKey { get; set; }
    public string DescriptionKey { get; set; }
    public string Image { get; set; }
    public ICollection<GameEventOption> Options { get; set; }
}

public enum GameEventOptionType
{
    Normal,
    Special,
}

public class GameEventOption
{
    public string LabelKey { get; set; }
    public ICollection<OptionEffect> Effects { get; set; }
    public ICollection<OptionEffectGroup> EffectGroups { get; set; }
    public Action Invoke { get; set; }
    public Func<bool> CheckEnabled { get; set; }
    public Func<bool> CheckVisible { get; set; }
    public GameEventOptionType Type { get; set; }
}

public interface IGameEventManager
{
    event Action<GameEvent> EventTriggered;
    void TriggerEvent(GameEvent gameEvent);
}

public class GameEventManager : MonoBehaviour, IGameEventManager
{
    public event Action<GameEvent> EventTriggered;

    [Inject] private ICrewManager _crewManager;
    [Inject] private IRegionManager _regionManager;
    [Inject] private IGameResultConsumer _gameResultConsumer;
    [Inject] private IDataManager _dataManager;
    
    // TODO: this is SHIT, we need some type of flag system
    private static bool _shownTraderEncounter = false;
    private bool _shownDesperateFuelPurchase = false;

    private void Start()
    {
        _crewManager.ChangedLocation += OnLocationChanged;
        _crewManager.Changed += OnCrewChanged;

        _gameResultConsumer.RegisterHandler(OnGameResult);

        if (_crewManager.Crew.Count == 0)
        {
            TriggerEvent(SystemEvents.AllDeadEvent);
        }
    }

    private void OnGameResult(GameResult result)
    {
        if (result.VictoryConditionsResults.Any(x => !x.IsAchieved))
        {
            TriggerEvent(SystemEvents.FederationTakeoverEvent);
        }
    }

    private void OnCrewChanged()
    {
        var resources = _crewManager.Resources;
        var location = _regionManager.Region.GetLocation(_crewManager.CurrentLocationId);
        bool noShop = location.ShopData == null;

        if (resources.Fuel > 0 || !noShop || _shownDesperateFuelPurchase) return;

        if (resources.Money >= 50 && resources.Juice >= 50)
        {
            TriggerEvent(SystemEvents.DesperateBuyFuelMoneyOrJuiceEvent);
            _shownDesperateFuelPurchase = true;
        }
        else if (resources.Money >= 50)
        {
            TriggerEvent(SystemEvents.DesperateBuyFuelForMoneyEvent);
            _shownDesperateFuelPurchase = true;
        }
        else if (resources.Juice >= 50)
        {
            TriggerEvent(SystemEvents.DesperateBuyFuelForJuiceEvent);
            _shownDesperateFuelPurchase = true;
        }
        else
        {
            TriggerEvent(SystemEvents.OutOfFuelEvent);
            _shownDesperateFuelPurchase = true;
        }
    }

    private void OnLocationChanged()
    {
        _shownDesperateFuelPurchase = false; // we resent this flag
        
        var location = _regionManager.Region.GetLocation(_crewManager.CurrentLocationId);
        if (location.Type == LocationType.BossNode && !location.Salvaged && !location.Visited)
        {
            TriggerEvent(SystemEvents.BossEncounterEvent);
        }

        if (location.ShopData is not null && !_shownTraderEncounter)
        {
            _shownTraderEncounter = true;
            TriggerEvent(SystemEvents.TraderEncounterEvent);
        }
        
        if(location.EventId != null && location.Visited == false)
        {
            var gameEvent = GameEventRegistry.Get(location.EventId);
            if (gameEvent != null)
            {
                TriggerEvent(gameEvent);
            }
            else
            {
                GameLogger.LogError($"GameEvent with ID '{location.EventId}' not found in registry.");
            }
        }
    }

    public void TriggerEvent(GameEvent gameEvent)
    {
        EventTriggered?.Invoke(gameEvent);
    }
}