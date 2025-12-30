using UnityEngine;

namespace Markers
{
    [RequireComponent(typeof(Collider2D))]
    public class CreatureCollider : EntityCollider
    {
        public Creature Creature { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Creature = GetComponent<Creature>()
                       ?? GetComponentInParent<Creature>()
                       ?? throw new MissingComponentException("Creature component not found");
        }
    }
}