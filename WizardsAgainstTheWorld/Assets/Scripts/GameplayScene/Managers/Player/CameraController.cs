using Cinemachine;
using Managers;
using UnityEngine;
using Zenject;

public interface ICameraController
{
    void MoveTo(Vector2 targetPosition);
}

public class CameraController : MonoBehaviour, ICameraController
{
    [Inject] IInputManager _inputManager;
    [Inject] IInputMapper _inputMapper;
    [Inject] ISelectionInspectionManager _inspectionManager;

    [SerializeField] private Transform cameraParent;
    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float scrollSensitive = 1f;

    [Header("Edge Panning")]
    [SerializeField] private float edgeThreshold = 30f; // pixels from edge
    [SerializeField] private bool enableEdgePanning = true;
    
    private CinemachineVirtualCamera _camera;

    private void Start()
    {
        _inputManager.CameraMovement += OnCameraMovement;
        _camera = Camera.main.GetComponent<CinemachineVirtualCamera>();
        _inputMapper.Zoom += OnZoom;
        _inputManager.ZoomOnInspectedUnit += ZoomOnInspectedUnit;
    }
    
    private void OnDestroy()
    {
        _inputManager.CameraMovement -= OnCameraMovement;
        _inputMapper.Zoom -= OnZoom;
    }
    
    
    private void Update()
    {
        if (enableEdgePanning)
            HandleEdgePanning();
    }

    private void HandleEdgePanning()
    {
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

        if (move != Vector3.zero)
        {
            move.Normalize();
            cameraParent.position += move * (cameraSpeed * Time.deltaTime * _camera.m_Lens.OrthographicSize);
        }
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
}