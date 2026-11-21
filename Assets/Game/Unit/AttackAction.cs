using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : UnitAction
{
    public override string ActionName => "Atacar";

    public int minRange;
    public int maxRange;

    public AttackAction(int min, int max)
    {
        minRange = min;
        maxRange = max;
    }

    public override bool CanExecute(Unit unit)
    {
        // REQUISITO: Si la unidad no puede atacar después de haberse movido y ya se desplazó, no puede atacar.
        if (!unit.canMoveAndAttack && unit.hasMovedThisTurn)
        {
            return false;
        }

        return FindEnemiesInRange(unit).Count > 0;
    }

    public override void Execute(Unit unit)
    {
        MapCursor.Instance.StartAttackTargeting(this);
    }

    public List<Vector2Int> GetAttackTiles(Vector2Int center)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                int dist = Mathf.Abs(x) + Mathf.Abs(y);
                if (dist >= minRange && dist <= maxRange)
                {
                    tiles.Add(new Vector2Int(center.x + x, center.y + y));
                }
            }
        }
        return tiles;
    }

    public List<Unit> FindEnemiesInRange(Unit unit)
    {
        List<Unit> enemies = new List<Unit>();
        List<Vector2Int> attackTiles = GetAttackTiles(unit.position);

        if (TurnManager.Instance != null)
        {
            foreach (var other in TurnManager.Instance.allUnits)
            {
                // 1. Debe ser enemigo (dueño distinto)
                // 2. Debe estar en las casillas de rango de ataque
                // 3. Comprobar matriz de daño: El valor debe ser mayor a 0 para ser un objetivo válido
                if (other != unit && other.owner != unit.owner)
                {
                    if (attackTiles.Contains(other.position))
                    {
                        int calculatedDamage = CombatMatrix.GetDamage(unit.unitID, other.unitID);
                        if (calculatedDamage > 0)
                        {
                            enemies.Add(other);
                        }
                    }
                }
            }
        }
        return enemies;
    }

    public void PerformAttack(Unit attacker, Unit target)
    {
        // Obtener el daño exacto consultando la matriz 2x2

        float soundDuration = attacker.PlayAttackSound();
        int finalDamage = CombatMatrix.GetDamage(attacker.unitID, target.unitID);

        target.TakeDamage(finalDamage);
        Debug.Log($"Ataque ID {attacker.unitID} a ID {target.unitID}: {finalDamage} de daño infligido.");

        finalDamage = CombatMatrix.GetDamage(target.unitID, attacker.unitID);
        finalDamage = attacker.currentHP <= finalDamage ? attacker.currentHP - 1 : finalDamage;
        attacker.TakeDamage(finalDamage);
        if (soundDuration > 0f)
        {
            attacker.StartCoroutine(BlockWhileSoundPlays(attacker, soundDuration));
        }
        else
        {
            attacker.ExecuteWaitAction();
        }


        attacker.ExecuteWaitAction();
    }

    private IEnumerator BlockWhileSoundPlays(Unit attacker, float duration)
    {
        attacker.SetPlayingAttackSound(true);

        // WaitForSecondsRealtime asegura esperar la duración real del audio
        yield return new WaitForSecondsRealtime(duration);

        attacker.SetPlayingAttackSound(false);

        if (attacker != null)
        {
            attacker.ExecuteWaitAction();
        }
    }
}