using UnityEngine;

public enum WeaponClass { SmallArms, MachineGun, Cannon, Explosive, AntiAir, Torpedo, AirToAir, AirToGround }

public interface IWeapon
{
    string Name { get; }
    WeaponClass Class { get; }
    int MinRange { get; }
    int MaxRange { get; }
    int BaseDamage { get; }
    int? AmmoCost { get; } // null = infinite ammo
    bool IsUsableAgainst(IDamageable target);
}
