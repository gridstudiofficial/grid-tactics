public class Infantry : Unit
{
    protected override void Start()
    {
        base.Start();
        movementRange = 3;
        unitID = 0;

        // Añade ataque cuerpo a cuerpo (Min: 1, Max: 1, Daño: 3) y la opción de Esperar
        availableActions.Add(new AttackAction(1, 1));
        availableActions.Add(new CaptureAction(true));
        availableActions.Add(new WaitAction());
    }
}