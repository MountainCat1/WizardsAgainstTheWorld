using UnityEngine;

namespace Utilities.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteLineBetween : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float width = 0.1f;

        private Transform _startPoint;
        private Transform _endPoint;

        private Vector3? _cachedStartPos;
        private Vector3? _cachedEndPos;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void Update()
        {
            // Check if references are still valid
            bool startValid = _startPoint != null;
            bool endValid = _endPoint != null;

            if (startValid)
                _cachedStartPos = _startPoint.position;
            if (endValid)
                _cachedEndPos = _endPoint.position;

            if (_cachedStartPos == null || _cachedEndPos == null)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;
                return;
            }

            if (spriteRenderer != null && !spriteRenderer.enabled)
                spriteRenderer.enabled = true;

            Vector3 start = _cachedStartPos.Value;
            Vector3 end = _cachedEndPos.Value;

            Vector3 dir = end - start;
            float length = dir.magnitude;
            Vector3 center = (start + end) / 2f;

            transform.position = center;
            transform.right = dir.normalized;
            transform.localScale = new Vector3(length, width, 1f);
        }

        public void SetPoints(Transform start, Transform end, bool enableSpriteRenderer)
        {
            _startPoint = start;
            _endPoint = end;
            _cachedStartPos = start != null ? start.position : null;
            _cachedEndPos = end != null ? end.position : null;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = enableSpriteRenderer;
            }
        }
    }
}
