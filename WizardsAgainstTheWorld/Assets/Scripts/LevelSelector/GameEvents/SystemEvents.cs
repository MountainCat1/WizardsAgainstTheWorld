using LevelSelector.GameEvents;

[GameEventCategory(GameEventCategory.System)]
public static class SystemEvents
{
    public static GameEvent FederationTakeoverEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.FederationTakeover.Title")
            .WithDescription("SystemEvents.FederationTakeover.Description")
            .WithOption("SystemEvents.FederationTakeover.Option1", GameEventActions.Lose)
            .Build();
    
    public static GameEvent AllDeadEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.AllDead.Title")
            .WithDescription("SystemEvents.AllDead.Description")
            .WithOption("SystemEvents.AllDead.Option1", GameEventActions.Lose)
            .Build();
    
    public static GameEvent TraderEncounterEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.TraderEncounter.Title")
            .WithDescription("SystemEvents.TraderEncounter.Description")
            .WithOption("SystemEvents.TraderEncounter.Option1")
            .Build();
    
    public static GameEvent BossEncounterEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.BossEncounter.Title")
            .WithDescription("SystemEvents.BossEncounter.Description")
            .WithOption("SystemEvents.BossEncounter.Option1")
            .Build();
    
    #region Fuel

    public static GameEvent OutOfFuelEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.OutOfFuel.Title")
            .WithDescription("SystemEvents.OutOfFuel.Description")
            .WithOption("SystemEvents.OutOfFuel.Option1", GameEventActions.Lose)
            .Build();

    public static GameEvent DesperateBuyFuelForMoneyEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.DesperateBuyFuelForMoney.Title")
            .WithDescription("SystemEvents.DesperateBuyFuelForMoney.Description")
            .WithOption("SystemEvents.DesperateBuyFuelForMoney.Option1", option =>
            {
                option.AddAction(GameEventActions.AddFuel(2));
                option.AddAction(GameEventActions.AddMoney(-50));
            })
            .Build();

    public static GameEvent DesperateBuyFuelForJuiceEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.DesperateBuyFuelForJuice.Title")
            .WithDescription("SystemEvents.DesperateBuyFuelForJuice.Description")
            .WithOption("SystemEvents.DesperateBuyFuelForJuice.Option1", option =>
            {
                option.AddAction(GameEventActions.AddFuel(2));
                option.AddAction(GameEventActions.AddJuice(-50));
            })
            .Build();

    public static GameEvent DesperateBuyFuelMoneyOrJuiceEvent =
        GameEventBuilder.Create()
            .WithTitle("SystemEvents.DesperateBuyFuelMoneyOrJuice.Title")
            .WithDescription("SystemEvents.DesperateBuyFuelMoneyOrJuice.Description")
            .WithOption("SystemEvents.DesperateBuyFuelMoneyOrJuice.Option1", option =>
            {
                option.AddAction(GameEventActions.AddFuel(2));
                option.AddAction(GameEventActions.AddMoney(-50));
            })
            .WithOption("SystemEvents.DesperateBuyFuelMoneyOrJuice.Option2", option =>
            {
                option.AddAction(GameEventActions.AddFuel(2));
                option.AddAction(GameEventActions.AddJuice(-50));
            })
            .Build();

    #endregion
}