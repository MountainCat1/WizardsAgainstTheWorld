using LevelSelector.Managers;
using Managers;
using UnityEngine;
using Zenject;

namespace CrewUpgrades
{
    public class UpgraderUpgrade : CrewUpgrade
    {
        public override void InitializeLevelSelectorScene(DiContainer diContainer)
        {
            base.InitializeLevelSelectorScene(diContainer);
            
            var interactionManager = diContainer.Resolve<ILocationInteractionManager>();
            
            interactionManager.CheckVisibleInteractionProcessor.Register(OnCheckVisibleInteraction);
        }

        private void OnCheckVisibleInteraction(CheckVisibleInteractionPreContext ctx)
        {
            if (ctx.Interaction.Type == LocationInteractionType.Upgrade)
            {
                ctx.IsDisplayed = true; 
            }
        }
    }
}