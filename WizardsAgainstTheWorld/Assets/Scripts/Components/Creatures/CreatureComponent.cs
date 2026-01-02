using UnityEngine;

namespace Components.Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureComponent : MonoBehaviour
    {
        public Creature Creature { get; private set; }

        protected virtual void Awake()
        {
            Creature = GetComponent<Creature>();
        }
    }
}