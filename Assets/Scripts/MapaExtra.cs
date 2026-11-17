
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public enum TerrainTypeExtra
{
    Plains, //Llanura
    Water, //Agua
    Mountain, //Montaña
    Sand, //Arena
    Forest,
    Grass//Bosque
}

[System.Serializable]
public struct TileTerrainDataExtra
{
    [Header("Datos del terreno")]
    public TileBase tileAsset;
    public TerrainType terrainType;

    [Header("Costes de movimiento")]
    public int costGround;
    public int costWater;
    public int costAir;
}

public class MapaExtra : MonoBehaviour
{
    public static MapaExtra Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private Tilemap tilemap;

    [Header("Generación de biomas")]
    [SerializeField] private float biomeScale = 0.08f;
    [SerializeField] private int seed = 12345;

    [Header("Base de datos de terrenos")]
    [SerializeField]
    private List<TileTerrainData> tileDatabase =
        new List<TileTerrainData>();

    private Dictionary<TileBase, TileTerrainData> tileLookup =
        new Dictionary<TileBase, TileTerrainData>();

    private Dictionary<TerrainType, TileTerrainData> terrainLookup =
        new Dictionary<TerrainType, TileTerrainData>();

    [Header("Configuración del mapa")]
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private Vector2Int mapOrigin = Vector2Int.zero;

    private void Start()
    {
        GenerateMap(mapWidth, mapHeight);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildTerrainDatabase();
    }

    // BASE DE DATOS

    private void BuildTerrainDatabase()
    {
        tileLookup.Clear();
        terrainLookup.Clear();

        foreach (var data in tileDatabase)
        {
            if (data.tileAsset != null)
            {
                if (!tileLookup.ContainsKey(data.tileAsset))
                    tileLookup.Add(data.tileAsset, data);
            }

            if (!terrainLookup.ContainsKey(data.terrainType))
                terrainLookup.Add(data.terrainType, data);
        }
    }


    // GENERACIÓN DE BIOMAS

    public TerrainType GetBiomeAt(Vector2Int coord)
    {
        float offsetX = seed * 0.0137f;
        float offsetY = seed * 0.0271f;

        float noise = Mathf.PerlinNoise(
            coord.x * biomeScale + offsetX,
            coord.y * biomeScale + offsetY
        );

        if (noise < 0.25f)
            return TerrainType.Water;

        if (noise < 0.40f)
            return TerrainType.Sand;

        if (noise > 0.70f)
            return TerrainType.Mountain;

        return TerrainType.Plains;
    }


    // GENERACIÓN DEL MAPA

    public void GenerateMap(int width, int height)
    {
        if (tilemap == null)
        {
            Debug.LogError("No hay ningún Tilemap asignado.");
            return;
        }

        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int coord = new Vector2Int(
                    mapOrigin.x + x,
                    mapOrigin.y + y
                );

                TerrainType terrain = GetBiomeAt(coord);

                if (terrainLookup.TryGetValue(terrain, out var data))
                {
                    tilemap.SetTile(
                        new Vector3Int(coord.x, coord.y, 0),
                        data.tileAsset
                    );
                }
            }
        }
    }


    // CONSULTA DE TERRENO

    public TerrainType GetTerrainAt(Vector2Int pos)
    {
        if (tilemap == null)
            return TerrainType.Plains;

        TileBase tile = tilemap.GetTile(new Vector3Int(pos.x, pos.y, 0));

        if (tile != null && tileLookup.TryGetValue(tile, out var data))
        {
            return data.terrainType;
        }

        return TerrainType.Plains;
    }

    // MOVIMIENTO DE UNIDADES


    public bool CanUnitTypePassTerrain(UnitMovementType unitMovementType, TerrainType terrain)
    {
        int cost = GetMovementCostForType(unitMovementType, terrain);

        return cost < 999;
    }

    public int GetMovementCost(Vector2Int pos, UnitMovementType unitMovementType)
    {
        TerrainType terrain = GetTerrainAt(pos);
        return GetMovementCostForType(unitMovementType, terrain);
    }

    private int GetMovementCostForType(UnitMovementType unitType, TerrainType terrain)
    {
        if (terrainLookup.TryGetValue(terrain, out var data))
        {
            switch (unitType)
            {
                case UnitMovementType.Ground:
                    return data.costGround;

                case UnitMovementType.Water:
                    return data.costWater;

                case UnitMovementType.Air:
                    return data.costAir;
            }
        }

        return 1;
    }
}

