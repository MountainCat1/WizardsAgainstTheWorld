using System;
using System.Collections;
using System.Linq;
using Building;
using GameplayScene.Managers;
using Managers;
using UnityEngine;
using Zenject;

namespace Components.Creatures
{
    public class ResourcePayer : CreatureComponent
    {
        [Inject] private IResourceManager _resourceManager;
        [Inject] private IEntityManager _entityManager;
        

        [field: SerializeField] public float Range { get; private set; } = 1;
        [field: SerializeField] public float BuildSpeed { get; private set; } = 5;
        [field: SerializeField] public int BuildAmount { get; private set; } = 5;

        public float BuildInterval => 1f / BuildSpeed;

        private void Start()
        {
            StartCoroutine(BuildBuildingsCoroutine());
        }

        private IEnumerator BuildBuildingsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(BuildInterval);

                var playerConstructions = _entityManager.PlayerEntities
                    .OfType<BuildingConstruction>()
                    .OrderBy(x => Vector2.Distance(x.transform.position, transform.position));


                foreach (var buildingConstruction in playerConstructions)
                {
                    var neededResources = buildingConstruction.GetRequiredResources();
                    
                    // check if we have eny of the needed resources
                    if (neededResources.Count == 0)
                        continue;

                    bool canPay = false;
                    foreach (var neededResource in neededResources)
                    {
                        var resource = _resourceManager.GetResource(neededResource.Type);
                        if (resource.Amount >= neededResource.Amount)
                        {
                            canPay = true;
                            break;
                        }
                    }
                    
                    if(!canPay)
                        continue;

                    if (Vector2.Distance(buildingConstruction.transform.position, transform.position) > Range)
                        continue;

                    bool paidSomething = false;
                    foreach (var neededResource in neededResources)
                    {
                        var resource = _resourceManager.GetResource(neededResource.Type);
                        int payAmount = Math.Min(neededResource.Amount, Math.Min(BuildAmount, resource.Amount));

                        if (payAmount > 0)
                        {
                            Debug.Log($"Paying {payAmount} of {neededResource.Type} to {buildingConstruction.name}");
                            _resourceManager.AddResource(neededResource.Type, -payAmount);
                            buildingConstruction.PayResource(new GameResource(neededResource.Type)
                            {
                                Amount = payAmount
                            });
                            paidSomething = true;
                            break;
                        }
                    }
                    
                    if (paidSomething)
                        break;
                }
            }
        }
    }
}