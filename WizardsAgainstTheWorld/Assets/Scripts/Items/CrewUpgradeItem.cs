using System.Collections.Generic;
using System.Linq;
using CrewUpgrades;
using Items.ItemInteractions;
using Managers;
using UnityEngine;
using Zenject;

namespace Items
{
    public class CrewUpgradeItem : ItemBehaviour
    {
        [SerializeField] private CrewUpgrade crewUpgrade;

        public override IEnumerable<ItemInteraction> GetInteractions(DiContainer ctx)
        {
            var crewManager = ctx.Resolve<ICrewManager>();
            var duplicateExists = crewManager.Upgrades.Any(x => x.Id == crewUpgrade.GetIdentifier());

            return new List<ItemInteraction>()
            {
                new("CrewUpgrade", interaction: ApplyUpgrade, enabled: !duplicateExists)
            };
        }

        private void ApplyUpgrade(ItemInteractionContext ctx)
        {
            var crewManager = ctx.Resolve<ICrewManager>();
            if (crewManager == null)
            {
                GameLogger.LogError("CrewManager is not available in the context.");
                return;
            }

            if (crewUpgrade == null)
            {
                GameLogger.LogError("CrewUpgrade is not set on the item.");
                return;
            }

            crewManager.AddUpgrade(crewUpgrade.ToData());
        }
    }
}