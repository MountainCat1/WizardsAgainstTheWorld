using Managers;
using UnityEngine;
using Zenject;

namespace CrewUpgrades
{
    public class FuelEfficiencyUpgrade : CrewUpgrade
    {
        [SerializeField] private float fuelNotUsedChance = 0.33f;

        public override void InitializeLevelSelectorScene(DiContainer diContainer)
        {
            base.InitializeLevelSelectorScene(diContainer);

            var crewManager = diContainer.Resolve<ICrewManager>();
            crewManager.PreResourceUsage.Register(OnPreResourceUsage);
        }

        private void OnPreResourceUsage(ResourceUsagePreContext ctx)
        {
            var roll = Random.value;

            if (ctx.Resource == InGameResource.Fuel && roll < fuelNotUsedChance)
            {
                // Prevent fuel usage
                ctx.Amount = 0;
                ctx.Cancel = true;
                GameLogger.Log(
                    $"Fuel efficiency upgrade activated, no fuel used for this travel. " +
                    $"Chance: {fuelNotUsedChance * 100:F0}% rolled {roll * 100:F0}%");
            }
            else
            {
                GameLogger.Log(
                    $"Fuel efficiency upgrade not activated, {ctx.Amount} fuel used for this travel. " +
                    $"Chance: {fuelNotUsedChance * 100:F0}% rolled {roll * 100:F0}%");
            }
        }
    }
}