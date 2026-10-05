using System;
using System.Collections.Generic;
using Game.Entity.Unit.Movement;
using Game.Entity.Unit.Weapon;
using Game.Team.Player;
using UnityEngine;

namespace Game.Entity.Unit
{
	public class Unit : Entity, IHealable, ISchedulable
	{
		public string id { get; private set; }
		public Player owner { get; private set; }
		public int fatigue { get; private set; }
		public int currentHealth { get; private set; }
		public int maxHealth { get; private set; }
		public UnitData data { get; private set; }

		public IMovementProfile movement { get; }
		[field: SerializeReference] public List<IWeapon> weapons { get; private set; }
		//private readonly List<IUnitComponent> components;

		public Unit(string id, Player owner, int maxHealth, int fatigue = 0)
		{
			if (string.IsNullOrWhiteSpace(id))
				throw new ArgumentException("A unit must have an id.", nameof(id));
			if (maxHealth <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be greater than zero.");
			if (fatigue < 0)
				throw new ArgumentOutOfRangeException(nameof(fatigue), "Fatigue cannot be negative.");

			this.id = id;
			this.owner = owner;
			this.maxHealth = maxHealth;
			currentHealth = maxHealth;
			this.fatigue = fatigue;
		}

		public void TakeDamage(int damage, Unit? attacker = null)
		{
			if (damage <= 0 || currentHealth <= 0)
				return;

			currentHealth = Math.Max(0, currentHealth - damage);
		}

		public void Heal(int health)
		{
			if (health <= 0 || currentHealth <= 0)
				return;

			currentHealth = (int)Math.Min(maxHealth, (long)currentHealth + health);
		}
	}
}
