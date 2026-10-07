using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Match
{
	public class MatchBuilder : MonoBehaviour
    {
        //[SerializeField] private UnitCatalogSO unitCatalog;
        //[SerializeField] private TileCatalogSO tileCatalog;
        [SerializeField] private Transform gridParent;
        /*
        private void Start()
        {
            MatchSaveData data = MatchContext.CurrentMatchData;

            if (data == null)
            {
                Debug.LogError("No hay datos para construir la partida.");
                return;
            }

            BuildMatch(data);
        }

        public void BuildMatch(MatchSaveData data)
        {
            // Reconstruir Tablero/Tiles
            BuildBoard(data.tileMap);

            // Reconstruir Equipos y Reglas
            //SetupRulesAndTeams(data.rules, data.teams);

            // Instanciar y Configurar Unidades
            SpawnUnits(data.units);
        }

        /*
        private void BuildBoard(TileData[] tiles)
        {
            foreach (var tileData in tiles)
            {
                TileDefinitionSO def = tileCatalog.GetTile(tileData.tileTypeId);
                Vector3 worldPos = GridToWorldPosition(tileData.x, tileData.y);
                Instantiate(def.tilePrefab, worldPos, Quaternion.identity, gridParent);
            }
        }

        private void SpawnUnits(List<UnitData> unitsData)
        {
            foreach (var unitData in unitsData)
            {
                UnitDefinitionSO def = unitCatalog.GetUnit(unitData.unitTypeId);
                Vector3 worldPos = GridToWorldPosition(unitData.gridX, unitData.gridY);

                GameObject unitGO = Instantiate(def.unitPrefab, worldPos, Quaternion.identity);

                // Aplicar el estado guardado a la unidad concreta
                Unit unitComponent = unitGO.GetComponent<Unit>();
                unitComponent.InitializeFromData(unitData);
            }
        }

        private Vector3 GridToWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y); // Ejemplo básico de cuadrícula
        }
        */
    }
}