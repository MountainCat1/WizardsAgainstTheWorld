using System;
using Services.MapGenerators;

public interface IMapGenerator
{
    public event Action MapGenerated;
    public event Action MapGeneratedLate;
    public MapData MapData { get; }
    GenerateMapSettings Settings { get; set; }

    public void GenerateMap();
    public void SafeGenerateMap(int? seed = null);
}

public enum TileType
{
    Empty = 0,
    Floor = 1,
    Wall = 2,
}