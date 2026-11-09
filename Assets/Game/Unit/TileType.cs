using UnityEngine;


[System.Serializable]
public class TileProperty
{
    public TerrainType type;
    public int movementCost = 1; // 1 por defecto en llanura, 2 en bosque, etc.
    public bool isPassableForGround = true;
    public bool allowsAttackThrough = true;
}