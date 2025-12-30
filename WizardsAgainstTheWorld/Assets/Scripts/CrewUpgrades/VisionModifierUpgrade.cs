using Managers;
using UnityEngine;
using Zenject;

namespace CrewUpgrades
{
    public class VisionModifierUpgrade : CrewUpgrade
    {
        [SerializeField] private float visionBoost = 0.25f;

        public override void InitializeGameScene(DiContainer diContainer)
        {
            base.InitializeGameScene(diContainer);

            var visionManager = diContainer.Resolve<IVisionManager>();

            visionManager.VisionRangeMultiplier *= (1 + visionBoost);
        }
    }
}