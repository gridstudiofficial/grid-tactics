using UnityEngine;

public interface IDamageable
{
    int currentHP { get; set; }
    int maxHP { get; set; }
    void TakeDamage(int amount /*Unit? attacker = null*/);
}
