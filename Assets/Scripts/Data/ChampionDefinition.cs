// Project D | Codename: Zantetsuken — ChampionDefinition.cs (GCD 9.x)
using UnityEngine; using ProjectD.Core;
namespace ProjectD.Data
{
    [CreateAssetMenu(fileName = "Champion_", menuName = "Project D/Champion Definition")]
    public sealed class ChampionDefinition : ScriptableObject
    {
        [Header("Identity")] public string championId; public string displayName; public string sourceWorld; public string sourceReleaseDate;
        public ChampionRole primaryRole; public ChampionRole secondaryRole; [TextArea] public string coreFantasy;
        [Header("Extended Bio (GCD 9.2)")] [TextArea(3,8)] public string loreSummary; [TextArea(2,6)] public string playstyle; [TextArea(1,4)] public string strengths; [TextArea(1,4)] public string weaknesses; [TextArea(1,4)] public string tipsAndCombos;
        [Header("Model & Rig")] public GameObject modelPrefab; public string assetLicenseRef;
        [Header("Base Stats")] public float baseHealth = 600f, baseResource = 300f, baseAttackDamage = 60f, baseMagicPower = 0f, baseArmor = 30f, baseMagicResist = 30f, baseMoveSpeed = 3.5f, baseAttackRange = 1.75f, baseAttacksPerSecond = 0.65f;
        [Header("Growth")] public float healthGrowth = 90f, attackDamageGrowth = 3.5f, armorGrowth = 4f, magicResistGrowth = 1.5f;
        [Header("Kit")] public AbilityDefinition passive, abilityQ, abilityW, abilityE, ultimate;
        [Header("Bot / Tutorial")] public bool availableInStarterRoster; [Range(1,5)] public int mechanicalDifficulty = 3;
    }
}
