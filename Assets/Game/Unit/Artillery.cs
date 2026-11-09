public class Artillery : Unit
{
    protected override void Start()
    {
        base.Start();
        movementRange = 2;

        // Añade ataque a distancia (Min: 2, Max: 5, Daño: 5) y la opción de Esperar
        availableActions.Add(new AttackAction(2, 5));
        availableActions.Add(new WaitAction());
    }
}