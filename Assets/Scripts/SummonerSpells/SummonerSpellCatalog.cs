// Project D | Codename: Zantetsuken — SummonerSpellCatalog.cs
// Summoner-spell definitions: cooldowns, effects, targeting, charges, per-mode availability, icon paths.
// Pure data + lookups (engine-agnostic, deterministic). All spell names are PLACEHOLDERS for
// Final Fantasy-flavoured originals. Cooldowns and the numbers in the design brief are exact;
// anything marked "placeholder" is a tunable guess.
using System;
using System.Collections.Generic;
using ProjectD.Modes;

namespace ProjectD.SummonerSpells
{
    public enum SummonerSpellId
    {
        // Standard set: Normal and Ranked (and everywhere else the map rule allows).
        Haste, Cure, Protect, Slow, Esuna, Warp, Bio, Ultima, Teleport,
        // Additional set: non-Normal / non-Ranked modes only.
        Pray, Warpstrike
    }

    public enum SpellTargeting { Self, SelfAndAlly, EnemyChampion, NeutralMonster, Direction, AlliedStructureOrUnit }

    /// <summary>Which maps a spell works on. CanonicalMapOnly = the main lane map, not rotating World Rift realms.</summary>
    public enum SpellMapRule { AnyMap, CanonicalMapOnly }

    [Flags]
    public enum TeleportTargets { None = 0, AlliedStructure = 1, AlliedChampion = 2, Ward = 4 }

    public enum SpellEffectKind
    {
        MoveSpeedBuff, Heal, Shield, MoveSpeedSlow, DamageDealtReduction, Cleanse, DisableImmunity, Blink,
        DamageOverTime, HealingReduction, GrantVision, TrueDamage, Teleport, ResourceRestore, ResourceRestoreAllies,
        MarkProjectile, DashToMark
    }

    /// <summary>
    /// One effect line. Value(level) = Magnitude + PerLevel * (level - 1).
    /// If DecaySeconds &gt; 0 the effect starts at StartMagnitude and eases linearly to Value(level) over DecaySeconds,
    /// then holds that value until DurationSeconds ends (see ValueAt).
    /// </summary>
    public readonly struct SpellEffect
    {
        public readonly SpellEffectKind Kind;
        public readonly float Magnitude;
        public readonly float PerLevel;
        public readonly float DurationSeconds;
        public readonly float StartMagnitude;
        public readonly float DecaySeconds;

        public SpellEffect(SpellEffectKind kind, float magnitude, float perLevel = 0f, float durationSeconds = 0f,
            float startMagnitude = 0f, float decaySeconds = 0f)
        {
            Kind = kind; Magnitude = magnitude; PerLevel = perLevel; DurationSeconds = durationSeconds;
            StartMagnitude = startMagnitude; DecaySeconds = decaySeconds;
        }

        /// <summary>An effect that grows evenly from atLevel1 to atMaxLevel across levels 1..MaxLevel.</summary>
        public static SpellEffect Scaled(SpellEffectKind kind, float atLevel1, float atMaxLevel, float durationSeconds = 0f)
            => new SpellEffect(kind, atLevel1, (atMaxLevel - atLevel1) / (SummonerSpellCatalog.MaxLevel - 1), durationSeconds);

        public bool Decays => DecaySeconds > 0f;

        public float Value(int level)
        {
            int lv = Math.Max(1, Math.Min(SummonerSpellCatalog.MaxLevel, level));
            return Magnitude + PerLevel * (lv - 1);
        }

        /// <summary>The value elapsedSeconds after the effect began (equals Value(level) when it does not decay).</summary>
        public float ValueAt(int level, float elapsedSeconds)
        {
            float end = Value(level);
            if (!Decays) return end;
            float t = Math.Max(0f, Math.Min(1f, elapsedSeconds / DecaySeconds));
            return StartMagnitude + (end - StartMagnitude) * t;
        }
    }

    public sealed class SummonerSpellDef
    {
        public SummonerSpellId Id { get; }
        public string DisplayName { get; }
        /// <summary>Design reference only (the genre staple this spell plays like). Never shown to players.</summary>
        public string Archetype { get; }
        public float CooldownSeconds { get; }
        public float RangeUnits { get; }
        public SpellTargeting Targeting { get; }
        public string Description { get; }
        public bool IsStandard { get; }
        public IReadOnlyList<SpellEffect> Effects { get; }

        public float CastTimeSeconds { get; internal set; }          // > 0 means a channel (Teleport)
        public float RecastWindowSeconds { get; internal set; }      // > 0 means a second press is allowed (Warpstrike)
        public SpellMapRule MapRule { get; internal set; } = SpellMapRule.AnyMap;

