using UnityEngine;

namespace Components
{
    public interface IColorable
    {
        Color Color { get; }
    }
    
    [RequireComponent(typeof(SpriteRenderer))]
    public class ReplaceSpriteColor : MonoBehaviour
    {
        private static readonly int ReplacementColor = Shader.PropertyToID("_ReplacementColor");
        [SerializeField] private Entity entity;
        
        private SpriteRenderer _spriteRenderer;
        private Material _material;
        private IColorable _colorable;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                GameLogger.LogError("SpriteRenderer component not found on the GameObject.");
                return;
            }

            if (entity is not IColorable colorable)
            {
                GameLogger.LogError("Entity does not implement IColorable interface.");
                return;
            }

            _colorable = colorable;
            _material = _spriteRenderer.material;
        }

        private void Update()
        {
            _material.SetColor(ReplacementColor, _colorable.Color);
        }
    }
}