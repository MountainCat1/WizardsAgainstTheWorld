using System.Collections.Generic;
using UnityEngine;

public static class PolygonMargin
{
    /// <summary>
    /// Returns a new polygon that is "inset" or "outset" (depending on the sign of padding)
    /// relative to the given polygon.
    ///
    /// <param name="polygon">The original list of vertices (in order: either CCW or CW).</param>
    /// <param name="padding">Positive value shrinks if polygon is clockwise, expands if CCW (and vice versa).
    ///                       The sign/direction depends on orientation and your chosen normal direction.</param>
    /// <returns>A new polygon offset by the specified padding.</returns>
    /// </summary>
    public static List<Vector2> ApplyMargin(List<Vector2> polygon, float padding)
    {
        if (polygon == null || polygon.Count < 3)
            return polygon; // Not a valid polygon

        // 1) Ensure the polygon’s orientation is consistent
        //    We’ll assume we want the polygon in clockwise orientation for “inward” offset
        //    If it’s not, we’ll reverse it.
        bool isClockwise = IsClockwise(polygon);
        if (!isClockwise)
        {
            polygon.Reverse();
        }

        // We’ll do a “shrink” if padding is positive.
        // If your polygon is already clockwise, offsetting “inward” means moving edges to the right of the edge direction
        // (because for a CW polygon, the interior is always on the right side of edges).
        // If you want the opposite behavior, just flip the sign of padding below.

        List<Line> offsetEdges = new List<Line>();

        // 2) Build offset lines for each edge
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count]; // wrap around

            // Edge direction
            Vector2 edgeDir = (next - current).normalized;
            // Normal: for CW polygons, the interior side is to the right of the direction
            // Right normal of (dx, dy) is (dy, -dx)
            Vector2 normal = new Vector2(edgeDir.y, -edgeDir.x);

            // Offset the line by padding in the direction of this normal
            // The line is: a point plus normal, direction is the edge itself
            // We keep it in "Ax + By = C" form for easier intersection
            Vector2 offsetPoint = current + normal * padding;
            Line offsetLine = Line.FromPointDir(offsetPoint, edgeDir);

            offsetEdges.Add(offsetLine);
        }

        // 3) Intersect consecutive offset lines to find new vertices
        List<Vector2> newPolygon = new List<Vector2>(polygon.Count);
        for (int i = 0; i < offsetEdges.Count; i++)
        {
            Line lineA = offsetEdges[i];
            Line lineB = offsetEdges[(i + 1) % offsetEdges.Count];

            bool found = Line.TryGetIntersection(lineA, lineB, out Vector2 intersection);
            if (!found)
            {
                // If lines are parallel or something unexpected, you could handle it here
                // (e.g., skip or keep old vertex). 
                // For simplicity, we just keep the old vertex to avoid errors.
                intersection = polygon[i]; 
            }
            newPolygon.Add(intersection);
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns true if the polygon is in clockwise order.
    /// One approach: 
    ///   - Compute the signed area using the “shoelace” formula, 
    ///   - If it’s negative, the polygon is clockwise; if positive, it’s CCW.
    /// </summary>
    private static bool IsClockwise(List<Vector2> poly)
    {
        float area = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Count];
            area += (b.x - a.x) * (b.y + a.y);
        }
        return (area > 0f) ? false : true;
    }

    // Helper structure to store a line in "Ax + By = C" form and compute intersections
    private struct Line
    {
        public float A;
        public float B;
        public float C;

        // Construct from a point and a direction
        public static Line FromPointDir(Vector2 point, Vector2 dir)
        {
            // Direction = (dx, dy)
            // Normal form: n = perpendicular to direction = (-dy, dx) or (dy, -dx)
            // Then A = n.x, B = n.y, and C = A*px + B*py
            float dx = dir.x;
            float dy = dir.y;

            // A perpendicular to (dx, dy)
            float A = -dy;
            float B = dx;

            float C = A * point.x + B * point.y;
            return new Line { A = A, B = B, C = C };
        }

        /// <summary>
        /// Try to find intersection between two lines. 
        /// Returns true if they intersect in a single point.
        /// </summary>
        public static bool TryGetIntersection(Line l1, Line l2, out Vector2 intersection)
        {
            // l1: A1x + B1y = C1
            // l2: A2x + B2y = C2
            float det = l1.A * l2.B - l2.A * l1.B;
            if (Mathf.Abs(det) < 1e-8f)
            {
                // Lines are parallel or nearly parallel
                intersection = Vector2.zero;
                return false;
            }

            float x = (l2.B * l1.C - l1.B * l2.C) / det;
            float y = (l1.A * l2.C - l2.A * l1.C) / det;
            intersection = new Vector2(x, y);
            return true;
        }
    }
}
