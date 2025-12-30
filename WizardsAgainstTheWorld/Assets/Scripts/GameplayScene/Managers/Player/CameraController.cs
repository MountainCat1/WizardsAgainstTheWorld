using Cinemachine;
using Managers;
using UnityEngine;
using UnityEngine.Animations;
using Zenject;

public interface ICameraController
{
    void MoveTo(Vector2 targetPosition);
    void SetParent(Transform parent);
}

public class CameraController : MonoBehaviour, ICameraController
{
    [Inject] IInputManager _inputManager;
    [Inject] IInputMapper _inputMapper;
    [Inject] ISelectionInspectionManager _inspectionManager;

    [SerializeField] private Transform cameraParent;
    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float scrollSensitive = 1f;
    [SerializeField] private GameObject defaultCameraTarget;

    [Header("Edge Panning")]
    [SerializeField] private float edgeThreshold = 30f; // pixels from edge
    [SerializeField] private bool enableEdgePanning = true;
    
    private CinemachineVirtualCamera _camera;
    private bool _hasFocus;

    private void Start()
    {
        _inputManager.CameraMovement += OnCameraMovement;
        _camera = Camera.main.GetComponent<CinemachineVirtualCamera>();
        _inputMapper.Zoom += OnZoom;
        _inputManager.ZoomOnInspectedUnit += ZoomOnInspectedUnit;
        
        if (defaultCameraTarget != null)
        {
            SetParent(defaultCameraTarget.transform);
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;
    }
    
    private void OnDestroy()
    {
        _inputManager.CameraMovement -= OnCameraMovement;
        _inputMapper.Zoom -= OnZoom;
    }
    
    
    private void Update()
    {
        if (!enableEdgePanning)
            return;

        if (!_hasFocus)
            return;

        HandleEdgePanning();
    }


    private void HandleEdgePanning()
    {
        if (!IsMouseInsideGameWindow())
            return;

        Vector3 move = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x <= edgeThreshold)
            move.x -= 1f;
        else if (mousePos.x >= Screen.width - edgeThreshold)
            move.x += 1f;

        if (mousePos.y <= edgeThreshold)
            move.y -= 1f;
        else if (mousePos.y >= Screen.height - edgeThreshold)
            move.y += 1f;

        if (move == Vector3.zero)
            return;

        move.Normalize();
        cameraParent.position += move * cameraSpeed * Time.deltaTime * _camera.m_Lens.OrthographicSize;
    }


    private void ZoomOnInspectedUnit()
    {
        var inspectedCreature = _inspectionManager.SelectedInspectedCreature;
        if (inspectedCreature == null)
            return;
        
        var targetPosition = inspectedCreature.transform.position;
        cameraParent.position = new Vector3(targetPosition.x, targetPosition.y, cameraParent.position.z);
    }

    private void OnZoom(float zoom)
    {
        _camera.m_Lens.OrthographicSize = Mathf.Clamp(_camera.m_Lens.OrthographicSize - zoom * scrollSensitive, 5, 20);
    }

    private void OnCameraMovement(Vector2 move)
    {
        cameraParent.transform.position += new Vector3(move.x, move.y, 0) * Time.deltaTime * cameraSpeed * _camera.m_Lens.OrthographicSize;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        cameraParent.transform.position = new Vector3(targetPosition.x, targetPosition.y, cameraParent.transform.position.z);
    }

    public void SetParent(Transform parent)
    {
        cameraParent.GetComponent<ParentConstraint>().SetSource(0, new ConstraintSource
        {
            sourceTransform = parent,
            weight = 1f
        });
    }

    private bool IsMouseInsideGameWindow()
    {
        Vector3 mousePos = Input.mousePosition;

        return mousePos.x >= 0 &&
               mousePos.y >= 0 &&
               mousePos.x <= Screen.width &&
               mousePos.y <= Screen.height;
    }

}