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

    // Añade esta lista y método en tu TurnManager:
    public List<Factory> allFactories = new List<Factory>();

    public List<City> allCities = new List<City>();

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
    public void RegisterCity(City city)
    {
        if (!allCities.Contains(city))
        {
            allCities.Add(city);
        }
    }

    public void RegisterFactory(Factory factory)
    {
        if (!allFactories.Contains(factory)) { allFactories.Add(factory); OnActiveUnitChanged?.Invoke(null); }
    }

    public void RegisterUnit(Unit unit)
    {
        if (!allUnits.Contains(unit))
            allUnits.Add(unit);
    }

    public void StartNextBatchOrReduce()
    {
        activeUnits = allUnits.Where(u => u.fatigue <= 0 && !u.hasActedThisRound).ToList();

        if (activeUnits.Count == 0)
        {
            anyUnitReady = false;
            while (!anyUnitReady)
            {
                foreach (var unit in allUnits)
                {
                    unit.ReduceFatigue(1);
                }
                foreach (var factory in allFactories) // <-- Reducir fatiga de las fábricas
                {
                    factory.ReduceFatigue(1);
                }
                foreach (var city in allCities)
                {
                    if (city != null)
                    {
                        city.ReduceFatigue(1);
                    }
                }
            }

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

            // COMPROBACIÓN DE IA: Si el jugador activo es la IA, delegamos el turno al AIController
            if (AIController.Instance != null && AIController.Instance.aiPlayer == activePlayer)
            {
                AIController.Instance.StartAITurn(activePlayer);
                return; // Detenemos la selección manual de unidades humanas
            }

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