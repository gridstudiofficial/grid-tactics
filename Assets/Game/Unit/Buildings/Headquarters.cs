using UnityEngine;
using Game.Team.Player;
using Game.Unit.Builgings;

public class Headquarters : Building
{
    [field: SerializeField] public Player originalOwner { get; private set; }

    protected override void Start()
    {
        base.Start();
        // Guardamos el dueño inicial al comenzar la partida
        originalOwner = owner;
    }

    public void UpdateOriginalOwner(Player newInitialOwner)
    {
        originalOwner = newInitialOwner;
    }
}