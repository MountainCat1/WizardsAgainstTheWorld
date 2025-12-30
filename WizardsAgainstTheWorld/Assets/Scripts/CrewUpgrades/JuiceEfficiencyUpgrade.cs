using Managers;
using UnityEngine;
using Zenject;

namespace CrewUpgrades
{
    public class JuiceEfficiencyUpgrade : CrewUpgrade
    {
        [SerializeField] private float juiceUsageDecreaseFactor = 0.33f;

        public override void InitializeGameScene(DiContainer diContainer)
        {
            base.InitializeGameScene(diContainer);

            var juiceManager = diContainer.Resolve<IJuiceManager>();

            juiceManager.JuiceUseProcessor.Register(OnJuiceUse);
        }

        private void OnJuiceUse(JuiceUsePreContext ctx)
        {
            ctx.JuiceUsed *= (1 - (decimal)juiceUsageDecreaseFactor);
        }
    }
}