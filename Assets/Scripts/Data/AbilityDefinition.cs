// Project D | Codename: Zantetsuken — AbilityDefinition.cs (GCD 9.3)
using UnityEngine; using ProjectD.Core;
namespace ProjectD.Data
{
    public enum AbilitySlot { Passive, Q, W, E, Ultimate }
    public enum TargetingMode { Self, UnitTarget, SkillshotLine, SkillshotCone, GroundArea, Directional, Toggle }
    [CreateAssetMenu(fileName = "Ability_", menuName = "Project D/Ability Definition")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")] public string abilityId; public string displayName; public AbilitySlot slot; [TextArea] public string designerNotes;
        [Header("Targeting & Timing")] public TargetingMode targeting = TargetingMode.SkillshotLine;
        public float range = 6f; public float radius = 1.5f; public float startupSeconds = 0.2f; public float activeSeconds = 0.1f; public float recoverySeconds = 0.3f; public bool canBeInterrupted = true;
        [Header("Cost & Cooldown")] public float baseCooldown = 8f; public float resourceCost = 40f;
        [Header("Payload")] public DamageType damageType = DamageType.Adaptive; public float[] baseDamagePerRank = { 60f, 100f, 140f, 180f }; public float damageRatio = 0.6f; public float controlDurationSeconds; public BuffDefinition appliedBuff;
        [Header("Presentation Keys")] public string castVfxKey, impactVfxKey, castSfxKey; public bool isDangerTelegraph = true;
    }
}
