using System;
using Data;
using JetBrains.Annotations;
using Managers;
using Steam;
using Zenject;
using Object = UnityEngine.Object;

namespace LevelSelector.GameEvents
{
    public class GameEventActions
    {
        [NotNull]
        private static DiContainer GetDi()
        {
            var ctx = Object.FindObjectOfType<SceneContext>();

            if (ctx == null)
            {
                GameLogger.LogError("GameEventActions: No SceneContext found in the scene.");
                throw new InvalidOperationException("No SceneContext found in the scene.");
            }

            return ctx.Container;
        }

        public static OptionEffect AddJuice(decimal juice)
        {
            void Effect()
            {
                var di = GetDi();
                var crewManager = di.Resolve<ICrewManager>();
                crewManager.Resources.AddJuice(juice);
            }

            if (!GameSettings.Instance.Preferences.UseJuiceMechanic)
            {
                return AddMoney(juice * 2);
            }

            return new OptionEffect
            {
                Value = juice,
                Effect = Effect,
                PositiveLabelKey = "GameEvents.Effects.AddJuice",
                NegativeLabelKey = "GameEvents.Effects.LoseJuice"
            };
        }

        public static OptionEffect AddMoney(decimal money)
        {
            void Effect()
            {
                var di = GetDi();
                var crewManager = di.Resolve<ICrewManager>();
                crewManager.Resources.AddMoney(money);
            }

            return new OptionEffect
            {
                Value = money,
                Effect = Effect,
                PositiveLabelKey = "GameEvents.Effects.AddMoney",
                NegativeLabelKey = "GameEvents.Effects.LoseMoney"
            };
        }

        public static OptionEffect CustomAction(
            Action<DiContainer> action,
            Action<OptionEffect> configure = null
        )
        {
            var di = GetDi();
            
            var option = new OptionEffect
            {
                Effect = () => action(di),
                Hidden = true
            };
            
            configure?.Invoke(option);
            
            return option;
        }

        public static OptionEffect AddFuel(decimal fuel)
        {
            void Effect()
            {
                var di = GetDi();
                var crewManager = di.Resolve<ICrewManager>();
                crewManager.Resources.AddFuel(fuel);
            }

            return new OptionEffect
            {
                Value = fuel,
                Effect = Effect,
                PositiveLabelKey = "GameEvents.Effects.AddFuel",
                NegativeLabelKey = "GameEvents.Effects.LoseFuel"
            };
        }

        public static OptionEffect TriggerEvent(string eventName)
        {
            void Effect()
            {
                var di = GetDi();
                var eventManager = di.Resolve<IGameEventManager>();
                var gameEvent = GameEventRegistry.Get(eventName);
                eventManager.TriggerEvent(gameEvent);
            }

            return new OptionEffect
            {
                Effect = Effect,
            };
        }


        public static OptionEffect Lose => new()
        {
            PositiveLabelKey = "GameEvents.Effects.Lose",
            Effect = () =>
            {
                var di = GetDi();
                var dataManager = di.Resolve<IDataManager>();
                var sceneLoader = di.Resolve<ISceneLoader>();
                dataManager.DeleteData();
                sceneLoader.LoadScene(Scenes.MainMenu);
            }
        };

        public static OptionEffect AddExperienceToAllCrew(int amount)
        {
            void Effect()
            {
                var di = GetDi();
                var crewManager = di.Resolve<ICrewManager>();

                foreach (var crewMember in crewManager.Crew)
                {
                    crewManager.AddXp(crewMember, amount);
                }
            }

            return new OptionEffect
            {
                Value = amount,
                Effect = Effect,
                PositiveLabelKey = "GameEvents.Effects.AddExperienceToAllCrew",
                NegativeLabelKey = "GameEvents.Effects.LoseExperienceFromAllCrew"
            };
        }

        public static OptionEffect UnlockAchievement(string achievementMiscCheatGambler)
        {
            void Effect()
            {
                var di = GetDi();
                var achievementManager = di.Resolve<IAchievementsManager>();
                achievementManager.UnlockAchievement(achievementMiscCheatGambler);
            }

            return new OptionEffect
            {
                Effect = Effect,
                Hidden = true
            };
        }
    }
}