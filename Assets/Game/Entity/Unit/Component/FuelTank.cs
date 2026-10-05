using System;

namespace Game.Entity.Unit.Component
{
	public interface IFuelDepletionPolicy
	{
		void OnFuelDepleted(Unit unit);
	}

	public sealed class DestroyOnEmptyFuel : IFuelDepletionPolicy
	{
		public void OnFuelDepleted(Unit unit)
		{
			unit.TakeDamage(unit.currentHealth); // unit "crash"
			//state.Events.Publish(new UnitFuelDepleted(unit));
		}
	}

	public sealed class ImmobilizeOnEmptyFuel : IFuelDepletionPolicy
	{
		public void OnFuelDepleted(Unit unit)
		{
		}
	}

	public sealed class FuelTank : IUnitComponent
	{
		public int capacity { get; }
		public int current { get; private set; }
		public bool consumesPerTurn { get; }
		public IFuelDepletionPolicy depletionPolicy { get; }

		public FuelTank(int capacity, bool consumesPerTurn, IFuelDepletionPolicy? depletionPolicy = null)
		{
			capacity = capacity;
			current = capacity;
			consumesPerTurn = consumesPerTurn;
			depletionPolicy = depletionPolicy ?? new ImmobilizeOnEmptyFuel(); // por defecto: terrestres/navales
		}

		public bool Consume(int amount)
		{
			if (current < amount) return false;
			current -= amount;
			return true;
		}

		public void Refill(int amount) => current = Math.Min(capacity, current + amount);
	}
}