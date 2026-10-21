// Project D | Codename: Zantetsuken — BuffDefinition.cs (GCD 7.x, Appendix A)
using UnityEngine;
namespace ProjectD.Data
{
    public enum BuffApplication { KillerOnly, NearbyAllies, EntireTeam }
    [CreateAssetMenu(fileName = "Buff_", menuName = "Project D/Buff Definition")]
    public sealed class BuffDefinition : ScriptableObject
    {
        [Header("Identity")] public string buffId; public string displayName; public Sprite icon; [TextArea] public string description;
        [Header("Application Rules")] public BuffApplication application = BuffApplication.KillerOnly;
        public float durationSeconds = 90f; public bool removedOnDeath; public int priority; public bool canStack; public int maxStacks = 1;
        [Header("Flat Stat Modifiers")] public float attackDamageBonus, magicPowerBonus, adaptivePowerBonus, armorBonus, magicResistanceBonus, maxHealthBonus;
        [Header("Percent Modifiers")] public float movementSpeedPercent, attackSpeedPercent, abilityHaste;
        [Header("Sustain")] public float healthRegenPerSecond, resourceRegenPerSecond, shieldOnLowHealth;
        [Header("On-Hit Riders")] public float onHitBurnPerSecond, onHitSlowPercent, structureDamagePercent, executeThreshold, executeBonusDamage;
    }
}
