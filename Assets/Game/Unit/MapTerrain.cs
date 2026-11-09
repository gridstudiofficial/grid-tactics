using UnityEngine;
using System.Collections.Generic;

public enum TerrainType
{
    Plains,    // Transitable para Tierra y Aire
    Water,     // Transitable solo para Agua y Aire
    Mountain   // Transitable solo para Aire (o Tierra según balance)
}

public class MapTerrain : MonoBehaviour
{
    public static MapTerrain Instance { get; set; }
    private Dictionary<Vector2Int, TerrainType> gridTerrain = new Dictionary<Vector2Int, TerrainType>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public TerrainType GetTerrainAt(Vector2Int pos)
    {
        if (gridTerrain.TryGetValue(pos, out TerrainType type)) return type;
        return TerrainType.Plains; // Por defecto Llanura (Tierra)
    }

    public void SetTerrainAt(Vector2Int pos, TerrainType type)
    {
        gridTerrain[pos] = type;
    }

    // Comprueba si un tipo de unidad puede transitar por un terreno específico
    public bool CanUnitTypePassTerrain(UnitMovementType unitType, TerrainType terrain)
    {
        switch (unitType)
        {
            case UnitMovementType.Ground:
                return terrain == TerrainType.Plains; // Tierra solo por llanuras
            case UnitMovementType.Water:
                return terrain == TerrainType.Water;  // Agua solo por casillas de agua
            case UnitMovementType.Air:
                return true; // El aire vuela sobre cualquier terreno
            default:
                return false;
        }
    }

    public int GetMovementCost(Vector2Int pos, UnitMovementType unitType)
    {
        return CanUnitTypePassTerrain(unitType, GetTerrainAt(pos)) ? 1 : 999;
    }
}