using UnityEngine;


namespace DefaultNamespace
{
    public class CameraSettingsApplier : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private UnityEngine.Rendering.Universal.PixelPerfectCamera pixelPerfectCamera;

        private void Start()
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (pixelPerfectCamera == null)
            {
                pixelPerfectCamera = GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            }

            ApplySettings();
        }

        private void ApplySettings()
        {
            pixelPerfectCamera.enabled = GameSettings.Instance.Visual.CameraSettings.PixelPerfectEnabled;
        }
    }
}