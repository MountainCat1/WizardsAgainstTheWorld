using System.Collections.Generic;
using UnityEngine;

namespace Constants
{
    public static class Directions
    {
        public static IList<Vector2> Compass = new List<Vector2>
        {
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        };
        
        public static IList<Vector2> Diagonals = new List<Vector2>
        {
            new Vector2(1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1),
            new Vector2(-1, 1)
        };
        
        public static IList<Vector2> AllDirections = new List<Vector2>
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(1, -1),
            new Vector2(0, -1),
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            new Vector2(-1, 1)
        };
    }
}