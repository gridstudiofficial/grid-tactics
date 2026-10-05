namespace Game.Entity
{
	public interface IHealable : IDamageable
	{
		void Heal(int health);
	}
}