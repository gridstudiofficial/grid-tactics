public class WaitAction : UnitAction
{
    public override string ActionName => "Esperar";

    public override bool CanExecute(Unit unit)
    {
        return true; // Siempre se puede esperar
    }

    public override void Execute(Unit unit)
    {
        unit.ExecuteWaitAction(); // Finaliza el turno y pasa a la siguiente ficha
    }
}