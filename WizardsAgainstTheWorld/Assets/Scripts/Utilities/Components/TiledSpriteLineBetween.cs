using UnityEngine;

namespace Utilities.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TiledSpriteLineBetween : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float width = 0.1f;

        private Transform _startPoint;
        private Transform _endPoint;

        void Awake()
        {
            if (spriteRenderer != null)
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        }

        void Update()
        {
            if (_startPoint == null || _endPoint == null || spriteRenderer == null)
            {
                spriteRenderer.enabled = false;
                return;
            }

            ;

            spriteRenderer.enabled = true;

            Vector3 dir = _endPoint.position - _startPoint.position;
            Vector3 center = (_startPoint.position + _endPoint.position) / 2f;

            transform.position = center;
            transform.right = dir.normalized;

            // Use dir.magnitude for length and `width` for thickness
            spriteRenderer.size = new Vector2(dir.magnitude, width);
        }

        public void SetPoints(Transform start, Transform end)
        {
            _startPoint = start;
            _endPoint = end;
        }
    }
}