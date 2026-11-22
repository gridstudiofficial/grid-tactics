using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct BuildableUnitData
{
    public string unitName;
    public GameObject unitPrefab;
    public int cost;
}

public class Factory : Building, ISchedulable
{
    [Header("Unidades Producibles")]
    public List<BuildableUnitData> producibleUnits = new List<BuildableUnitData>();

    [Header("Fatigue / Cooldown")]
    [field: SerializeField] public int fatigue { get; set; } = 0;
    public string id => $"Factory_{position.x}_{position.y}";

    [Header("Visuales de Fatiga")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fatiguedColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Tinte oscuro cuando está en cooldown

    protected override void Start()
    {
        base.Start();
        UpdateVisuals();
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterFactory(this);
        }
    }

    public void ReduceFatigue(int amount)
    {
        fatigue = Mathf.Max(0, fatigue - amount);
        UpdateVisuals();
    }

    protected override void UpdateVisuals()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (owner != null)
        {
            spriteRenderer.color = owner.GetColor(fatigue > 0);
        }
        else
        {
            spriteRenderer.color = neutralColor;
        }
    }

    public Unit SpawnUnit(BuildableUnitData unitData)
    {
        if (owner == null) return null;

        // BLOQUEO ESTRICTO: Si está fatigada, no permite fabricar ni continuar
        if (fatigue > 0)
        {
            Debug.Log($"La fábrica está fatigada. Turnos restantes: {fatigue}");
            return null;
        }

        if (owner.Points >= unitData.cost)
        {
            // Comprobar si la casilla está ocupada por otra unidad
            bool isOccupied = TurnManager.Instance.allUnits.Any(u => u.position == position);
            if (isOccupied)
            {
                Debug.Log("No se puede spawnear: la casilla está ocupada por una unidad.");
                return null;
            }

            owner.Points -= unitData.cost;

            GameObject unitObj = Instantiate(unitData.unitPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);
            Unit newUnit = unitObj.GetComponent<Unit>();

            if (newUnit != null)
            {
                newUnit.position = position;
                newUnit.owner = owner;
                newUnit.fatigue = 50;
                newUnit.hasActedThisRound = true;
                newUnit.SetExhaustedState(true);
                newUnit.RefreshOwnerColor();

                if (TurnManager.Instance != null)
                {
                    TurnManager.Instance.RegisterUnit(newUnit);
                }

                // Aplicar fatiga/cooldown a la fábrica (ej. 30 turnos/unidades de fatiga)
                fatigue = 30;
                UpdateVisuals(); // Oscurecer inmediatamente al fabricar

                Debug.Log($"Unidad creada en {position}. Fábrica en cooldown.");
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