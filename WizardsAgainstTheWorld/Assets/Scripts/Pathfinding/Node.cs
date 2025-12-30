using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gCost;
    public int hCost;
    public Node parent;

    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int fCost { get { return gCost + hCost; } }
    
    /// <summary>
    /// Enumerates through all neighbors and their neighbors in a BFS way.
    /// </summary>
    /// <param name="getNeighbors">Function to get neighbors of a node.</param>
    /// <returns>An enumerable of nodes in BFS order.</returns>
    public IEnumerable<Node> EnumerateNeighborsBFS(System.Func<Node, IEnumerable<Node>> getNeighbors)
    {
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var neighbor in getNeighbors(current))
            {
                if (!visited.Contains(neighbor) && neighbor.walkable) // Ensure it's walkable and not visited
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}