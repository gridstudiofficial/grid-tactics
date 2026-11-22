public class Tanque : Unit
{
    protected override void Start()
    {
        base.Start();
        movementRange = 4;

        // Añade ataque a distancia (Min: 2, Max: 5, Daño: 5) y la opción de Esperar
        availableActions.Add(new AttackAction(1, 1));
        availableActions.Add(new WaitAction());
    }
}