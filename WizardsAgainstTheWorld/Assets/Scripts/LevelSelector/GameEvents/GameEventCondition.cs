using System;
using System.Linq;
using JetBrains.Annotations;
using Managers;
using Zenject;
using Object = UnityEngine.Object;

namespace LevelSelector.GameEvents
{
    public class GameEventCondition
    {
        public Func<bool> Check { get; set; }
    }
    
    public static class GameEventConditions
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
        
        public static GameEventCondition HasJuice(int amount)
        {
            return new GameEventCondition
            {
                Check = () =>
                {
                    var di = GetDi();
                    var crewManager = di.Resolve<ICrewManager>();
                    return crewManager.Resources.Juice >= amount;
                }
            };
        }
        
        public static GameEventCondition HasMoney(int amount)
        {
            return new GameEventCondition
            {
                Check = () =>
                {
                    var di = GetDi();
                    var crewManager = di.Resolve<ICrewManager>();
                    return crewManager.Resources.Money >= amount;
                }
            };
        }
        
        public static GameEventCondition HasFuel(int amount)
        {
            return new GameEventCondition
            {
                Check = () =>
                {
                    var di = GetDi();
                    var crewManager = di.Resolve<ICrewManager>();
                    return crewManager.Resources.Fuel >= amount;
                }
            };
        }

        public static GameEventCondition HasUpgrade(string crewUpgradeId)
        {
            return new GameEventCondition
            {
                Check = () =>
                {
                    var di = GetDi();
                    var crewManager = di.Resolve<ICrewManager>();
                    return crewManager.Upgrades.Any(u => u.Id == crewUpgradeId);
                }
            };
        }

        public static GameEventCondition HasItem(string itemId)
        {
            return new GameEventCondition
            {
                Check = () =>
                {
                    var di = GetDi();
                    var crewManager = di.Resolve<ICrewManager>();
                    return crewManager.Inventory.Items.Any(u => u.Identifier == itemId)
                           || crewManager.Crew.Any(x 
                               => x.Inventory.Items.Any(x => x.Identifier == itemId));
                }
            };
        }
    }
}