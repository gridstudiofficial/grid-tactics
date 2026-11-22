using System.Collections.Generic;
using System.Linq;
using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;

public class CaptureAction : UnitAction
{
    public override string ActionName => "Conquistar";
    private bool canCaptureUnitType;

    public CaptureAction(bool canCapture)
    {
        this.canCaptureUnitType = canCapture;
    }

    public override bool CanExecute(Unit unit)
    {
        if (!canCaptureUnitType) return false;

        Building building = FindBuildingAt(unit.position);
        return building != null && building.owner != unit.owner;
    }

    public override void Execute(Unit unit)
    {
        Building building = FindBuildingAt(unit.position);
        if (building != null)
        {
            bool isHumanPlayer = AIController.Instance == null || unit.owner != AIController.Instance.aiPlayer;

            if (building is City city)
            {
                city.OnCaptured(unit.owner);

                // Tutorial intermedio: Primera ciudad capturada por el jugador
                if (isHumanPlayer && TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.PlayById("Tuto_primera_ciudad");
                }
            }
            else
            {
                building.SetOwner(unit.owner);
                //Debug.Log($"¡La unidad {unit.id} conquistó el edificio en {unit.position} para {unit.owner.Name}!");

                // Tutorial intermedio: Primera fábrica capturada por el jugador
                if (isHumanPlayer && building is Factory && TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.PlayById("tuto_primera_fabrica");
                }
            }

            List<Player> players = Object.FindObjectsOfType<Building>()
                .Select(b => b.owner)
                .Where(o => o != null)
                .Distinct()
                .ToList();

            if (WinManager.Instance != null)
            {
                WinManager.Instance.CheckWinConditions();
            }
        }
        unit.ExecuteWaitAction();
    }

    private Building FindBuildingAt(Vector2Int pos)
    {
        Building[] buildings = Object.FindObjectsOfType<Building>();
        foreach (var b in buildings)
        {
            if (b.position == pos) return b;
        }
        return null;
    }
}