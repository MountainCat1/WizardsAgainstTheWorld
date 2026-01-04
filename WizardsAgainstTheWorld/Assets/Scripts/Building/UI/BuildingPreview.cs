using UnityEngine;

namespace Building.UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BuildingPreview : MonoBehaviour
    {
        [SerializeField] private Color canBuildColor = Color.green;
        [SerializeField] private Color cannotBuildColor = Color.red;
        [SerializeField, Range(0f, 1f)] private float alpha = 0.5f;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(bool canBuild,
            Sprite buildingSprite,
            BuildingPrefab selectedBuildingDefinition,
            Vector3 position
        )
        {
            if (buildingSprite == null)
            {
                throw new System.ArgumentNullException(nameof(buildingSprite));
            }

            _spriteRenderer.sprite = buildingSprite;

            Color baseColor = canBuild ? canBuildColor : cannotBuildColor;
            baseColor.a = alpha;
            _spriteRenderer.color = baseColor;

            transform.localScale = Vector3.one;
            _spriteRenderer.size = selectedBuildingDefinition.MainSpriteRenderer.size;
            transform.position = position;
        }
    }
}