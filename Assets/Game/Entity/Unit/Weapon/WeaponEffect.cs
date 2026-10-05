using System;

namespace Game.Entity.Unit.Weapon
{
	[Serializable]
	public abstract class WeaponEffect
	{
		public abstract void Execute(Unit origin, Unit target, IWeapon weapon);
	}

	[Serializable]
	public class DealDamage : WeaponEffect
	{
		public override void Execute(Unit origin, Unit target, IWeapon weapon)
		{
			target.TakeDamage(weapon.data.damage);
		}
	}
}