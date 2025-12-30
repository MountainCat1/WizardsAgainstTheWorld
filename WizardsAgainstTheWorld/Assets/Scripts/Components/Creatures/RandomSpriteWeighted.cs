using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteWeighted : MonoBehaviour
{
    [SerializeField] private WeightedSprite sprites;

    private SpriteRenderer _spriteRenderer;
    
    private void OnEnable()
    {
        if(!_spriteRenderer)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _spriteRenderer.sprite = sprites.GetRandomItem();
    }
}