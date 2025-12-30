using UnityEngine;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapTileSet", menuName = "Custom/MapTileSet", order = 1)]
    public class MapTileSet : ScriptableObject
    {
        [field: SerializeField] public TileBase WallTile { get; private set; } = null!;
        [field: SerializeField] public TileBase FloorTile { get; private set; } = null!;
    }
}