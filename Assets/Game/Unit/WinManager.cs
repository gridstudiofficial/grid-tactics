using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;

public enum WinReason
{
    None,
    Elimination,
    MajorityBuildings,
    HeadquartersCapture
}

public class WinManager : MonoBehaviour
{
    public static WinManager Instance { get; private set; }

    [Header("Condiciones de Victoria Activas en este Nivel")]
    public bool winByElimination = true;
    public bool winByMajorityBuildings = true;
    public bool winByHeadquartersCapture = true;

    public List<Player> activePlayers;

    private List<Building> allBuildings = new List<Building>();
    private List<Headquarters> allHQs = new List<Headquarters>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshMapReferences();
    }

    public void RefreshMapReferences()
    {
        allBuildings = FindObjectsOfType<Building>().ToList();
        allHQs = FindObjectsOfType<Headquarters>().ToList();
    }

    public void ConfigureWinConditions(bool elimination, bool majorityBuildings, bool hqCapture)
    {
        winByElimination = elimination;
        winByMajorityBuildings = majorityBuildings;
        winByHeadquartersCapture = hqCapture;
    }

    public void CheckWinConditions()
    {
        if (activePlayers == null) return;

        foreach (var player in activePlayers)
        {
            if (player == null) continue;

            WinReason reason = EvaluatePlayerWin(player);
            if (reason != WinReason.None)
            {
                TriggerWin(player, reason);
                break;
            }
        }
    }

    private WinReason EvaluatePlayerWin(Player player)
    {
        // CONDICIÓN 1: El enemigo no tiene más unidades desplegadas
        if (winByElimination && TurnManager.Instance != null)
        {
            var opponentUnits = TurnManager.Instance.allUnits.Where(u => u != null && u.owner != player).ToList();
            if (TurnManager.Instance.allUnits.Count > 0 && opponentUnits.Count == 0)
            {
                Debug.Log($"¡{player.Name} gana! No quedan unidades enemigas.");
                return WinReason.Elimination;
            }
        }

        // CONDICIÓN 2: Ha conquistado la mitad + 1 de los edificios totales del mapa
        if (winByMajorityBuildings)
        {
            int totalBuildings = allBuildings.Count;
            if (totalBuildings > 0)
            {
                int requiredBuildings = (totalBuildings / 2) + 1;
                int playerOwnedBuildings = allBuildings.Count(b => b != null && b.owner == player);

                if (playerOwnedBuildings >= requiredBuildings)
                {
                    Debug.Log($"¡{player.Name} gana! Ha conquistado {playerOwnedBuildings} de {totalBuildings} edificios.");
                    return WinReason.MajorityBuildings;
                }
            }
        }

        // CONDICIÓN 3: Ha conquistado el Cuartel General (HQ) del enemigo
        if (winByHeadquartersCapture)
        {
            foreach (var hq in allHQs)
            {
                if (hq != null && hq.originalOwner != null && hq.originalOwner != player && hq.owner == player)
                {
                    Debug.Log($"¡{player.Name} gana! Ha conquistado el Cuartel General de {hq.originalOwner.Name}.");
                    return WinReason.HeadquartersCapture;
                }
            }
        }

        return WinReason.None;
    }

    private void TriggerWin(Player winner, WinReason reason)
    {
        Debug.Log($"<color=yellow>========================================</color>");
        Debug.Log($"<color=green> ¡FIN DE LA PARTIDA! GANADOR: {winner.Name} ({reason}) </color>");
        Debug.Log($"<color=yellow>========================================</color>");

        string tutorialId = reason switch
        {
            WinReason.Elimination => "tut_win_elimination",
            WinReason.MajorityBuildings => "tut_win_buildings",
            WinReason.HeadquartersCapture => "tut_win_hq",
            _ => string.Empty
        };

        if (TutorialManager.Instance != null && !string.IsNullOrEmpty(tutorialId))
        {
            TutorialManager.Instance.PlayById(tutorialId, () =>
            {
                Time.timeScale = 0f;
            });
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
}