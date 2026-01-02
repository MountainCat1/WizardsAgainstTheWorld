using System.Collections;
using GameplayScene.Managers;
using UI;
using UnityEngine;
using Zenject;

namespace Components.Entities
{
    public class IncomeProducer : EntityComponent
    {
        [Inject] private IResourceManager _resourceManager;
        [Inject] private IFloatingTextManager _floatingTextManager;

        [SerializeField] private int interval = 1;
        [SerializeField] private int amount = 1;
        [SerializeField] private GameResourceType resourceType;

        private Coroutine _incomeCoroutine;
        
        private void Start()
        {
            _incomeCoroutine = StartCoroutine(IncomeCoroutine());
        }

        private IEnumerator IncomeCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);

                _resourceManager.AddResource(resourceType, amount);

                _floatingTextManager.SpawnFloatingText(
                    transform.position,
                    "Game.FloatingText.Income",
                    FloatingTextType.Income,
                    resourceType,
                    amount
                );
            }
        }
    }
}