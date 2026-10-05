using UnityEngine;

namespace Game.Entity.Unit
{
	[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Match/Unit/Weapon Data")]
	public class UnitData : ScriptableObject
	{
		[field: SerializeField] public string unitName { get; private set; }
		[field: SerializeField] public string uniDescription { get; private set; }
		[field: SerializeField] public int cost { get; private set; }
		[field: SerializeField] public int visionRange { get; private set; }
		[field: SerializeField] public int buildTime { get; private set; }
		[field: SerializeField] public UnitCategory category { get; private set; }
	}
}