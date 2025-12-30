using UnityEngine;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
public class TooltipOffseter : MonoBehaviour
{
    [SerializeField] private ToolTip tooltip;
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Account for canvas scaling
        Vector3 scaleFactor = _rectTransform.lossyScale;

        tooltip.YShift = (_rectTransform.rect.height * scaleFactor.y) / 2f;
        tooltip.xShift = -(_rectTransform.rect.width * scaleFactor.x) / 2f;
    }
}