using UnityEngine;
using Zenject;

namespace CreatureControllers
{
    [RequireComponent(typeof(Entity))]
    public abstract class EntityController : MonoBehaviour
    {
        public Entity Entity { get; protected set; }
    
        [Inject] private IPathfinding _pathfinding;

        protected virtual void Awake()
        {
            Entity = GetComponent<Entity>();
        }
    }
}