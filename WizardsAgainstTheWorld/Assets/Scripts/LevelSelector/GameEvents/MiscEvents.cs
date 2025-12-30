using CrewUpgrades;
using LevelSelector.GameEvents;
using Managers;
using Steam;
using Steam.Steam;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

public static class MiscEvents
{
    public static GameEvent CrateInSpace =
        GameEventBuilder.Create()
            .WithTitle("Misc.CrateInSpace.Title")
            .WithDescription("Misc.CrateInSpace.Description")
            .WithOption("Misc.CrateInSpace.Option1", option =>
            {
                option.AddAction(GameEventActions.AddMoney(35));
                option.AddAction(GameEventActions.AddJuice(15));
            })
            .WithOption("Misc.CrateInSpace.Option2", option =>
            {
                option.AddAction(GameEventActions.AddMoney(45));
                option.AddAction(GameEventActions.AddJuice(20));

                option.AddIsVisibleCondition(GameEventConditions.HasUpgrade("Upgrader"));

                option.SetType(GameEventOptionType.Special);
            })
            .Build();

    public static GameEvent StrangerNeedsFuel =
        GameEventBuilder.Create()
            .WithTitle("Misc.StrangerNeedsFuel.Title")
            .WithDescription("Misc.StrangerNeedsFuel.Description")
            .WithOption("Misc.StrangerNeedsFuel.Option1", option => { })
            .WithOption("Misc.StrangerNeedsFuel.Option2", option =>
            {
                option.AddAction(GameEventActions.AddMoney(40));
                option.AddAction(GameEventActions.AddFuel(-2));

                option.AddCondition(GameEventConditions.HasFuel(3));
            })
            .Build();

    public static GameEvent TrainingFacility =
        GameEventBuilder.Create()
            .WithTitle("Misc.TrainingFacility.Title")
            .WithDescription("Misc.TrainingFacility.Description")
            .WithOption("Misc.TrainingFacility.Option1", option => { })
            .WithOption("Misc.TrainingFacility.Option2", option =>
            {
                option.AddAction(GameEventActions.AddExperienceToAllCrew(600));
                option.AddAction(GameEventActions.AddMoney(-40));

                option.AddCondition(GameEventConditions.HasMoney(40));
            })
            .Build();

    public static GameEvent SpaceGambler =
        GameEventBuilder.Create()
            .WithTitle("Misc.SpaceGambler.Title")
            .WithDescription("Misc.SpaceGambler.Description")
            .WithOption("Misc.SpaceGambler.Option1", option => { })
            .WithOption("Misc.SpaceGambler.Option2", option =>
            {
                option.AddCondition(GameEventConditions.HasMoney(50));

                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.5f;
                    ag.AddAction(GameEventActions.AddMoney(50));
                });
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.5f;
                    ag.AddAction(GameEventActions.CustomAction((di) =>
                    {
                        var crewManager = di.Resolve<ICrewManager>();
                        var achievementManager = di.Resolve<IAchievementsManager>();

                        if (crewManager.Resources.Money <= 60)
                        {
                            achievementManager.UnlockAchievement(Achievements.AchievementMiscLoseAllMoneyToGambler);
                        }
                    }, (cfg) =>
                    {
                        cfg.Hidden = true;
                    }));
                    ag.AddAction(GameEventActions.AddMoney(-50));
                });
                option.AddAction(GameEventActions.TriggerEvent(nameof(SpaceGambler)));
            })
            .WithOption("Misc.SpaceGambler.Option3", option =>
            {
                option.AddIsVisibleCondition(GameEventConditions.HasItem("HackingDevice"));
                option.SetType(GameEventOptionType.Special);
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.8f;
                    ag.AddAction(GameEventActions.AddMoney(100));
                    ag.AddAction(GameEventActions.UnlockAchievement(Achievements.AchievementMiscCheatGambler));
                });
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.2f;
                    ag.AddAction(GameEventActions.AddMoney(-200));
                });
            })
            .Build();

    public static GameEvent DriftingCrate =
        GameEventBuilder.Create()
            .WithTitle("Misc.DriftingCrate.Title")
            .WithDescription("Misc.DriftingCrate.Description")
            .WithOption("Misc.DriftingCrate.Option1", option => { })
            .WithOption("Misc.DriftingCrate.Option2", option =>
            {
                option.AddAction(GameEventActions.AddFuel(-1));
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.5f;
                    ag.AddToolTip("Misc.DriftingCrate.Option2.Tooltip.Lose");
                });
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.5f;
                    ag.AddAction(GameEventActions.AddMoney(50));
                });

                option.AddCondition(GameEventConditions.HasFuel(1));
            })
            .WithOption("Misc.DriftingCrate.Option3", option =>
            {
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.33f;
                    ag.AddToolTip("Misc.DriftingCrate.Option3.Tooltip.Lose");
                });
                option.AddActionGroup(ag =>
                {
                    ag.Chance = 0.66f;
                    ag.AddAction(GameEventActions.AddFuel(-1));
                    ag.AddAction(GameEventActions.AddMoney(50));
                });
                option.AddIsVisibleCondition(GameEventConditions.HasUpgrade("FuelEfficientPumps"));
                option.AddCondition(GameEventConditions.HasFuel(1));
                option.SetType(GameEventOptionType.Special);
            })
            .Build();
}