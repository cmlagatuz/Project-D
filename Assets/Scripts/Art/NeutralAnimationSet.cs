// Project D  |  Codename: Zantetsuken
// NeutralAnimationSet.cs  (Art pipeline data — summons, monsters, minions)
//
// One asset per neutral unit (elemental summon, Odin, divine manifestation,
// jungle monster tier, or goblin minion). Neutrals don't have a 5-slot kit, but
// they DO need: spawn, idle, move, basic attack(s), one-or-more signature
// abilities (the mechanic that defines the camp/objective), a hit/death set, and
// for objectives a "reward/blessing grant" flourish. The AnimatorController
// generator reuses the same state pattern as champions.

using UnityEngine;

namespace ProjectD.Art.Animation
{
    public enum NeutralKind { ElementalSummon, ElderSummon, DivineManifestation, MonsterSmall, MonsterMedium, MonsterLarge, Minion }

    [CreateAssetMenu(fileName = "NeutralAnim_", menuName = "Project D/Neutral Animation Set")]
    public sealed class NeutralAnimationSet : ScriptableObject
    {
        [Header("Identity")]
        public string neutralId;            // e.g. ifrit, odin, materia, tonberry, goblin_melee
        public string displayName;
        public NeutralKind kind = NeutralKind.MonsterMedium;

        [Header("Base states")]
        public string spawnClip = "spawn";
        public string idleClip = "idle";
        public string moveClip = "move";
        public string[] basicAttacks = { "attack_1" };
        public float basicHitFrame = 0.3f;

        [Header("Signature abilities (the mechanic that defines this unit)")]
        public AbilityAnimation signatureA;   // primary (e.g. Ifrit fire cone, Tonberry knife)
        public AbilityAnimation signatureB;   // optional secondary (e.g. Odin Sleipnir charge)
        public AbilityAnimation signatureUltimate; // objective finisher (e.g. Odin Zantetsuken)

        [Header("Reactions & death")]
        public string hitReactClip = "hit_react";
        public string enrageClip;             // optional low-HP enrage (Behemoth, bosses)
        public string deathClip = "death";
        public string deathVfxKey = "vfx_death_burst";

        [Header("Objective flourish (summons / divine only)")]
        [Tooltip("Played when the objective is slain and grants its blessing to the killing team.")]
        public string rewardGrantClip = "reward_grant";
        public string rewardGrantVfxKey = "vfx_blessing_grant";

        [TextArea(2, 6)] public string notes;
    }
}
