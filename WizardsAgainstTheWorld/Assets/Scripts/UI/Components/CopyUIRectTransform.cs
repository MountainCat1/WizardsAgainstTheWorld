using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class CopyUIRectTransform : MonoBehaviour
{
    public RectTransform target;

    private RectTransform _self;

    private void Awake()
    {
        _self = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (target == null || _self == null)
            return;

        CopyRectTransformExact(target, _self);
    }

    private void CopyRectTransformExact(RectTransform from, RectTransform to)
    {
        // Cache world position
        Vector3 worldPos = from.position;
        Quaternion worldRot = from.rotation;
        Vector3 worldScale = from.lossyScale;

        // Copy transform properties
        to.anchorMin = from.anchorMin;
        to.anchorMax = from.anchorMax;
        to.pivot = from.pivot;
        to.sizeDelta = from.sizeDelta;
        to.anchoredPosition = from.anchoredPosition;

        // Re-apply world position to match exactly even if parent differs
        to.position = worldPos;
        to.rotation = worldRot;

        // Manually scale (Unity UI usually doesn’t need this, but just in case)
        Vector3 scaleCorrection = new Vector3(
            worldScale.x / to.lossyScale.x,
            worldScale.y / to.lossyScale.y,
            worldScale.z / to.lossyScale.z
        );
        to.localScale = Vector3.Scale(to.localScale, scaleCorrection);
    }
}