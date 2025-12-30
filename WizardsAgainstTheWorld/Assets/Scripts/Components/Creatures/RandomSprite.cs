using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSprite : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private SpriteRenderer _spriteRenderer;
    
    private void OnEnable()
    {
        if(!_spriteRenderer)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}