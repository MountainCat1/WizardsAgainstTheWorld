// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Managers
// {
//     public interface IPopupManager
//     {
//         void SpawnFloatingText(string text, Vector3 position, Color color);
//     }
//
//     public class PopupManager : MonoBehaviour, IPopupManager
//     {
//         [FormerlySerializedAs("popupPrefab")]
//         [Header("Dependencies")]
//         [SerializeField] private FloatingTextUI floatingTextPrefab;
//         [SerializeField] private Transform floatingTextContainer;
//         
//         [Header("Settings")]
//         [SerializeField] private float timeToDestroyPopup = 1f;
//         [SerializeField] private Vector2 popupOffset = new Vector2(0, 0.5f);
//         
//         public void SpawnFloatingText(string text, Vector3 position, Color color)
//         {
//             var popup = Instantiate(floatingTextPrefab, floatingTextContainer);
//             popup.transform.position = position;
//             popup.Text.text = text;
//             popup.Text.color = color;
//
//             Destroy(popup.gameObject, timeToDestroyPopup);
//         }
//     }
// }