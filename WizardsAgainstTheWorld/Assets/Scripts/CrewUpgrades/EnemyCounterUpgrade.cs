using System;
using System.Collections;
using System.Linq;
using Managers;
using TMPro;
using UnityEngine;
using Utilities;
using Zenject;

namespace CrewUpgrades
{
    public class EnemyCounterUpgrade : CrewUpgrade
    {
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject enableInGame;
        [SerializeField] private string counterTextKey = "Game.UI.EnemyCounterUpgrade.CounterText";

        public override void InitializeGameScene(DiContainer diContainer)
        {
            base.InitializeGameScene(diContainer);

            var creatureManager = diContainer.Resolve<ICreatureManager>();

            StartCoroutine(CountCreatures(creatureManager));
            
            enableInGame.SetActive(true);
        }

        public override void InitializeLevelSelectorScene(DiContainer diContainer)
        {
            base.InitializeLevelSelectorScene(diContainer);
            
            enableInGame.SetActive(false);
        }

        private IEnumerator CountCreatures(ICreatureManager creatureManager)
        {
            while (true)
            {
                int count = creatureManager.GetAliveCreatures().Count();

                if (counterText is not null)
                {
                    counterText.text = counterTextKey.Localize(count);
                }
                else
                {
                    GameLogger.LogWarning("Counter text is not assigned.");
                }

                yield return new WaitForSeconds(1);
            }
        }
    }
}