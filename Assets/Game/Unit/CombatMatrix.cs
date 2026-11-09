public enum UnitMovementType
{
    Ground,
    Water,
    Air
}

public static class CombatMatrix
{
    // Matriz de daños: [ID_Atacante][ID_Defensor] = Daño infligido
    // Si el valor es 0, la unidad defensora NO es un objetivo válido.
    private static readonly int[,] damageTable = new int[,]
    {
        // Defensor: ID 0 (Infantería) | Defensor: ID 1 (Vehículo Pesado)
        { 5, 2 }, // Atacante ID 0 (Infantería)
        { 8, 4 }  // Atacante ID 1 (Vehículo Pesado)
    };

    public static int GetDamage(int attackerID, int defenderID)
    {
        // Validar límites de la matriz
        if (attackerID >= 0 && attackerID < damageTable.GetLength(0) &&
            defenderID >= 0 && defenderID < damageTable.GetLength(1))
        {
            return damageTable[attackerID, defenderID];
        }
        return 0; // Por defecto 0 si no está contemplado en la matriz
    }

    public static bool CanAttack(int attackerID, int defenderID)
    {
        return GetDamage(attackerID, defenderID) > 0;
    }
}