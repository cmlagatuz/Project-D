// Project D | Codename: Zantetsuken — IDamageable.cs
using ProjectD.Core;
namespace ProjectD.Gameplay.Combat
{
    public struct DamageInstance { public float Amount; public DamageType Type; public int SourceEntityId; public int SourceTeam; public bool FromStructure; public float ExecuteThreshold; public float ExecuteBonus; }
    public interface IDamageable
    {
        int EntityId { get; } Team Team { get; } bool IsAlive { get; } float CurrentHealth { get; } float MaxHealth { get; }
        float ReceiveDamage(in DamageInstance dmg);
    }
}
