using UnityEngine;
using Game.Unit.Builgings;
public class City : Building
{
    [SerializeField] private int incomeValue = 1000; // Puntos que otorga al dueño

    public void ProvideIncome()
    {
        if (owner != null)
        {
            owner.Points += incomeValue;
            Debug.Log($"Ciudad en {position} otorgó {incomeValue} puntos a {owner.Name}. Total actual: {owner.Points}");
        }
    }
}