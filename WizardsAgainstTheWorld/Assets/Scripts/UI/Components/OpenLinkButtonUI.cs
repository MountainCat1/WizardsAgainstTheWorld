using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenLinkButtonUI : MonoBehaviour
{
    [SerializeField] private string url;

    private Button _button;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.onClick.AddListener(OpenLink);
    }

    private void OpenLink()
    {
        if (!string.IsNullOrWhiteSpace(url))
            Application.OpenURL(url);
    }
}