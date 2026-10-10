using UnityEngine;
using Game.Team.Player;
public interface ISchedulable
{
    string id { get; }
    Player owner { get; }
    int fatigue { get; }
}
