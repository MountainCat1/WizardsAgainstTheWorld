using UnityEngine;

namespace Markers
{
    [RequireComponent(typeof(Collider2D))]
    public class EntityCollider : MonoBehaviour
    {
        private Entity _entity;

        public Entity Entity
        {
            get
            {
                if (_entity == null)
                {
                    GameLogger.LogError("Something went wrong, EntityCollider requires a Creature component " + gameObject.name);
                }
                return _entity;
            }
            private set { _entity = value; }
        }

        protected virtual void Awake()
        {
            Entity = GetComponent<Creature>() 
                       ?? GetComponentInParent<Creature>() 
                       ?? throw new MissingComponentException("EntityCollider requires a Creature component");
        }
    }
}