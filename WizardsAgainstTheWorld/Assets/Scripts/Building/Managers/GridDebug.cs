using UnityEngine;
using Zenject;

namespace Building.Managers
{
    public sealed class GridDebug : MonoBehaviour
    {
        [Inject] private GridSystem _grid;

        [SerializeField] private bool drawDebug = true;

        [SerializeField] private Color emptyCellColor = Color.white;
        [SerializeField] private Color blockedCellColor = Color.red;

        private const float Z_OFFSET = 0.01f;

        private void OnDrawGizmos()
        {
            if (!drawDebug || _grid == null)
                return;

            for (var x = 0; x < _grid.Width; x++)
            for (var y = 0; y < _grid.Height; y++)
            {
                var pos = new GridPosition(x, y);
                var cell = _grid.GetCell(pos);

                Gizmos.color = cell.Walkable
                    ? emptyCellColor
                    : blockedCellColor;

                DrawCellX(_grid.GridToWorld(pos));
            }
        }

        private void DrawCellX(Vector3 center)
        {
            var half = _grid.CellSize * 0.25f;

            var topLeft = new Vector3(
                center.x - half,
                center.y + half,
                Z_OFFSET);

            var topRight = new Vector3(
                center.x + half,
                center.y + half,
                Z_OFFSET);

            var bottomLeft = new Vector3(
                center.x - half,
                center.y - half,
                Z_OFFSET);

            var bottomRight = new Vector3(
                center.x + half,
                center.y - half,
                Z_OFFSET);

            Gizmos.DrawLine(topLeft, bottomRight);
            Gizmos.DrawLine(topRight, bottomLeft);
        }
    }
}