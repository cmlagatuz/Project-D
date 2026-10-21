// Project D | Codename: Zantetsuken — NeutralMonsterDefinition.cs (GCD 7.x/8.x)
using UnityEngine;
namespace ProjectD.Data
{
    public enum NeutralTier { Small, Medium, Large, ElementalSummon, ElderSummon, DivineManifestation }
    [CreateAssetMenu(fileName = "Monster_", menuName = "Project D/Neutral Monster Definition")]
    public sealed class NeutralMonsterDefinition : ScriptableObject
    {
        [Header("Identity")] public string monsterId; public string displayName; public NeutralTier tier; public GameObject prefab; [TextArea] public string signatureMechanic;
        [Header("Base Stats")] public float maxHealth = 1200f, attackDamage = 40f, armor = 20f, magicResist = 20f, moveSpeed = 2.5f, attackRange = 1.5f, attacksPerSecond = 0.6f;
        [Header("Scaling")] public float healthGrowthPerMinute, damageGrowthPerMinute, resistGrowthPerMinute;
        [Header("Camp")] public int escortCount; public float respawnSeconds = 90f; public float leashRange = 8f;
        [Header("Rewards")] public int killerGold = 60; public int teamGold; public int experience = 90; public BuffDefinition rewardBuff;
    }
}
