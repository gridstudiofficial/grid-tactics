namespace Game.Entity
{
	public interface IDamageable
	{
		int currentHealth { get; }
		int maxHealth { get; }
		void TakeDamage(int damage, Unit.Unit attacker);
	}
}