using UnityEngine;

namespace Components.Entities
{
    [RequireComponent(typeof(Entity))]
    public class EntityComponent : MonoBehaviour
    {
        public Entity Entity { get; private set; }
        
        protected virtual void Awake()
        {
            Entity = GetComponent<Entity>();
        }
    }
}