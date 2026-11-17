using UnityEngine;
using Game.Unit.Builgings;
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
        // Puede conquistar si hay un edificio y su dueño es diferente (o neutro)
        return building != null && building.owner != unit.owner;
    }

    public override void Execute(Unit unit)
    {
        Building building = FindBuildingAt(unit.position);
        if (building != null)
        {
            building.SetOwner(unit.owner);
            Debug.Log($"¡La unidad {unit.id} conquistó el edificio en {unit.position} para el jugador {unit.owner.Name}!");
        }
        // Conquistar gasta el turno de la acción
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