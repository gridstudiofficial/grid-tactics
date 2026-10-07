using Game.Team;
using Game.Team.Player;
using UnityEngine;

namespace Game.Entity.Unit
{
	public class Unit : MonoBehaviour, ISchedulable
	{
		[SerializeField] private string id;
		[SerializeField] private Player owner;
		[SerializeField] private int fatigue;

		public string Id => id;
		public IOwner Owner => owner;
		public int Fatigue => fatigue;
	}
}