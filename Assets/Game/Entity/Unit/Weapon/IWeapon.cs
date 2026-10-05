namespace Game.Entity.Unit.Weapon
{
	public interface IWeapon
	{
		WeaponData data { get; }
		int currentAmmo { get; }
	}
}