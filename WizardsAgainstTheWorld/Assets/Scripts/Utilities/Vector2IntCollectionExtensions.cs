using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Vector2IntCollectionExtensions
    {
        public static Vector2 GetAverageCenter(this ICollection<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return Vector2.zero;

            int sumX = 0;
            int sumY = 0;

            foreach (var pos in positions)
            {
                sumX += pos.x;
                sumY += pos.y;
            }

            return new Vector2((float)sumX / positions.Count, (float)sumY / positions.Count);
        }
    }
}