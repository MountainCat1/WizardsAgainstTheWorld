using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TilePolygon
{
    public static List<Vector2> GetWrappingPolygon(List<Vector2Int> tilePositions, float tileSize)
    {
        // 1) Build outer-edge set
        var edges = new HashSet<(Vector2 Start, Vector2 End)>();

        foreach (var tile in tilePositions)
        {
            Vector2 bottomLeft  = (Vector2)tile * tileSize;
            Vector2 bottomRight = bottomLeft + new Vector2(tileSize, 0);
            Vector2 topLeft     = bottomLeft + new Vector2(0, tileSize);
            Vector2 topRight    = bottomLeft + new Vector2(tileSize, tileSize);

            // Define edges for this tile
            var tileEdges = new (Vector2, Vector2)[]
            {
                (bottomLeft,  bottomRight),
                (bottomRight, topRight),
                (topRight,    topLeft),
                (topLeft,     bottomLeft),
            };

            // Add/remove edges accordingly
            foreach (var edge in tileEdges)
            {
                // If reverse edge exists, remove it (it's an internal edge).
                if (!edges.Remove((edge.Item2, edge.Item1)))
                {
                    // Otherwise, add this one
                    edges.Add(edge);
                }
            }
        }

        // 2) Turn edges into ordered polygon
        var orderedPolygon = OrderEdges(edges);

        // 3) Remove collinear points
        var simplifiedPolygon = RemoveCollinearPoints(orderedPolygon);

        return simplifiedPolygon;
    }

    /// <summary>
    /// Orders a set of edges into a continuous polygon path.
    /// </summary>
    private static List<Vector2> OrderEdges(HashSet<(Vector2 Start, Vector2 End)> edges)
    {
        // Create a map from Start->End so we can 'walk' the edges
        var edgeDict = new Dictionary<Vector2, Vector2>();
        foreach (var (start, end) in edges)
        {
            // Assuming there's no overlap in "start" keys
            edgeDict[start] = end;
        }

        // Start from the first available key
        var polygon = new List<Vector2>();
        if (edgeDict.Count == 0)
            return polygon;

        var current = edgeDict.Keys.First();
        polygon.Add(current);

        // Follow the chain
        while (edgeDict.TryGetValue(current, out var next))
        {
            polygon.Add(next);
            edgeDict.Remove(current);
            current = next;
        }

        return polygon;
    }

    /// <summary>
    /// Removes points that are collinear with their neighbors.
    /// </summary>
    private static List<Vector2> RemoveCollinearPoints(List<Vector2> polygon)
    {
        // If fewer than 3 points, no need to remove anything
        if (polygon.Count < 3) 
            return polygon;

        // We can repeatedly remove collinear points until no changes
        bool changed = true;
        while (changed)
        {
            changed = false;

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
                Vector2 curr = polygon[i];
                Vector2 next = polygon[(i + 1) % polygon.Count];

                if (IsCollinear(prev, curr, next))
                {
                    // Remove the middle vertex (curr)
                    polygon.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }

        return polygon;
    }

    /// <summary>
    /// Checks if three points A, B, C are collinear, using cross product ~= 0.
    /// </summary>
    private static bool IsCollinear(Vector2 A, Vector2 B, Vector2 C)
    {
        // Cross product of AB x AC
        float cross = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);

        // Use an epsilon to account for floating-point precision
        const float epsilon = 0.0001f;
        return Mathf.Abs(cross) < epsilon;
    }
}