        /// <summary>Stored charges (1 = a plain cooldown spell). With charges, CooldownSeconds is the delay between casts.</summary>
        public int MaxCharges { get; internal set; } = 1;
        public float ChargeRegenSeconds { get; internal set; }

        /// <summary>Haste only: each takedown adds this many seconds back, capped at the original duration.</summary>
        public float TakedownExtendSeconds { get; internal set; }

        /// <summary>Cure only: a target healed within this window receives RecentRecipientMultiplier of the heal.</summary>
        public float RecentRecipientWindowSeconds { get; internal set; }
        public float RecentRecipientMultiplier { get; internal set; } = 1f;

        /// <summary>Teleport only: allied structures are valid targets only before this match time.</summary>
        public float StructureTeleportWindowSeconds { get; internal set; }

        /// <summary>Resources.Load path (no extension). Icons live in Assets/Resources/UI/SummonerSpells/.</summary>
        public string IconResourcePath => "UI/SummonerSpells/summoner_" + Id.ToString().ToLowerInvariant();

        public bool TryGetEffect(SpellEffectKind kind, out SpellEffect effect)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                if (Effects[i].Kind == kind) { effect = Effects[i]; return true; }
            }
            effect = default;
            return false;
        }

        internal SummonerSpellDef(SummonerSpellId id, string name, string archetype, float cooldown, float range,
            SpellTargeting targeting, string description, bool isStandard, params SpellEffect[] effects)
        {
            Id = id; DisplayName = name; Archetype = archetype; CooldownSeconds = cooldown; RangeUnits = range;
            Targeting = targeting; Description = description; IsStandard = isStandard; Effects = effects;
        }
    }

    public static class SummonerSpellCatalog
    {
        public const int MaxLevel = 15;

        private static readonly Dictionary<SummonerSpellId, SummonerSpellDef> Defs = Build();

        // Modes that may use the additional (non-standard) spells. Normal and Ranked never do.
        private static readonly HashSet<GameMode> AdditionalSpellModes = new HashSet<GameMode>
        {
            GameMode.CoOpVsAI, GameMode.Practice, GameMode.WorldRift
        };

        public static IEnumerable<SummonerSpellDef> All
        {
            get { foreach (SummonerSpellId id in Enum.GetValues(typeof(SummonerSpellId))) yield return Defs[id]; }
        }

        public static SummonerSpellDef Get(SummonerSpellId id) => Defs[id];

        public static bool IsAvailable(SummonerSpellId id, GameMode mode)
        {
            var d = Defs[id];
            if (!d.IsStandard && !AdditionalSpellModes.Contains(mode)) return false;
            if (d.MapRule == SpellMapRule.CanonicalMapOnly
                && GameModeRegistry.Get(mode).mapPolicy != MapPolicy.CanonicalOnly) return false;
            return true;
        }

        public static List<SummonerSpellDef> AvailableFor(GameMode mode)
        {
            var list = new List<SummonerSpellDef>();
            foreach (var d in All) if (IsAvailable(d.Id, mode)) list.Add(d);
            return list;
        }

        private static Dictionary<SummonerSpellId, SummonerSpellDef> Build()
        {
            var d = new Dictionary<SummonerSpellId, SummonerSpellDef>();
            void Add(SummonerSpellDef s) => d[s.Id] = s;

            // Haste: a burst of speed that eases down to +25% (the burst peak is a placeholder). 8 s.
            Add(new SummonerSpellDef(SummonerSpellId.Haste, "Haste", "Ghost", 90f, 0f, SpellTargeting.Self,
                "Surge forward with a burst of speed that settles into a steady boost. Takedowns extend it.", true,
                new SpellEffect(SpellEffectKind.MoveSpeedBuff, 0.25f, 0f, 8f, startMagnitude: 0.55f, decaySeconds: 2f)
            ) { TakedownExtendSeconds = 6f });

            // Cure: 110 -> 400 by level, to you and the most wounded nearby ally; +30% speed for 2 s.
            Add(new SummonerSpellDef(SummonerSpellId.Cure, "Cure", "Heal", 100f, 6f, SpellTargeting.SelfAndAlly,
                "Restores health to you and your most wounded nearby ally and briefly speeds you both up. Repeat heals are weaker.", true,
                SpellEffect.Scaled(SpellEffectKind.Heal, 110f, 400f),
                new SpellEffect(SpellEffectKind.MoveSpeedBuff, 0.30f, 0f, 2f)
            ) { RecentRecipientWindowSeconds = 35f /* placeholder */, RecentRecipientMultiplier = 0.5f });

            // Protect: 120 -> 560 by level, 2.5 s.
            Add(new SummonerSpellDef(SummonerSpellId.Protect, "Protect", "Barrier", 100f, 0f, SpellTargeting.Self,
                "Wraps you in a shield that absorbs damage for a short time.", true,
                SpellEffect.Scaled(SpellEffectKind.Shield, 120f, 560f, 2.5f)));

            // Slow: slow starts at 35% and fades to nothing over its 2.5 s; -40% damage dealt.
            Add(new SummonerSpellDef(SummonerSpellId.Slow, "Slow", "Exhaust", 100f, 6f, SpellTargeting.EnemyChampion,
                "Saps an enemy champion: they are slowed (fading over time) and deal less damage.", true,
                new SpellEffect(SpellEffectKind.MoveSpeedSlow, 0f, 0f, 2.5f, startMagnitude: 0.35f, decaySeconds: 2.5f),
                new SpellEffect(SpellEffectKind.DamageDealtReduction, 0.40f, 0f, 2.5f)));

            // Esuna: removes disables (including spell debuffs), then 0.25 s of disable immunity.
            Add(new SummonerSpellDef(SummonerSpellId.Esuna, "Esuna", "Cleanse", 110f, 0f, SpellTargeting.Self,
                "Removes disabling effects (including spell debuffs) and briefly grants immunity to new ones.", true,
                new SpellEffect(SpellEffectKind.Cleanse, 1f),
                new SpellEffect(SpellEffectKind.DisableImmunity, 1f, 0f, 0.25f)));

            // Warp: short blink toward the aimed direction.
            Add(new SummonerSpellDef(SummonerSpellId.Warp, "Warp", "Flash", 150f, 4.5f, SpellTargeting.Direction,
                "Instantly blink a short distance toward the aimed direction.", true,
                new SpellEffect(SpellEffectKind.Blink, 4.5f)));

            // Bio: 72 -> 380 true damage over 5 s, 60% healing reduction, reveals the target.
            Add(new SummonerSpellDef(SummonerSpellId.Bio, "Bio", "Ignite", 100f, 5f, SpellTargeting.EnemyChampion,
                "Poisons an enemy champion with true damage over time, cuts their healing and reveals them.", true,
                SpellEffect.Scaled(SpellEffectKind.DamageOverTime, 72f, 380f, 5f),
                new SpellEffect(SpellEffectKind.HealingReduction, 0.60f, 0f, 5f),
                new SpellEffect(SpellEffectKind.GrantVision, 1f, 0f, 5f)));

            // Ultima: 600 true damage (see SmiteMath for tiers), 10 s between casts, up to 2 charges, +1 per 45 s.
            Add(new SummonerSpellDef(SummonerSpellId.Ultima, "Ultima", "Smite", 10f, 5f, SpellTargeting.NeutralMonster,
                "Blasts minions and jungle monsters with true damage. Clearing camps upgrades it.", true,
                new SpellEffect(SpellEffectKind.TrueDamage, 600f)
            ) { MaxCharges = 2, ChargeRegenSeconds = 45f, MapRule = SpellMapRule.CanonicalMapOnly });

            // Teleport: 3.5 s channel to an allied structure (first 6 min only), champion or ward.
            Add(new SummonerSpellDef(SummonerSpellId.Teleport, "Teleport", "Teleport", 150f, 0f, SpellTargeting.AlliedStructureOrUnit,
                "After a short channel, travel to an allied champion, ward or (early on) structure.", true,
                new SpellEffect(SpellEffectKind.Teleport, 0f, 0f, 3.5f)
            ) { CastTimeSeconds = 3.5f, StructureTeleportWindowSeconds = 360f, MapRule = SpellMapRule.CanonicalMapOnly });

            // Additional set --------------------------------------------------------------------

            Add(new SummonerSpellDef(SummonerSpellId.Pray, "Pray", "Clarity", 110f, 8f, SpellTargeting.SelfAndAlly,
                "Restores resource to you and, at a reduced amount, to nearby allies.", false,
                new SpellEffect(SpellEffectKind.ResourceRestore, 0.50f),
                new SpellEffect(SpellEffectKind.ResourceRestoreAllies, 0.25f)));

            Add(new SummonerSpellDef(SummonerSpellId.Warpstrike, "Warpstrike", "Snowball", 10f, 14f, SpellTargeting.Direction,
                "Hurl a warp mark in a line. Press again within the window to dash to whatever it hit.", false,
                new SpellEffect(SpellEffectKind.MarkProjectile, 14f, 0f, 3f),
                new SpellEffect(SpellEffectKind.DashToMark, 0f)
            ) { RecastWindowSeconds = 3f });

            return d;
        }
    }
}
