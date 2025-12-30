using UnityEngine;

public class IgnoreRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity; // Resets rotation every frame
    }
}
