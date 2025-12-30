using UnityEngine;

namespace UI
{
    public class HighlightableUI : MonoBehaviour
    {
        [SerializeField] private bool highlighted;
        [SerializeField] private GameObject activateOnHighlighted;

        public bool Highlighted
        {
            get => highlighted;
            set
            {
                highlighted = value;
                OnChange();
            }
        }

        private void OnChange()
        {
            activateOnHighlighted.SetActive(highlighted);
        }
    }
}