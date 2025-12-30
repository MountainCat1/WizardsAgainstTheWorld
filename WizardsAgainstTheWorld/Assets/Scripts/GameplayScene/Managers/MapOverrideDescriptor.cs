using UnityEngine;
using UnityEngine.Tilemaps;

namespace Managers
{
    public class MapOverrideDescriptor : MonoBehaviour
    {
        [field: SerializeField] public Tilemap FloorTileMap { get; private set; }
        [field: SerializeField] public Tilemap WallTileMap { get; private set; }
    }
}