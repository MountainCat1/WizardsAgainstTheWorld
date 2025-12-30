using UnityEngine;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Tiles/Random Sprite Tile")]
    public sealed class RandomSpriteTile : TileBase
    {
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private int seed = 1337;

        public override void GetTileData(
            Vector3Int position,
            ITilemap tilemap,
            ref TileData tileData)
        {
            if (sprites == null || sprites.Length == 0)
                return;

            int hash = Hash(position.x, position.y, seed);
            int index = Mathf.Abs(hash) % sprites.Length;

            tileData.sprite = sprites[index];
            tileData.colliderType = Tile.ColliderType.Sprite;
        }

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = seed;
                h = h * 73856093 ^ x;
                h = h * 19349663 ^ y;
                return h;
            }
        }
    }
}