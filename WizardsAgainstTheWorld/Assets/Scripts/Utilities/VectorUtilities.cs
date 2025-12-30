using UnityEngine;

namespace Utilities
{
    public static class VectorUtilities
    {
        public static Vector3 GetRandomPositionOnCircleEdge(Vector3 center, float radius)
        {
            float randomAngle = Random.Range(0f, 360f);
            float spawnX = center.x + radius * Mathf.Cos(Mathf.Deg2Rad * randomAngle);
            float spawnY = center.y + radius * Mathf.Sin(Mathf.Deg2Rad * randomAngle);

            return new Vector3(spawnX, spawnY, center.z);
        }
        
        //
        public static Vector3 RoundToNearest(this Vector3 value, float nearest)
        {
            return new Vector3(
                Mathf.Round(value.x / nearest) * nearest,
                Mathf.Round(value.y / nearest) * nearest,
                Mathf.Round(value.z / nearest) * nearest
            );
        }
        
        public static Vector3 RoundToNearest(this Vector3 value)
        {
            return new Vector3(
                Mathf.Round(value.x),
                Mathf.Round(value.y),
                Mathf.Round(value.z)
            );
        }

        
        public static Vector2 RoundToNearest(this Vector2 value, float nearest)
        {
            return new Vector2(
                Mathf.Round(value.x / nearest) * nearest,
                Mathf.Round(value.y / nearest) * nearest
            );
        }
        
        public static Vector2 RoundToNearest(this Vector2 value)
        {
            return new Vector2(
                Mathf.Round(value.x),
                Mathf.Round(value.y)
            );
        }
        
        public static Vector2 GetRandomDirection2D()
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }
    }
}