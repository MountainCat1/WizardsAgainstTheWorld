using System;
using System.Collections;
using System.Linq;
using Building;
using GameplayScene.Managers;
using Managers;
using UnityEngine;
using Visual;
using Zenject;

namespace Components.Creatures
{
    public class ResourcePayer : CreatureComponent
    {
        [Inject] private IResourceManager _resourceManager;
        [Inject] private IEntityManager _entityManager;
        [Inject] private IDynamicPoolingManager _dynamicPoolingManager;
        [Inject] private ISoundPlayer _soundPlayer;
        
        [field: SerializeField] public float Range { get; private set; } = 1;
        [field: SerializeField] public float BuildSpeed { get; private set; } = 5;
        [field: SerializeField] public int BuildAmount { get; private set; } = 5;
        [field: SerializeField] public PaymentVisual PaymentVisualPrefab { get; private set; }
        [field: SerializeField] public AudioClip PaySound { get; private set; }

        public float BuildInterval => 1f / BuildSpeed;
        private IPoolAccess<PaymentVisual> _visualsPool;

        private void Start()
        {
            StartCoroutine(BuildBuildingsCoroutine());
            _visualsPool = _dynamicPoolingManager.GetPoolAccess<PaymentVisual>("PaymentVisualPool");
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
                            Pay(payAmount, neededResource, buildingConstruction);
                            paidSomething = true;
                            break;
                        }
                    }
                    
                    if (paidSomething)
                        break;
                }
            }
        }

        private void Pay(int payAmount, GameResource neededResource, BuildingConstruction buildingConstruction)
        {
            Debug.Log($"Paying {payAmount} of {neededResource.Type} to {buildingConstruction.name}");
            _resourceManager.AddResource(neededResource.Type, -payAmount);
            buildingConstruction.PayResource(new GameResource(neededResource.Type)
            {
                Amount = payAmount
            });
            
            var paymentVisual = _visualsPool.SpawnObject(PaymentVisualPrefab, transform.position);
            paymentVisual.Setup(_resourceManager.GetIcon(neededResource.Type), buildingConstruction.transform);

            _soundPlayer.PlaySound(PaySound, transform.position, SoundType.UI);
        }
    }
}