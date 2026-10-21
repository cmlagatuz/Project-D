// Project D  |  Codename: Zantetsuken
// EffectDefinition.cs  (Art pipeline data — VFX/SFX catalog entry)
//
// One asset per vfx_*/sfx_* key referenced by the animation designs. It is the
// single source of truth that binds a cue KEY (fired by AbilityAnimator /
// CombatReactionAnimator / NeutralAnimator / DeathRespawnController) to a concrete
// prefab (VFX) or AudioClip (SFX) plus its spawn rule: where it attaches, how long
// it lives, whether it follows the socket, pooling, and readability budget.
//
// The runtime EffectCatalog resolves keys to these definitions; artists fill in
// the actual prefab/clip references. Everything else is authored here.

using UnityEngine;

namespace ProjectD.Art.Effects
{
    public enum EffectKind { Vfx, Sfx }

    public enum EffectCategory
    {
        // VFX families
        AbilityCast, AbilityImpact, WeaponTrail, Projectile, GroundZone, Aura,
        Buff, Heal, Shield, Cleanse, Reaction, Death, Respawn, Objective, Summon,
        Monster, Minion, Environment,
        // SFX families
        SfxCast, SfxImpact, SfxVoice, SfxWhoosh, SfxUi, SfxAmbient
    }

    /// <summary>How a spawned effect is positioned/parented.</summary>
    public enum SpawnRule
    {
        AttachToSocket,     // parented to the named bone/socket, follows it
        SpawnAtSocket,      // spawned at socket world pos, does NOT follow (detached)
        SpawnAtTarget,      // spawned at the ability's target point/unit
        SpawnAtRoot,        // at the caster's root
        SpawnAtGroundPoint, // projected onto the ground at the aim/target point
        ProjectileTravel,   // spawned at origin, travels to target (handled by projectile system)
        WorldZone           // large persistent zone placed in the world
    }

    [CreateAssetMenu(fileName = "FX_", menuName = "Project D/Effect Definition")]
    public sealed class EffectDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string key;                 // e.g. vfx_radiant_arc, sfx_sword_woosh
        public EffectKind kind = EffectKind.Vfx;
        public EffectCategory category = EffectCategory.AbilityImpact;
        [TextArea(1, 3)] public string description;

        [Header("Asset (artist fills these)")]
        public GameObject vfxPrefab;       // for kind == Vfx
        public AudioClip audioClip;        // for kind == Sfx

        [Header("Spawn rule")]
        public SpawnRule spawnRule = SpawnRule.SpawnAtSocket;
        [Tooltip("Default socket/bone if the cue doesn't override it (weapon_tip, hand_r, root, feet, chest...).")]
        public string defaultSocket = "root";
        [Tooltip("Seconds before auto-despawn/return-to-pool. 0 = one-shot self-terminating (particle system stop).")]
        public float lifetime = 2f;
        [Tooltip("If true the effect follows the socket after spawn (attach); else it detaches at spawn pos.")]
        public bool followSocket = false;

        [Header("Pooling & performance (GCD 13.3)")]
        [Tooltip("Pool this effect (recommended for anything spawned frequently: hits, minions, projectiles).")]
        public bool pooled = true;
        [Range(0, 64)] public int prewarmCount = 4;
        [Tooltip("Max simultaneous instances of this key; excess requests are dropped (readability/perf budget).")]
        public int maxConcurrent = 12;

        [Header("Readability (VFX)")]
        [Tooltip("Danger telegraphs must remain visible at every quality level — never culled by the 'reduce effects' setting.")]
        public bool isDangerTelegraph = false;
        [Tooltip("Team-tinted so friend/foe reads correctly (color set by owner team at spawn).")]
        public bool teamColored = false;

        [Header("Audio (SFX)")]
        [Range(0f, 1f)] public float volume = 1f;
        public bool is3D = true;
        [Tooltip("Higher = wins when the voice/audio limiter must drop sounds (ults, deaths, low-HP > footsteps).")]
        [Range(0, 10)] public int priority = 5;
        public Vector2 pitchRandom = new Vector2(0.97f, 1.03f);
    }
}
