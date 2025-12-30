using Managers;
using Markers;
using UnityEngine;
using Zenject;

namespace Components
{
    [RequireComponent(typeof(DamageableCollider))]
    public class Destructible : MonoBehaviour, IDamageable
    {
        [Inject] private ILootManager _lootManager;
        [Inject] private IAstarManager _astarManager;

        public HealthComponent Health { get; private set; }
        private ContainerObject _containerObject;
        private RandomContainerObject _randomContainerObject;


        private void Awake()
        {
            Health = GetComponent<HealthComponent>();

            // TODO: hack
            _containerObject = GetComponent<ContainerObject>();
            _randomContainerObject = GetComponent<RandomContainerObject>();
            //

            if (Health == null)
            {
                GameLogger.LogError("Destructible script requires a HealthComponent!");
                enabled = false; // Disable the script if no HealthComponent is found
                return;
            }

            Health.Death += (_) => OnDeath();
        }
        
        private void OnDeath()
        {
            // TODO: hack
            if (_containerObject?.ItemBehaviour != null && _containerObject.IsInteractable)
            {
                _lootManager.SpawnPickup(_containerObject.ItemBehaviour, transform.position);
            }
            
            if (_randomContainerObject != null && _randomContainerObject.IsInteractable)
            {
                var item = _randomContainerObject.GetRandomItem();
                if(item != null)
                    _lootManager.SpawnPickup(item, transform.position);
            }
            
            _astarManager.ScanDelayed();

            Destroy(gameObject);
        }
    }
}