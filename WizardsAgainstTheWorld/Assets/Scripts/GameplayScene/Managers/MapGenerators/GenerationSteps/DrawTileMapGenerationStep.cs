using System.Linq;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class DrawTileMapGenerationStep : GenerationStep
    {
        [SerializeField] private Tilemap wallTileMap = null!;
        [SerializeField] private Tilemap floorTileMap = null!;
        
        [SerializeField] private TileBase wallTile = null!;
        [SerializeField] private TileBase floorTile = null!;
        
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var tileSet = GetMapTileSet(settings.mapTileSetOverrideType == MapTileSetOverrideType.None
                ? MapTileSetOverrideType.Default
                : settings.mapTileSetOverrideType);
            
            DrawTiles(data, TileType.Floor, floorTileMap, tileSet.FloorTile);
            DrawTiles(data, TileType.Wall, wallTileMap, tileSet.WallTile);
        }

        public override void Clear()
        {
            base.Clear();
            
            wallTileMap.ClearAllTiles();
            floorTileMap.ClearAllTiles();
        }

        private void DrawTiles(GenerateMapData data, TileType tileType, Tilemap tileMap, TileBase tileBase)
        {
            var tilePositions = data.CreateTileList(tileType).Select(v => new Vector3Int(v.x ,v.y, 0)).ToArray();
            var tiles = data.CreateTileList(tileType).Select(x => tileBase).ToArray();
            tileMap.SetTiles(tilePositions, tiles);
        }
        
        public MapTileSet GetMapTileSet(MapTileSetOverrideType mapTileSetOverrideType)
        {
            return Resources.Load<MapTileSet>($"MapTileSets/{mapTileSetOverrideType}");
        }

    }
}