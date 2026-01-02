using System.Collections.Generic;
using Components;
using Markers;
using UnityEngine;

namespace Utilities
{
    public static class CollisionUtility
    {
        public static bool IsObstacle(GameObject go)
        {
            return go.layer == LayerMask.NameToLayer("Obstacles");
        }
        
        public static bool IsWall(GameObject go)
        {
            return go.layer == LayerMask.NameToLayer("Walls");
        }

        public static int BlockingVisionLayerMask => LayerMask.GetMask("Obstacles", "Walls");
        public static int UnwalkableLayerMask => LayerMask.GetMask("Obstacles", "Walls", "Building");
        
        public static List<IDamageable> GetCreaturesInRadius(Vector2 position, float radius, IDamageable[] ignore = null)
        {
            List<IDamageable> hitCreatures = new();
            HashSet<IDamageable> ignoreSet = ignore != null ? new(ignore) : new();

            Collider2D[] results = new Collider2D[50];
            int size = Physics2D.OverlapCircleNonAlloc(position, radius, results);

            for (int i = 0; i < size; i++)
            {
                var hitCollider = results[i];
                var hitDamageable = hitCollider.GetComponent<DamageableCollider>()?.Damagable;

                if (hitDamageable == null || ignoreSet.Contains(hitDamageable))
                    continue;

                if (Creature.IsCreature(hitCollider.gameObject))
                {
                    hitCreatures.Add(hitDamageable);
                }
            }

            return hitCreatures;
        }
    }
}