using Game.Team.Player;
using Game.Unit.Builgings;
using UnityEngine;
using UnityEngine.UIElements;

public class City : Building, ISchedulable
{
    [Header("Economía de la Ciudad")]
    [SerializeField] private int captureBonusPoints = 250; // Pequeña cantidad al conquistar
    [SerializeField] private int fullIncomeValue = 1000;   // Valor completo cada X fatiga

    [Header("Programación / Fatiga (ISchedulable)")]
    [SerializeField] private int incomeCycleFatigue = 30;  // Cantidad de fatiga requerida para dar el valor completo
    [field: SerializeField] public int fatigue { get; set; } = 0;

    public string id => $"City_{position.x}_{position.y}";

    protected override void Start()
    {
        base.Start();

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterCity(this);
        }

        // Si la ciudad empieza con dueño desde el inicio de la partida, arrancamos su ciclo de fatiga
        if (owner != null)
        {
            fatigue = incomeCycleFatigue;
        }
    }

    // Llamado cuando una unidad conquista esta ciudad
    public void OnCaptured(Player newOwner)
    {
        if (newOwner == null) return;

        SetOwner(newOwner);

        // 1. Otorgar pequeña cantidad inicial al conquistar
        newOwner.Points += captureBonusPoints;
        Debug.Log($"¡Ciudad {id} conquistada por {newOwner.Name}! Recibe +{captureBonusPoints} pts iniciales (Total: {newOwner.Points}).");

        // 2. Iniciar la fatiga para que tras X turnos/fatiga otorgue el valor completo
        fatigue = incomeCycleFatigue;
        UpdateVisuals();
    }

    // Implementación de ISchedulable
    public void ReduceFatigue(int amount)
    {
        // Solo acumula/avanza ciclo si la ciudad pertenece a un jugador
        if (owner == null)
        {
            fatigue = 0;
            return;
        }

        fatigue -= amount;

        // Cuando se cumple el ciclo de fatiga, otorga el valor completo y reinicia el contador
        if (fatigue <= 0)
        {
            GrantFullIncome();
            fatigue = incomeCycleFatigue;
        }
    }

    private void GrantFullIncome()
    {
        if (owner != null)
        {
            owner.Points += fullIncomeValue;
            Debug.Log($"Ciudad {id} completó su ciclo de fatiga y otorgó +{fullIncomeValue} pts a {owner.Name} (Total: {owner.Points}).");
        }
    }
}