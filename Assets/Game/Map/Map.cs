
using System;
using UnityEngine;

/// <summary>
/// Representa una cuadrícula rectangular de casillas.
/// </summary>
public class Map
{
    public int width { get; }
    public int height { get; }

    private readonly Tile[,] tiles;

    public Map(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        this.width = width;
        this.height = height;

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(new Vector2Int(x, y));
            }
        }
    }

    public bool IsInside(Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < width &&
               position.y >= 0 &&
               position.y < height;
    }

    public Tile GetTile(Vector2Int position)
    {
        if (!IsInside(position))
            throw new ArgumentOutOfRangeException(nameof(position));

        return tiles[position.x, position.y];
    }

    public void SetTerrain(Vector2Int position, TerrainType terrain)
    {
        GetTile(position).terrain = terrain;
    }

    public TerrainType GetTerrain(Vector2Int position)
    {
        return GetTile(position).terrain;
    }

}
