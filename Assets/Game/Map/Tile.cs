
using UnityEngine;

/// <summary>
/// Tipos de terreno provisionales para las casillas del mapa.
/// </summary>
public enum TerrainType
{
    Grass,
    Water,
    Sand,
    Mountain
}

/// <summary>
/// Representa una casilla del mapa de Grid Tactics.
/// </summary>
public class Tile
{
    public Vector2Int position { get; }

    public TerrainType terrain { get; set; }

    public Tile(Vector2Int position, TerrainType terrain = TerrainType.Grass)
    {
        this.position = position;
        this.terrain = terrain;
    }
}
