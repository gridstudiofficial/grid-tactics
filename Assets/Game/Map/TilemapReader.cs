
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Lee las casillas pintadas en un Tilemap de Unity
/// y construye la representación lógica del mapa.
/// </summary>
public class TilemapReader : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;

    [Header("Tipos de tile")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase sandTile;
    [SerializeField] private TileBase mountainTile;

    private Map map;

    public Map GetMap()
    {
        return map;
    }

    public int GetPaintedTileCount()
    {
        if (tilemap == null)
        {
            Debug.LogWarning("TilemapReader: no se ha asignado un Tilemap.");
            return 0;
        }

        int count = 0;
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
                count++;
        }

        return count;
    }

    [ContextMenu("Leer mapa pintado")]
    private void ReadPaintedMap()
    {
        if (tilemap == null)
        {
            Debug.LogError("TilemapReader: asigna el Tilemap.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogError("TilemapReader: el Tilemap está vacío.");
            return;
        }

        map = new Map(bounds.size.x, bounds.size.y);

        int recognized = 0;
        int unrecognized = 0;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell))
                continue;

            TileBase paintedTile = tilemap.GetTile(cell);
            Vector2Int mapPosition = new Vector2Int(
                cell.x - bounds.xMin,
                cell.y - bounds.yMin
            );

            if (paintedTile == grassTile)
            {
                map.SetBaseType(mapPosition, TileBaseType.Ground);
                map.SetTerrain(mapPosition, TerrainType.Grass);
                recognized++;
            }
            else if (paintedTile == waterTile)
            {
                map.SetBaseType(mapPosition, TileBaseType.Water);
                map.SetTerrain(mapPosition, null);
                recognized++;
            }
            else if (paintedTile == sandTile)
            {
                map.SetBaseType(mapPosition, TileBaseType.Ground);
                map.SetTerrain(mapPosition, TerrainType.Sand);
                recognized++;
            }
            else if (paintedTile == mountainTile)
            {
                map.SetBaseType(mapPosition, TileBaseType.Ground);
                map.SetTerrain(mapPosition, TerrainType.Mountain);
                recognized++;
            }
            else
            {
                unrecognized++;
                Debug.LogWarning($"Tile sin tipo configurado en {cell}.");
            }
        }

        Debug.Log(
            $"Mapa leído: {bounds.size.x} x {bounds.size.y}. " +
            $"Tiles reconocidos: {recognized}. Sin reconocer: {unrecognized}."
        );
    }

    [ContextMenu("Probar casillas del mapa")]
    private void TestMapTiles()
    {
        if (map == null)
        {
            Debug.LogWarning("Primero ejecuta 'Leer mapa pintado'.");
            return;
        }

        Vector2Int position = new Vector2Int(0, 0);
        Tile tile = map.GetTile(position);

        Debug.Log(
            $"Casilla {position}: Base={tile.baseType}, " +
            $"Terreno={tile.terrain?.ToString() ?? "Ninguno"}"
        );
    }

    [ContextMenu("Comprobar agua y montaña")]
    private void CheckWaterAndMountain()
    {
        if (map == null)
        {
            Debug.LogWarning("Primero ejecuta 'Leer mapa pintado'.");
            return;
        }

        bool foundWater = false;
        bool foundMountain = false;

        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = map.GetTile(position);

                if (!foundWater && tile.baseType == TileBaseType.Water)
                {
                    Debug.Log(
                        $"AGUA en {position}: Base={tile.baseType}, " +
                        $"Terreno={tile.terrain?.ToString() ?? "Ninguno"}"
                    );
                    foundWater = true;
                }

                if (!foundMountain && tile.terrain == TerrainType.Mountain)
                {
                    Debug.Log(
                        $"MONTAÑA en {position}: Base={tile.baseType}, " +
                        $"Terreno={tile.terrain}"
                    );
                    foundMountain = true;
                }
            }
        }

        if (!foundWater)
            Debug.LogWarning("No se encontró ninguna casilla de agua.");

        if (!foundMountain)
            Debug.LogWarning("No se encontró ninguna casilla de montaña.");
    }


    [ContextMenu("Verificar mapa visual y lógico")]
    private void VerifyVisualAndLogicalMap()
    {
        if (map == null || tilemap == null)
        {
            Debug.LogWarning("Primero ejecuta 'Leer mapa pintado'.");
            return;
        }

        int correct = 0;
        int incorrect = 0;

        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            TileBase visualTile = tilemap.GetTile(cell);

            if (visualTile == null)
                continue;

            Vector2Int logicalPosition = new Vector2Int(
                cell.x - bounds.xMin,
                cell.y - bounds.yMin
            );

            Tile logicalTile = map.GetTile(logicalPosition);
            TerrainType? expectedTerrain = null;
            TileBaseType expectedBase = TileBaseType.Ground;
            bool recognized = true;

            if (visualTile == grassTile)
                expectedTerrain = TerrainType.Grass;
            else if (visualTile == waterTile)
                expectedBase = TileBaseType.Water;
            else if (visualTile == sandTile)
                expectedTerrain = TerrainType.Sand;
            else if (visualTile == mountainTile)
                expectedTerrain = TerrainType.Mountain;
            else
                recognized = false;

            bool matches = recognized &&
                           logicalTile.baseType == expectedBase &&
                           logicalTile.terrain == expectedTerrain;

            if (matches)
                correct++;
            else
            {
                incorrect++;
                Debug.LogError(
                    $"Diferencia en celda visual {cell}, " +
                    $"casilla lógica {logicalPosition}: " +
                    $"base={logicalTile.baseType}, " +
                    $"terreno={logicalTile.terrain}"
                );
            }
        }

        Debug.Log(
            $"Verificación terminada. Correctas: {correct}. " +
            $"Incorrectas: {incorrect}."
        );
    }

    [ContextMenu("Contar terrenos del mapa")]
    private void CountTerrainTypes()
    {
        if (map == null)
        {
            Debug.LogWarning("Primero ejecuta 'Leer mapa pintado'.");
            return;
        }

        int grassCount = 0;
        int waterCount = 0;
        int sandCount = 0;
        int mountainCount = 0;

        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                Tile tile = map.GetTile(new Vector2Int(x, y));

                if (tile.baseType == TileBaseType.Water)
                {
                    waterCount++;
                    continue;
                }

                switch (tile.terrain)
                {
                    case TerrainType.Grass:
                        grassCount++;
                        break;

                    case TerrainType.Water:
                        waterCount++;
                        break;

                    case TerrainType.Sand:
                        sandCount++;
                        break;

                    case TerrainType.Mountain:
                        mountainCount++;
                        break;
                }
            }
        }

        Debug.Log(
            $"RECUENTO DEL MAPA ({map.width} x {map.height})\n" +
            $"Hierba: {grassCount}\n" +
            $"Agua: {waterCount}\n" +
            $"Arena: {sandCount}\n" +
            $"Montaña: {mountainCount}\n" +
            $"Total: {grassCount + waterCount + sandCount + mountainCount}"
        );
    }




}
