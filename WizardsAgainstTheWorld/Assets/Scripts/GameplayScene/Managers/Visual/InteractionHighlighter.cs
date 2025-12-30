using System.Linq;
using Interactables;
using UnityEngine;
using Zenject;

namespace Managers.Visual
{
    public class InteractionHighlighter : MonoBehaviour
    {
        [Inject] private IDynamicPoolingManager _dynamicPoolingManager;
        [Inject] private IUnderCursorManager _underCursorManager;
        [Inject] private ISelectionManager _selectionManager;

        [SerializeField] private GameObject highlight;
        [SerializeField] private SpriteRenderer highlightRenderer;

        [SerializeField] private Color normalHighlight = Color.white;
        [SerializeField] private Color aggressiveHighlight = Color.red;

        private void Start()
        {
            _underCursorManager.HoveredObjectChanged += OnHoveredObjectChanged;
            highlight.SetActive(false);
        }

        private void OnHoveredObjectChanged(IHoverable obj)
        {
            if (obj == null)
            {
                highlight.SetActive(false);
                return;
            }

            highlight.SetActive(true);
            highlightRenderer.color = normalHighlight;
                
                
            switch (obj)
            {
                case InteractionBehavior interactionBehavior:
                    var selectedCreatures = _selectionManager.SelectedCreatures;
                    var interaction = interactionBehavior.Container
                        .FirstOrDefault(x => selectedCreatures.Any(x.CanInteract));

                    if (interaction is DismantableObject)
                    {
                        highlightRenderer.color = aggressiveHighlight;
                    }
                    
                    if (interaction != null)
                    {
                        highlight.transform.position = interactionBehavior.transform.position;
                    }
                    else
                    {
                        highlight.SetActive(false);
                    }

                    break;

                default:
                    highlight.SetActive(false);
                    return;
            }
        }
    }
}