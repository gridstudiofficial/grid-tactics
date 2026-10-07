using UnityEngine;

public class Infantry : Unit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start(); // Ejecuta la inicialización visual de la clase Unit

        // Atributos específicos de Infantería
        maxHP = 10;
        currentHP = maxHP;
        movementRange = 3;
    }

}
