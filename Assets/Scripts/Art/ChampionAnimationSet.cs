// Project D  |  Codename: Zantetsuken
// ChampionAnimationSet.cs  (Art pipeline data — additive)
//
// One asset per champion holding the locomotion clip names plus the five ability
// animations (passive, Q, W, E, ultimate) and the universal reaction set. The
// AnimatorController generator reads this to lay out states/transitions, and the
// runtime AbilityAnimator plays the right clip when the server confirms a cast.

using UnityEngine;

namespace ProjectD.Art.Animation
{
    [CreateAssetMenu(fileName = "AnimSet_", menuName = "Project D/Champion Animation Set")]
    public sealed class ChampionAnimationSet : ScriptableObject
    {
        [Header("Identity")]
        public string championId;
        public string displayName;

        [Header("Locomotion & base states (loopable)")]
        public string idleClip = "idle";
        public string runClip = "run";
        public string turnLeftClip = "turn_l";
        public string turnRightClip = "turn_r";
        public string recallClip = "recall";
        public string deathClip = "death";
        public string spawnClip = "spawn";

        [Header("Universal reactions")]
        public string hitReactClip = "hit_react";
        public string knockupClip = "knockup";
        public string stunClip = "stun";

        [Header("Basic attack chain (auto-attacks)")]
        public string[] basicAttackChain = { "attack_1", "attack_2", "attack_3" };
        [Tooltip("Seconds; the attack's damage point within each basic clip.")]
        public float basicHitFrame = 0.25f;

        [Header("Abilities")]
        public AbilityAnimation passive;
        public AbilityAnimation abilityQ;
        public AbilityAnimation abilityW;
        public AbilityAnimation abilityE;
        public AbilityAnimation ultimate;

        [Header("Extended combat animation (attacks / reactions / healing / death)")]
        public BasicAttackChain attackChain;
        public ReactionSet reactions;
        public HealingEffectSet healing;
        public DeathAndRespawn deathRespawn;

        public AbilityAnimation ForSlot(AbilitySlotLite slot) => slot switch
        {
            AbilitySlotLite.Passive => passive,
            AbilitySlotLite.Q => abilityQ,
            AbilitySlotLite.W => abilityW,
            AbilitySlotLite.E => abilityE,
            AbilitySlotLite.Ultimate => ultimate,
            _ => null
        };
    }

    // Local slot enum so this art module has no hard dependency on the gameplay
    // AbilitySlot; the importer maps between them.
    public enum AbilitySlotLite { Passive, Q, W, E, Ultimate }
}
