using System.Collections.Generic;
using System.Linq;
using ShadowModule;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


public partial class TilemapShadowGenerator : MonoBehaviour
{
    [FormerlySerializedAs("tilemap")] [SerializeField]
    private Tilemap wallTilemap;

    [SerializeField] private ShadowCaster2D shadowCasterPrefab;

    [SerializeField] private float margin = 0.1f;

    [SerializeField] private bool debugClusters = false;

    // Called when the node enters the scene tree for the first time.

    private void Awake()
    {
        UpdateShadows();
    }

    private void OnDrawGizmos()
    {
        if (debugClusters)
        {
            var getAllTilePositionsOfType =
                GetAllTilePositions(wallTilemap).Select(x => new Vector2Int(x.x, x.y)).ToList();

            var clusters = TileCluster.GetConnectedClusters(getAllTilePositionsOfType);

            // List of predefined colors, or you can dynamically create random colors.
            List<Color> clusterColors = new List<Color>
            {
                Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white, Color.black,
                Color.gray
            };

            int colorIndex = 0;

            foreach (var cluster in clusters)
            {
                var wrapPolygon = TilePolygon.GetWrappingPolygon(cluster, wallTilemap.cellSize.x);
                wrapPolygon = PolygonMargin.ApplyMargin(wrapPolygon, margin);

                // Cycle through colors or generate new ones.
                Gizmos.color = clusterColors[colorIndex % clusterColors.Count];
                colorIndex++;

                for (int i = 0; i < wrapPolygon.Count; i++)
                {
                    var next = wrapPolygon[(i + 1) % wrapPolygon.Count];
                    Gizmos.DrawLine(wrapPolygon[i], next);
                }
            }
        }
    }

    public void UpdateShadows()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var wallTiles = GetAllTilePositions(wallTilemap).Select(x => new Vector2Int(x.x, x.y)).ToList();
        var wallClusters = TileCluster.GetConnectedClusters(wallTiles);

        foreach (var cluster in wallClusters)
        {
            var wrapPolygon = TilePolygon.GetWrappingPolygon(cluster, wallTilemap.cellSize.x);

            if (!IsPolygonSimple(wrapPolygon))
            {
                throw new InvalidShadowPolygonException("HEY YOU! " +
                                                        "Invalid polygon (likely due to a donut-shaped cluster). Cannot generate valid shadow." +
                                                        $"SO PLEASE FOR THE LOVE OF GOD, quit NOW (alt+F4) and look for saveData.json located in " +
                                                        $"{Application.persistentDataPath} and send it to me. THX :*");
            }
            
            var shadowCaster = Instantiate(this.shadowCasterPrefab, transform);
            shadowCaster.transform.SetParent(transform);
            shadowCaster.name = "Wall Shadow Caster";

            wrapPolygon = PolygonMargin.ApplyMargin(wrapPolygon, margin);

            ShadowCasterUtility.UpdateShadowCasterShape(shadowCaster.gameObject, wrapPolygon.ToArray());
        }

        // var floorTiles = GetAllTilePositions(floorTilemap).Select(x => new Vector2Int(x.x, x.y)).ToList();
        // var floorClusters = TileCluster.GetConnectedClusters(floorTiles);
        //
        // foreach (var cluster in floorClusters)
        // {
        //     var wrapPolygon = TilePolygon.GetWrappingPolygon(cluster, wallTilemap.cellSize.x);
        //
        //     var shadowCaster = Instantiate(this.shadowCasterPrefab, transform);
        //     shadowCaster.transform.SetParent(transform);
        //     shadowCaster.name = "Floor Shadow Caster";
        //
        //     wrapPolygon = PolygonMargin.ApplyMargin(wrapPolygon, -margin);
        //
        //     ShadowCasterUtility.UpdateShadowCasterShape(shadowCaster.gameObject, wrapPolygon.ToArray());
        //
        //     shadowCaster.selfShadows = false;
        // }
    }


    /// <summary>
    /// Returns a list of all cell positions within the given Tilemap where a tile exists.
    /// </summary>
    public List<Vector3Int> GetAllTilePositions(Tilemap tm)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        if (tm == null) return positions;

        // Get the bounding area (min and max inclusive) of all the cells in the tilemap
        BoundsInt bounds = tm.cellBounds;

        // Loop through each cell in the bounding box
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, z);

                    // Check if there is actually a tile in this position
                    TileBase tile = tm.GetTile(cellPos);
                    if (tile != null)
                    {
                        positions.Add(cellPos);
                    }
                }
            }
        }

        return positions;
    }

    //
    // private void OnMapGenerated()
    // {
    // 	return; // TODO: remove this line
    // 	
    // 	var mapData = _dungeonGenerator.MapData!;
    // 	var walls = mapData
    // 		.GetAllTilePositionsOfType(TileType.Wall)
    // 		.Select(x => new Vector2(x.X, x.Y))
    // 		.ToList();
    //
    // 	var clusters = TileCluster.GetConnectedClusters(walls);
    //
    // 	foreach (var cluster in clusters)
    // 	{
    // 		var wrapPolygon = TilePolygon.GetWrappingPolygon(cluster, mapData.TileSize);
    // 		var shadowCaster = new LightOccluder2D();
    // 		AddChild(shadowCaster);
    //
    // 		var polygon = new OccluderPolygon2D();
    // 		polygon.SetPolygon(wrapPolygon.ToArray());
    // 		polygon.CullMode = OccluderPolygon2D.CullModeEnum.Clockwise;
    // 		
    // 		shadowCaster.SetOccluderPolygon(polygon);
    // 	}
    // }
    public void SetFloorTilemap(Tilemap mapDescriptorFloorTileMap)
    {
        UpdateShadows();
    }

    public void SetWallTilemap(Tilemap mapDescriptorWallTileMap)
    {
        wallTilemap = mapDescriptorWallTileMap;
        UpdateShadows();
    }


    private bool IsPolygonSimple(List<Vector2> polygon)
    {
        int count = polygon.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2 a1 = polygon[i];
            Vector2 a2 = polygon[(i + 1) % count];

            for (int j = i + 1; j < count; j++)
            {
                // Skip adjacent edges
                if (j == (i + 1) % count || i == (j + 1) % count)
                    continue;

                Vector2 b1 = polygon[j];
                Vector2 b2 = polygon[(j + 1) % count];

                if (LinesIntersect(a1, a2, b1, b2))
                    return false;
            }
        }

        return true;
    }

    private bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (d == 0) return false;

        float u = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / d;
        float v = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / d;

        return (u >= 0 && u <= 1) && (v >= 0 && v <= 1);
    }
}