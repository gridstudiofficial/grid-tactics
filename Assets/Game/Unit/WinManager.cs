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

    private bool gameEnded = false;

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
        if (gameEnded || activePlayers == null) return;

        foreach (var player in activePlayers)
        {
            if (player == null) continue;

            WinReason reason = EvaluatePlayerWin(player);
            if (reason != WinReason.None)
            {
                bool isHumanPlayer = AIController.Instance == null || player != AIController.Instance.aiPlayer;
                if (isHumanPlayer)
                {
                    TriggerWin(player, reason);
                }
                else
                {
                    TriggerLoss(player, reason);
                }
                break;
            }
        }
    }

    private WinReason EvaluatePlayerWin(Player player)
    {
        // CONDICIÓN 1: El bando rival no tiene más unidades desplegadas
        if (winByElimination && TurnManager.Instance != null)
        {
            var opponentUnits = TurnManager.Instance.allUnits
                .Where(u => u != null && u.owner != null && u.owner != player)
                .ToList();

            var playerUnits = TurnManager.Instance.allUnits
                .Where(u => u != null && u.owner == player)
                .ToList();

            if (playerUnits.Count > 0 && opponentUnits.Count == 0)
            {
                Debug.Log($"¡{player.Name} cumple eliminación! No quedan unidades rivales.");
                return WinReason.Elimination;
            }
        }

        // CONDICIÓN 2: Mitad + 1 de los edificios totales del mapa
        if (winByMajorityBuildings)
        {
            int totalBuildings = allBuildings.Count;
            if (totalBuildings > 0)
            {
                int requiredBuildings = (totalBuildings / 2) + 1;
                int playerOwnedBuildings = allBuildings.Count(b => b != null && b.owner == player);

                if (playerOwnedBuildings >= requiredBuildings)
                {
                    Debug.Log($"¡{player.Name} cumple mayoría de edificios ({playerOwnedBuildings}/{totalBuildings})!");
                    return WinReason.MajorityBuildings;
                }
            }
        }

        // CONDICIÓN 3: Captura del Cuartel General (HQ) rival
        if (winByHeadquartersCapture)
        {
            foreach (var hq in allHQs)
            {
                if (hq != null && hq.originalOwner != null && hq.originalOwner != player && hq.owner == player)
                {
                    Debug.Log($"¡{player.Name} ha capturado el HQ de {hq.originalOwner.Name}!");
                    return WinReason.HeadquartersCapture;
                }
            }
        }

        return WinReason.None;
    }

    private void TriggerWin(Player winner, WinReason reason)
    {
        gameEnded = true;
        Debug.Log($"<color=green>¡VICTORIA! Ganador: {winner.Name} ({reason})</color>");
        Time.timeScale = 0f;

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

    private void TriggerLoss(Player winnerOpponent, WinReason reason)
    {
        gameEnded = true;
        Debug.Log($"<color=red>¡DERROTA! Ha ganado {winnerOpponent.Name} por {reason}</color>");

        string tutorialId = reason switch
        {
            WinReason.Elimination => "tut_lose_elimination",
            WinReason.MajorityBuildings => "tut_lose_buildings",
            WinReason.HeadquartersCapture => "tut_lose_hq",
            _ => string.Empty
        };

        if (TutorialManager.Instance != null && !string.IsNullOrEmpty(tutorialId))
        {
            TutorialManager.Instance.PlayById(tutorialId, () =>
            {
                TutorialManager.Instance.GoToStartScene();
            });
        }
        else if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.GoToStartScene();
        }
    }
}