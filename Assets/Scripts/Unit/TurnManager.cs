using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Cola de Unidades")]
    public List<Unit> allUnits = new List<Unit>();
    public Unit currentActiveUnit;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ejemplo: Empezar el combate al cargar la escena
        // En tu juego real, llamarás a esto tras colocar las fichas
        CalculateNextTurn();
    }

    public void RegisterUnit(Unit unit)
    {
        if (!allUnits.Contains(unit))
            allUnits.Add(unit);
    }

    public void CalculateNextTurn()
    {
        if (allUnits.Count == 0) return;

        // 1. Reducir fatiga hasta que al menos una unidad llegue a 0
        while (!allUnits.Any(u => u.fatigue <= 0))
        {
            foreach (var unit in allUnits)
            {
                unit.ReduceFatigue(1); // Reduce 1 punto por "tick" de tiempo
            }
        }

        allUnits = allUnits.OrderBy(u => u.fatigue).ToList();
        currentActiveUnit = allUnits.First(u => u.fatigue <= 0);

        // 3. Ceder el control a la unidad (y a su jugador)
        Debug.Log($"Turno de {currentActiveUnit.name}. Coste de fatiga actual: {currentActiveUnit.fatigue}");
        currentActiveUnit.StartTurn();
    }

    // Se llama cuando la unidad termina de moverse o atacar
    public void OnUnitActionCompleted(Unit unit, int actionFatigueCost)
    {
        unit.AddFatigue(actionFatigueCost);
        currentActiveUnit = null;

        // Volver a calcular quién sigue en la cola
        CalculateNextTurn();
    }
}