using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance { get; private set; }

    private List<Building> allBuildings = new List<Building>();
    private List<Headquarters> allHQs = new List<Headquarters>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Registrar todos los edificios y cuarteles generales del mapa al iniciar
        allBuildings = FindObjectsOfType<Building>().ToList();
        allHQs = FindObjectsOfType<Headquarters>().ToList();
    }

    // Método principal para evaluar si algún jugador ha ganado
    public void CheckWinConditions(List<Player> activePlayers)
    {
        foreach (var player in activePlayers)
        {
            if (HasPlayerWon(player))
            {
                TriggerWin(player);
                break;
            }
        }
    }

    private bool HasPlayerWon(Player player)
    {
        // CONDICIÓN 1: El enemigo no tiene más unidades desplegadas
        var opponentUnits = TurnManager.Instance.allUnits.Where(u => u.owner != player).ToList();
        if (TurnManager.Instance.allUnits.Count > 0 && opponentUnits.Count == 0)
        {
            Debug.Log($"¡{player.Name} gana! No quedan unidades enemigas.");
            return true;
        }

        // CONDICIÓN 2: Ha conquistado la mitad + 1 de los edificios totales del mapa
        int totalBuildings = allBuildings.Count;
        if (totalBuildings > 0)
        {
            int requiredBuildings = (totalBuildings / 2) + 1;
            int playerOwnedBuildings = allBuildings.Count(b => b.owner == player);

            if (playerOwnedBuildings >= requiredBuildings)
            {
                Debug.Log($"¡{player.Name} gana! Ha conquistado {playerOwnedBuildings} de {totalBuildings} edificios (Requerido: {requiredBuildings}).");
                return true;
            }
        }

        // CONDICIÓN 3: Ha conquistado el Cuartel General (HQ) del enemigo
        foreach (var hq in allHQs)
        {
            // Si el HQ pertenece a este jugador, pero su dueño original era otro jugador distinto
            if (hq.originalOwner != null && hq.originalOwner != player && hq.owner == player)
            {
                Debug.Log($"¡{player.Name} gana! Ha conquistado el Cuartel General de {hq.originalOwner.Name}.");
                return true;
            }
        }

        return false;
    }

    private void TriggerWin(Player winner)
    {
        Debug.Log($"<color=yellow>========================================</color>");
        Debug.Log($"<color=green> ¡FIN DE LA PARTIDA! GANADOR: {winner.Name} </color>");
        Debug.Log($"<color=yellow>========================================</color>");

        // Pausar el juego o congelar la lógica de turnos
        Time.timeScale = 0f;

        // AQUÍ PUEDES ACTIVAR TU UI DE VICTORIA:
        // VictoryUI.Instance.ShowVictoryScreen(winner);
    }
}