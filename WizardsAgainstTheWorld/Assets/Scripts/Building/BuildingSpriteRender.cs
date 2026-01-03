using System;
using UnityEngine;

namespace Building
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BuildingSpriteRender : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        
        [SerializeField] private BuildingPrefab buildingPrefab;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.size = new Vector2(buildingPrefab.Footprint.Width, buildingPrefab.Footprint.Height);
        }
    }
}