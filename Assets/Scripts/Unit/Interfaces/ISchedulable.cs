using UnityEngine;

public interface ISchedulable
{
    string id { get; }
    Player Owner { get; }
    int fatigue { get; }
}
