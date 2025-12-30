
using UnityEngine;

using System.Collections.Generic;
using System.Linq;


public class TileCluster
{
    public static List<List<Vector2Int>> GetConnectedClusters(IEnumerable<Vector2Int> tilePositions)
    {
        var remainingTiles = new HashSet<Vector2Int>(tilePositions);
        var clusters = new List<List<Vector2Int>>();

        // Define the four cardinal directions for adjacency (side-touching)
        var directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0)  // Left
        };

        while (remainingTiles.Count > 0)
        {
            var cluster = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            // Start a new cluster from any remaining tile
            var startTile = remainingTiles.First();
            queue.Enqueue(startTile);
            remainingTiles.Remove(startTile);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                cluster.Add(current);

                // Check all neighboring tiles
                foreach (var direction in directions)
                {
                    var neighbor = current + direction;
                    if (remainingTiles.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        remainingTiles.Remove(neighbor);
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}
