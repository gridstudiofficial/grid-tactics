using UnityEngine;
using System.Collections.Generic;
using Game.Team.Player;
using Game.Unit.Builgings;
[System.Serializable]
public struct BuildableUnitData
{
    public string unitName;
    public GameObject unitPrefab;
    public int cost;
}

public class Factory : Building
{
    [Header("Unidades Producibles")]
    public List<BuildableUnitData> availableUnitsToBuild = new List<BuildableUnitData>();

    // Método para instanciar la unidad seleccionada
    public Unit SpawnUnit(BuildableUnitData unitData)
    {
        if (owner == null) return null;

        if (owner.Points >= unitData.cost)
        {
            owner.Points -= unitData.cost;
            Debug.Log($"{owner.Name} gastó {unitData.cost} puntos. Puntos restantes: {owner.Points}");

            // Instanciar en la posición de la fábrica
            GameObject unitObj = Instantiate(unitData.unitPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);
            Unit newUnit = unitObj.GetComponent<Unit>();

            if (newUnit != null)
            {
                newUnit.position = position;
                newUnit.owner = owner;

                // REQUISITO: Nacen fatigadas y no podrán usarse en este turno
                newUnit.fatigue = 50;
                newUnit.hasActedThisRound = true;
                newUnit.SetExhaustedState(true);

                // Registrar en el TurnManager
                if (TurnManager.Instance != null)
                {
                    TurnManager.Instance.RegisterUnit(newUnit);
                }

                Debug.Log($"Unidad {unitData.unitName} desplegada en {position} (Fatigada).");
                return newUnit;
            }
        }
        else
        {
            Debug.Log("Puntos insuficientes para fabricar esta unidad.");
        }
        return null;
    }
}