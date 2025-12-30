using System.Collections.Generic;
using Constants;
using UnityEngine;

namespace DefaultNamespace.Pathfinding
{
    public static class PathfindingUtilities
    {
        public static List<Vector2> GetSpreadPosition(Vector2 center, int count,
            LayerMask collisionLayer, float spaceBetween)
        {
            List<Vector2> positions = new List<Vector2>();
            Queue<Vector2> positionQueue = new Queue<Vector2>();
            HashSet<Vector2> visitedPositions = new HashSet<Vector2>();

            var directions = Directions.Compass;

            positionQueue.Enqueue(center);
            visitedPositions.Add(center);

            while (positions.Count < count && positionQueue.Count > 0)
            {
                Vector2 currentPosition = positionQueue.Dequeue();

                if (Physics2D.OverlapCircle(currentPosition, spaceBetween / 4f, collisionLayer))
                {
                    visitedPositions.Add(currentPosition);

                    if (center == currentPosition)
                    {
                        foreach (var direction in directions)
                        {
                            Vector2 newPosition = currentPosition + direction * spaceBetween;

                            if (!visitedPositions.Contains(newPosition))
                            {
                                visitedPositions.Add(newPosition);
                                positionQueue.Enqueue(newPosition);
                            }
                        }
                    }

                    continue;
                }


                positions.Add(currentPosition);

                foreach (var direction in directions)
                {
                    Vector2 newPosition = currentPosition + direction * spaceBetween;

                    if (!visitedPositions.Contains(newPosition))
                    {
                        visitedPositions.Add(newPosition);
                        positionQueue.Enqueue(newPosition);
                    }
                }
            }

            return positions;
        }
    }
}