using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity.Unit.Weapon
{
	[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Match/Unit/Weapon Data")]
	public class WeaponData : ScriptableObject
	{
		[field: SerializeField] public string weaponName { get; private set; }
		[field: SerializeField] public WeaponClass weaponClass { get; private set; }
		[field: SerializeField] public int minRange { get; private set; }
		[field: SerializeField] public int maxRange { get; private set; }
		[field: SerializeField] public int maxAmmo { get; private set; }
		[field: SerializeField] public int damage { get; private set; }

		[field: SerializeReference] public List<WeaponEffect> effects { get; private set; }

		void OnEnable()
		{
			if (string.IsNullOrEmpty(weaponName))
				weaponName = name;
			if (effects == null)
				effects = new List<WeaponEffect>();
		}
	}
}