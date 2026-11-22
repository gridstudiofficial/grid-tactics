using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;
using UnityEngine.UIElements;

public class AIController : MonoBehaviour
{
    public static AIController Instance { get; private set; }

    [Header("Configuración de la IA")]
    public Player aiPlayer; // El jugador al que controla esta IA
    [SerializeField] private float actionDelay = 1f; // Pausa visual entre acciones para que el jugador vea los movimientos

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Método llamado por el TurnManager cuando inicia el turno de la IA
    public void StartAITurn(Player player)
    {
        if (player != aiPlayer) return;

        Debug.Log($"<color=cyan>--- TURNO DE LA IA ({aiPlayer.Name}): Iniciando Planificación ---</color>");
        StartCoroutine(ExecuteAIPlanRoutine());
    }

    private IEnumerator ExecuteAIPlanRoutine()
    {
        yield return new WaitForSeconds(actionDelay);

        // ==========================================
        // PASO 1: FABRICAR UNIDADES (Si hay fábricas libres)
        // ==========================================
        Factory[] aiFactories = FindObjectsOfType<Factory>()
            .Where(f => f.owner == aiPlayer && f.fatigue <= 0)
            .ToArray();

        foreach (var factory in aiFactories)
        {
            if (factory.producibleUnits.Count > 0 && aiPlayer.Points >= factory.producibleUnits[0].cost)
            {
                factory.SpawnUnit(factory.producibleUnits[0]);
                yield return new WaitForSeconds(actionDelay);
            }
        }

        // ==========================================
        // PASO 2: BUCLE DINÁMICO DE ACCIONES DE LA IA
        // ==========================================
        // Usamos un bucle while para obtener unidad por unidad de forma dinámica y evitar bucles dobles
        while (true)
        {
            Unit unit = TurnManager.Instance.allUnits
                .FirstOrDefault(u => u.owner == aiPlayer && !u.hasActedThisRound && u.fatigue <= 0);

            if (unit == null) break; // Si no quedan unidades activas de la IA, terminamos el turno
            if (MapCameraController.Instance != null)
            {
                MapCameraController.Instance.FocusOnUnit(unit);
            }

            Debug.Log($"IA evaluando unidad: {unit.id}");

            // 1. Intentar Atacar (Oportunista / Victoria)
            AttackAction attackAction = unit.availableActions.OfType<AttackAction>().FirstOrDefault();
            if (attackAction != null && attackAction.CanExecute(unit))
            {
                List<Unit> enemiesInRange = attackAction.FindEnemiesInRange(unit);
                if (enemiesInRange.Count > 0)
                {
                    Unit target = enemiesInRange.OrderBy(e => e.currentHP).First();
                    attackAction.PerformAttack(unit, target);
                    yield return new WaitForSeconds(actionDelay);
                    continue; // Pasa a evaluar la siguiente unidad de forma segura
                }
            }

            // 2. Intentar Conquistar si está sobre un edificio
            CaptureAction captureAction = unit.availableActions.OfType<CaptureAction>().FirstOrDefault();
            Building buildingAtPos = FindObjectsOfType<Building>().FirstOrDefault(b => b.position == unit.position);

            if (captureAction != null && buildingAtPos != null && buildingAtPos.owner != aiPlayer)
            {
                captureAction.Execute(unit);
                yield return new WaitForSeconds(actionDelay);
                continue;
            }

            // 3. Movimiento estratégico
            List<Building> targetBuildings = FindObjectsOfType<Building>().Where(b => b.owner != aiPlayer).ToList();
            List<Unit> visibleEnemies = TurnManager.Instance.allUnits.Where(u => u.owner != aiPlayer).ToList();

            Vector2Int bestDestination = FindBestMovementTarget(unit, targetBuildings, visibleEnemies);
            unit.CalculateValidMovesWithTerrain();
            List<Vector2Int> path = unit.FindPathTo(bestDestination);

            if (path.Count > 0)
            {
                foreach (Vector2Int step in path)
                {
                    Vector3 targetWorldPos = new Vector3(step.x, step.y, 0f);
                    while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, unit.moveSpeed * Time.deltaTime);
                        yield return null;
                    }
                    transform.position = targetWorldPos;
                    unit.position = step;
                }
                unit.position = path.Last();
                unit.SetPendingFatigue(10 + (path.Count * 5));
                unit.transform.position = new Vector3(unit.position.x, unit.position.y, 0f);

                Building buildingAfterMove = FindObjectsOfType<Building>().FirstOrDefault(b => b.position == unit.position);

                if (attackAction != null && attackAction.CanExecute(unit))
                {
                    List<Unit> enemiesInRange = attackAction.FindEnemiesInRange(unit);
                    if (enemiesInRange.Count > 0)
                    {
                        Unit target = enemiesInRange.OrderBy(e => e.currentHP).First();
                        attackAction.PerformAttack(unit, target);
                        yield return new WaitForSeconds(actionDelay);
                        continue; // Pasa a evaluar la siguiente unidad de forma segura
                    }
                }
                else if (captureAction != null && buildingAfterMove != null && buildingAfterMove.owner != aiPlayer)
                {
                    captureAction.Execute(unit);
                }
                else
                {
                    unit.SetPendingFatigue(10);
                    unit.ExecuteWaitAction();
                }
            }
            else
            {
                unit.SetPendingFatigue(15);
                unit.ExecuteWaitAction();
            }

            yield return new WaitForSeconds(actionDelay);
        }

        // ==========================================
        // PASO 3: FIN DEL TURNO DE LA IA
        // ==========================================
        Debug.Log($"<color=cyan>--- Fin del turno de la IA ---</color>");
        PassTurnBackToHuman();
    }

    private Vector2Int FindBestMovementTarget(Unit unit, List<Building> targetBuildings, List<Unit> visibleEnemies)
    {
        // Si hay edificios enemigos/neutrales, moverse hacia el más cercano (Victoria / Oportunista)
        if (targetBuildings.Count > 0)
        {
            Building closestBuilding = targetBuildings
                .OrderBy(b => Mathf.Abs(unit.position.x - b.position.x) + Mathf.Abs(unit.position.y - b.position.y))
                .First();
            return closestBuilding.position;
        }

        // Si no hay edificios, buscar acercarse al enemigo visible más cercano
        if (visibleEnemies.Count > 0)
        {
            Unit closestEnemy = visibleEnemies
                .OrderBy(e => Mathf.Abs(unit.position.x - e.position.x) + Mathf.Abs(unit.position.y - e.position.y))
                .First();
            return closestEnemy.position;
        }

        // Si no hay nada, quedarse quieto
        return unit.position;
    }

    private void PassTurnBackToHuman()
    {
        // Aquí buscas al jugador humano y le asignas el turno en el TurnManager
        // Ejemplo genérico:
        Player humanPlayer = FindObjectsOfType<Building>()
            .Select(b => b.owner)
            .Where(p => p != null && p != aiPlayer)
            .Distinct()
            .FirstOrDefault();

        if (humanPlayer != null && TurnManager.Instance != null)
        {
            TurnManager.Instance.activePlayer = humanPlayer;
            TurnManager.Instance.StartNextBatchOrReduce();
        }
    }
}