using UnityEngine;

public interface IHealable : IDamageable
{
    void Heal(int amount);
}
