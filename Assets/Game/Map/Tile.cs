
using UnityEngine;

/// <summary>
/// Tipos de base de una casilla.
/// </summary>
public enum TileBaseType
{
    Ground,
    Water
}

/// <summary>
/// Tipos de terreno que puede tener una casilla.
/// </summary>
public enum TerrainType
{
    Grass,
    Water,
    Sand,
    Mountain
}

/// <summary>
/// Representa una casilla del mapa.
/// </summary>
public class Tile
{
    public Vector2Int position { get; }

    public TileBaseType baseType { get; private set; }

    public TerrainType? terrain { get; set; }

    public Tile(
        Vector2Int position,
        TileBaseType baseType = TileBaseType.Ground,
        TerrainType? terrain = null)
    {
        this.position = position;
        this.baseType = baseType;
        this.terrain = terrain;
    }

    public void SetBaseType(TileBaseType baseType)
    {
        this.baseType = baseType;
    }
}
