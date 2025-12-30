namespace Utilities.Components
{
    using UnityEngine;

    using UnityEngine;

    [RequireComponent(typeof(LineRenderer))]
    public class LineBetween : MonoBehaviour
    {
        public Transform startPoint;
        public Transform endPoint;
        public Material litMaterial; // assign this in the inspector

        private LineRenderer _lineRenderer;

        void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;

            // Setup appearance
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;

            if (litMaterial != null)
                _lineRenderer.material = litMaterial;
        }

        void Update()
        {
            if (startPoint != null && endPoint != null)
            {
                _lineRenderer.SetPosition(0, startPoint.position);
                _lineRenderer.SetPosition(1, endPoint.position);
            }
        }
    }


}