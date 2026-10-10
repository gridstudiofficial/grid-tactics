using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Cola de Unidades")]
    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> activeUnits = new List<Unit>();
    public bool anyUnitReady = false;


    [Header("Estado Actual de Turnos")]
    public Player activePlayer;
    public Unit currentActiveUnit;

    public System.Action<Unit> OnActiveUnitChanged;
    public System.Action<List<Unit>> OnQueueUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ejemplo: Empezar el combate al cargar la escena
        // En tu juego real, llamarás a esto tras colocar las fichas
        //StartNextBatchOrReduce();
    }

    public void RegisterUnit(Unit unit)
    {
        if (!allUnits.Contains(unit))
            allUnits.Add(unit);
    }

    public void StartNextBatchOrReduce()
    {
        activeUnits  = allUnits.Where(u => u.fatigue <= 0 && !u.hasActedThisRound).ToList();

        if (activeUnits.Count == 0)
        {
            anyUnitReady = false;
            // Nadie tiene fatiga 0. Reducir fatiga globalmente hasta que aparezca el próximo lote
            while (!anyUnitReady)
            {
                foreach (var unit in allUnits)
                {
                    unit.ReduceFatigue(1);
                }
            }

            // Reiniciar el estado de acción para el nuevo ciclo
            foreach (var unit in allUnits)
            {
                unit.hasActedThisRound = false;
            }

            activeUnits = allUnits.Where(u => u.fatigue <= 0 && !u.hasActedThisRound).ToList();
        }
        SelectNextUnit();
    }


    public void SelectNextUnit()
    {
        
        if (activeUnits.Count > 0)
        {
            activePlayer = activeUnits.First().owner;
            currentActiveUnit = activeUnits.First();
            currentActiveUnit.StartTurn();

            OnActiveUnitChanged?.Invoke(currentActiveUnit);
            UpdateUIQueue();
        }
        else
        {
            currentActiveUnit = null;
            OnActiveUnitChanged?.Invoke(null);
            StartNextBatchOrReduce();
        }
    }



    // Se llama cuando la unidad termina de moverse o atacar
    public void CompleteUnitAction(Unit unit, int fatigueCost)
    {
        unit.AddFatigue(fatigueCost);
        unit.hasActedThisRound = true;
        currentActiveUnit = null;
        activeUnits.RemoveAt(0);
        SelectNextUnit();
    }

    public List<Unit> GetUpcomingQueue()
    {
        // Ordenar la cola de la UI por menor fatiga
        return allUnits.OrderBy(u => u.fatigue).ToList();
    }

    private void UpdateUIQueue()
    {
        OnQueueUpdated?.Invoke(GetUpcomingQueue());
    }

    public void unitReady()
    {
        this.anyUnitReady = true;
    }
}