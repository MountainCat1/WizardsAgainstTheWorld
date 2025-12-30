using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ColliderEventProducer))]
    public class CollisionEventProducerDebugger : ColliderEventProducer
    {
        private void Start()
        {
            var producer = GetComponent<ColliderEventProducer>();
            producer.TriggerEnter += collider => GameLogger.Log($"TriggerEnter: {collider.gameObject.name}");
            producer.TriggerExit += collider => GameLogger.Log($"TriggerExit: {collider.gameObject.name}");
            producer.TriggerStay += collider => GameLogger.Log($"TriggerStay: {collider.gameObject.name}");
        }
    }
}