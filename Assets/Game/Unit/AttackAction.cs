using UnityEngine;
using System.Collections.Generic;

public class AttackAction : UnitAction
{
    public override string ActionName => "Atacar";
    public int attackRange = 1; // Rango de ataque adyacente clásico
    public int attackDamage = 3;

    public override bool CanExecute(Unit unit)
    {
        return FindNearbyEnemies(unit).Count > 0;
    }

    public override void Execute(Unit unit)
    {
        List<Unit> enemies = FindNearbyEnemies(unit);
        if (enemies.Count > 0)
        {
            // Atacamos por defecto al primer enemigo encontrado en rango
            Unit target = enemies[0];
            target.TakeDamage(attackDamage);
            Debug.Log($"{unit.id} atacó a {target.id} causando {attackDamage} de daño.");

            // Tras atacar, la unidad termina su turno
            unit.ExecuteWaitAction();
        }
        else
        {
            Debug.Log("No hay enemigos en rango de ataque.");
            // Si no puede atacar, le devolvemos el menú o forzamos esperar
            unit.ExecuteWaitAction();
        }
    }

    private List<Unit> FindNearbyEnemies(Unit unit)
    {
        List<Unit> enemies = new List<Unit>();

        if (TurnManager.Instance != null)
        {
            foreach (var other in TurnManager.Instance.allUnits)
            {
                if (other != unit && other.owner != unit.owner)
                {
                    // Distancia Manhattan para tablero táctico
                    int dist = Mathf.Abs(unit.position.x - other.position.x) + Mathf.Abs(unit.position.y - other.position.y);
                    if (dist <= attackRange)
                    {
                        enemies.Add(other);
                    }
                }
            }
        }
        return enemies;
    }
}