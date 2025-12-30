namespace Utilities
{
    using UnityEngine;

    public static class GridSnappingUtility
    {
        /// <summary>
        /// Snaps a Vector2 to the nearest grid point.
        /// </summary>
        /// <param name="position">The original position.</param>
        /// <param name="gridSize">Size of a grid to which the position should be snapped.</param>
        /// <returns>Snapped Vector2 position.</returns>
        public static Vector2 SnapToGrid(Vector2 position, float gridSize)
        {
            return new Vector2(
                Mathf.Round(position.x / gridSize) * gridSize,
                Mathf.Round(position.y / gridSize) * gridSize
            );
        }

        /// <summary>
        /// Snaps a Vector3 to the nearest grid point, preserving the z-coordinate.
        /// </summary>
        /// <param name="position">The original position.</param>
        /// <param name="gridSize">Size of a grid to which the position should be snapped.</param>
        /// <returns>Snapped Vector3 position.</returns>
        public static Vector3 SnapToGrid(Vector3 position, float gridSize)
        {
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                Mathf.Round(position.y / gridSize) * gridSize,
                position.z
            );
        }
    }

}