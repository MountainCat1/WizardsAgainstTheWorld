using System;
using UnityEngine;
using UnityEngine.Animations;

namespace Managers
{
    [RequireComponent(typeof(ParentConstraint))]
    public class SelectionMarker : MonoBehaviour, IFreeable
    {
        public ParentConstraint ParentConstraint { get; private set; }
        public Creature Creature { get; set; }
        
        void Awake()
        {
            ParentConstraint = GetComponent<ParentConstraint>();
        }

        public void Deinitialize()
        {
            // Remove all sources
            for (int i = 0; i < ParentConstraint.sourceCount; i++)
            {
                ParentConstraint.RemoveSource(i);
            }
        }

        public void Initialize(Action free)
        {
        }

        public void SetTarget(Creature creature)
        {
            if (creature == null)
            {
                Deinitialize();
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);

            Creature = creature;

            // Clear existing sources
            Deinitialize();

            // Add new source
            ParentConstraint.AddSource(new ConstraintSource
            {
                sourceTransform = creature.transform,
                weight = 1f
            });

            // Reapply the constraint
            ParentConstraint.constraintActive = true;
        }
    }
}