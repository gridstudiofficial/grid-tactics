using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public enum TerrainType
{
    Plains,    // Llanura / Hierba
    Water,     // Agua
    Mountain,  // Montaña
    Sand,      // Arena
    Forest,     // Bosque
    Grass
}

[System.Serializable]
public struct TileTerrainData
{
    public TileBase tileAsset;       // Arrastra aquí tu Grass.asset, Water.asset, etc.
    public TerrainType terrainType;  // Tipo de terreno lógico

    [Header("Costes de Movimiento por Tipo de Unidad")]
    public int costGround;           // 999 = Impasable
    public int costWater;
    public int costAir;
}

public class MapTerrain : MonoBehaviour
{
    public static MapTerrain Instance { get; private set; }

    [Header("Referencia al Tilemap de la Escena")]
    [SerializeField] private Tilemap tilemap;

    [Header("Base de Datos de Terrenos")]
    [SerializeField] private List<TileTerrainData> tileDatabase = new List<TileTerrainData>();

    private Dictionary<TileBase, TileTerrainData> tileLookup = new Dictionary<TileBase, TileTerrainData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Construir un diccionario rápido para optimizar la búsqueda en tiempo de ejecución
        foreach (var data in tileDatabase)
        {
            if (data.tileAsset != null && !tileLookup.ContainsKey(data.tileAsset))
            {
                tileLookup[data.tileAsset] = data;
            }
        }
    }

    // Obtiene el tipo de terreno analizando la celda actual del Tilemap
    public TerrainType GetTerrainAt(Vector2Int pos)
    {
        if (tilemap == null) return TerrainType.Plains;

        TileBase tile = tilemap.GetTile(new Vector3Int(pos.x, pos.y, 0));
        if (tile != null && tileLookup.TryGetValue(tile, out var data))
        {
            return data.terrainType;
        }
        return TerrainType.Plains; // Terreno por defecto si la casilla está vacía
    }

    // Comprueba si una unidad puede transitar por el terreno según su movilidad
    public bool CanUnitTypePassTerrain(UnitMovementType unitMovementType, TerrainType terrain)
    {
        int cost = GetMovementCostForType(unitMovementType, terrain);
        return cost < 999; // Si el coste es menor a 999, es transitable
    }

    public int GetMovementCost(Vector2Int pos, UnitMovementType unitMovementType)
    {
        TerrainType terrain = GetTerrainAt(pos);
        return GetMovementCostForType(unitMovementType, terrain);
    }

    private int GetMovementCostForType(UnitMovementType unitType, TerrainType terrain)
    {
        if (tilemap == null) return 1;

        // Buscar en la base de datos el tile correspondiente a esta posición
        // (O mapear directamente por tipo de terreno)
        foreach (var data in tileDatabase)
        {
            if (data.terrainType == terrain)
            {
                switch (unitType)
                {
                    case UnitMovementType.Ground: return data.costGround;
                    case UnitMovementType.Water: return data.costWater;
                    case UnitMovementType.Air: return data.costAir;
                }
            }
        }
        return 1; // Coste por defecto
    }
}