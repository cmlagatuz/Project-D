// Project D  |  Codename: Zantetsuken
// CombatAnimationData.cs  (Art pipeline data — basic attacks, reactions, healing, death)
//
// Additive data types that complete the champion's non-ability animation coverage:
//   - BasicAttackChain: per-swing timing (windup/hit/recovery) + weapon-type flavor,
//     with combo reset and cancel-into-move windows.
//   - ReactionSet: universal hit/knock-up/stun/airborne/knockback/silence reactions
//     driven by SERVER crowd-control events (never client-guessed).
//   - HealingEffectSet: heal-receive, shield-gain/break, buff/cleanse visuals as
//     additive-layer overlays that never interrupt locomotion or casts.
//   - DeathAndRespawn: death, the respawn COUNTDOWN pose/loop, and the revive.
//
// These are POCO-serializable structs used by ChampionAnimationSet and the
// neutral/summon sets. Timings are seconds; clip names are what the DCC authors.

using System;
using UnityEngine;

namespace ProjectD.Art.Animation
{
    public enum WeaponClass { Sword, Greatsword, Lance, Bow, DualBlade, Staff, Gauntlet, Tentacle }

    [Serializable]
    public sealed class BasicAttackSwing
    {
        public string clipName;                 // e.g. wol_attack_1
        [Tooltip("Seconds from clip start to the damage/impact point.")]
        public float hitFrame = 0.24f;
        public float totalDuration = 0.55f;
        [Tooltip("Latest point (sec) the player can cancel into move/ability.")]
        public float cancelWindow = 0.30f;
        [Tooltip("Optional VFX key fired at the hit frame (e.g. weapon trail impact).")]
        public string impactVfxKey;
        public string swingSfxKey;
    }

    [Serializable]
    public sealed class BasicAttackChain
    {
        public WeaponClass weaponClass = WeaponClass.Sword;
        public BasicAttackSwing[] swings = new BasicAttackSwing[0];
        [Tooltip("Seconds of no-attack before the combo counter resets to swing 1.")]
        public float comboResetSeconds = 1.2f;
        [Tooltip("Attack-speed stat scales playback rate around this baseline (attacks/sec).")]
        public float baseAttacksPerSecond = 0.65f;
        [TextArea(1, 4)] public string notes;
    }

    [Serializable]
    public sealed class ReactionClip
    {
        public string clipName;
        public float duration = 0.4f;
        [Tooltip("If true this reaction fully interrupts current action (hard CC).")]
        public bool interrupts = true;
        public string reactVfxKey;
    }

    [Serializable]
    public sealed class ReactionSet
    {
        public ReactionClip flinch;        // light hit, does not interrupt
        public ReactionClip hitReact;      // standard damage react
        public ReactionClip knockback;     // pushed back
        public ReactionClip knockup;       // launched (airborne)
        public ReactionClip airborneLoop;  // held while airborne from a launch
        public ReactionClip stun;          // rooted + dazed loop
        public ReactionClip root;          // feet locked, upper body free
        public ReactionClip silence;       // can move, cannot cast (mouth/hands sealed FX)
        public ReactionClip slowOverlay;   // additive drag while slowed
        [TextArea(1, 4)] public string notes;
    }

    [Serializable]
    public sealed class HealingEffectSet
    {
        [Tooltip("Additive overlay when receiving healing.")]
        public string healReceiveClip = "heal_receive";
        public string healReceiveVfxKey = "vfx_heal_glow";
        public string shieldGainClip = "shield_gain";
        public string shieldGainVfxKey = "vfx_shield_on";
        public string shieldBreakVfxKey = "vfx_shield_break";
        public string buffReceiveVfxKey = "vfx_buff_up";
        public string cleanseClip = "cleanse";
        public string cleanseVfxKey = "vfx_cleanse_burst";
        [Tooltip("Heal/shield overlays play on an ADDITIVE layer so they never interrupt.")]
        public bool additiveOnly = true;
        [TextArea(1, 4)] public string notes;
    }

    [Serializable]
    public sealed class DeathAndRespawn
    {
        public string deathClip = "death";
        public string deathVfxKey = "vfx_death_dissolve";
        [Tooltip("Held pose/loop shown at the base during the respawn countdown.")]
        public string countdownLoopClip = "respawn_wait";
        [Tooltip("Revive/re-materialize clip when the timer hits zero.")]
        public string reviveClip = "spawn";
        public string reviveVfxKey = "vfx_respawn_materialize";
        [Tooltip("Whether a floating countdown number is shown over the corpse/base.")]
        public bool showCountdownNumber = true;
        [TextArea(1, 4)] public string notes;
    }
}
