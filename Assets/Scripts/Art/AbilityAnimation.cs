// Project D  |  Codename: Zantetsuken
// AbilityAnimation.cs  (Art pipeline data — animation design, additive)
//
// Engine-wired animation spec for a single ability. It mirrors the timing fields
// already on AbilityDefinition (startup/active/recovery) so the animation and the
// gameplay state machine stay in lockstep, and adds the animator-facing details a
// technical animator needs: clip name, root motion, cancel window, loop, VFX/SFX
// sync times, and per-phase notes. This is DATA that describes clips the artist
// authors in the DCC; it does not generate motion by itself.

using System;
using UnityEngine;

namespace ProjectD.Art.Animation
{
    public enum AnimTargeting { Self, Directional, Point, Unit }

    /// <summary>How the ability's motion is delivered.</summary>
    public enum MotionKind
    {
        InPlace,        // stationary cast (most abilities)
        RootMotion,     // clip drives displacement (dashes, leaps, charges)
        Steerable       // partial root motion the player can nudge
    }

    [Serializable]
    public struct AnimVfxCue
    {
        [Tooltip("Seconds from clip start when this cue fires.")]
        public float time;
        [Tooltip("VFX/SFX key resolved by the effects system (matches AbilityDefinition.castVfxKey/impactVfxKey).")]
        public string key;
        [Tooltip("Optional bone/socket to spawn on, e.g. weapon_tip, hand_r, root.")]
        public string socket;
    }

    [Serializable]
    public sealed class AbilityAnimation
    {
        [Header("Binding")]
        [Tooltip("Ability id this animation belongs to, e.g. wol_q.")]
        public string abilityId;
        [Tooltip("Human label, e.g. 'Radiant Cut'.")]
        public string displayName;

        [Header("Clips (author these in the DCC)")]
        [Tooltip("Primary clip name, e.g. 'wol_q_radiant_cut'.")]
        public string clipName;
        [Tooltip("Optional hold/charge loop clip (for chargeable casts).")]
        public string chargeLoopClip;
        [Tooltip("Optional recast/second-cast clip (e.g. Kain recast, Onion Knight resets).")]
        public string recastClip;

        [Header("Phase timing (seconds — keep in sync with AbilityDefinition)")]
        public float startup;    // anticipation/wind-up before the ability becomes active
        public float active;     // the committed hit/effect window
        public float recovery;   // follow-through before the character regains control
        [Tooltip("Latest point (sec from start) the player may cancel into move/basic.")]
        public float cancelWindow;
        public bool loop;        // true only for stances/channels

        [Header("Motion")]
        public MotionKind motion = MotionKind.InPlace;
        public AnimTargeting targeting = AnimTargeting.Directional;
        [Tooltip("Meters travelled if RootMotion/Steerable (design intent; clip must match).")]
        public float travelDistance;

        [Header("Weapon motion (uses the WeaponSocket rig)")]
        [Tooltip("If true, the separate/held weapon has authored keys in this clip (swings, thrusts).")]
        public bool animatesWeapon = true;
        [Tooltip("Optional trail VFX key drawn along the weapon during 'active'.")]
        public string weaponTrailKey;

        [Header("Sync cues")]
        public AnimVfxCue[] vfxCues;
        public AnimVfxCue[] sfxCues;

        [Header("Notes")]
        [TextArea(2, 6)] public string animatorNotes;

        public float TotalDuration => startup + active + recovery;
    }
}
