using UnityEngine;

namespace Game.Entity
{
	public abstract class Entity : MonoBehaviour
	{
		protected Vector2Int position { get; private set; }
	}
}
