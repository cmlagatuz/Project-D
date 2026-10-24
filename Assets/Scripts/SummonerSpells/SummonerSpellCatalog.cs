// Project D | Codename: Zantetsuken — SummonerSpellCatalog.cs
// Summoner-spell definitions: cooldowns, effects, targeting, per-mode availability, icon paths.
// Pure data + lookups (engine-agnostic, deterministic). All spell names are PLACEHOLDERS for
// Final Fantasy-flavoured originals; effect numbers other than cooldowns are tunable placeholders.
using System;
using System.Collections.Generic;
using ProjectD.Modes;

namespace ProjectD.SummonerSpells
{
    public enum SummonerSpellId
    {
        // Standard set: every mode, including Normal and Ranked.
        Haste, Cure, Protect, Slow, Esuna, Warp, Bio, Ultima, Teleport,
        // Additional set: non-Normal / non-Ranked modes only.
        Pray, Warpstrike
    }

    public enum SpellTargeting { Self, SelfAndAlly, EnemyChampion, NeutralMonster, Direction, AlliedStructureOrUnit }

    public enum SpellEffectKind
    {
        MoveSpeedBuff, Heal, Shield, MoveSpeedSlow, DamageDealtReduction, Cleanse, Blink,
        DamageOverTime, HealingReduction, TrueDamage, Teleport, ResourceRestore, ResourceRestoreAllies,
        MarkProjectile, DashToMark
    }

    /// <summary>One effect line. Value(level) = magnitude + perLevel * (level - 1).</summary>
    public readonly struct SpellEffect
    {
        public readonly SpellEffectKind Kind;
        public readonly float Magnitude;
        public readonly float PerLevel;
        public readonly float DurationSeconds;

        public SpellEffect(SpellEffectKind kind, float magnitude, float perLevel = 0f, float durationSeconds = 0f)
        {
            Kind = kind; Magnitude = magnitude; PerLevel = perLevel; DurationSeconds = durationSeconds;
        }

        public float Value(int level)
        {
            int lv = Math.Max(1, Math.Min(SummonerSpellCatalog.MaxLevel, level));
            return Magnitude + PerLevel * (lv - 1);
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
        public float CastTimeSeconds { get; }       // > 0 means a channel (Teleport)
        public float RecastWindowSeconds { get; }   // > 0 means a second press is allowed (Warpstrike)
        public SpellTargeting Targeting { get; }
        public string Description { get; }
        public bool IsStandard { get; }
        public IReadOnlyList<SpellEffect> Effects { get; }

        /// <summary>Resources.Load path (no extension). Icons live in Assets/Resources/UI/SummonerSpells/.</summary>
        public string IconResourcePath => "UI/SummonerSpells/summoner_" + Id.ToString().ToLowerInvariant();

        internal SummonerSpellDef(SummonerSpellId id, string name, string archetype, float cooldown, float range,
            SpellTargeting targeting, string description, bool isStandard, float castTime, float recastWindow,
            params SpellEffect[] effects)
        {
            Id = id; DisplayName = name; Archetype = archetype; CooldownSeconds = cooldown; RangeUnits = range;
            Targeting = targeting; Description = description; IsStandard = isStandard;
            CastTimeSeconds = castTime; RecastWindowSeconds = recastWindow; Effects = effects;
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
            => Defs[id].IsStandard || AdditionalSpellModes.Contains(mode);

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

            Add(new SummonerSpellDef(SummonerSpellId.Haste, "Haste", "Ghost", 90f, 0f, SpellTargeting.Self,
                "Greatly increases your movement speed for a short time.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.MoveSpeedBuff, 0.25f, 0f, 10f)));

            Add(new SummonerSpellDef(SummonerSpellId.Cure, "Cure", "Heal", 100f, 6f, SpellTargeting.SelfAndAlly,
                "Restores health to you and a nearby ally, and briefly speeds you both up.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.Heal, 90f, 15f),
                new SpellEffect(SpellEffectKind.MoveSpeedBuff, 0.30f, 0f, 1f)));

            Add(new SummonerSpellDef(SummonerSpellId.Protect, "Protect", "Barrier", 100f, 0f, SpellTargeting.Self,
                "Wraps you in a shield that absorbs damage for a short time.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.Shield, 105f, 20f, 2f)));

            Add(new SummonerSpellDef(SummonerSpellId.Slow, "Slow", "Exhaust", 100f, 6f, SpellTargeting.EnemyChampion,
                "Slows an enemy champion and reduces the damage they deal.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.MoveSpeedSlow, 0.30f, 0f, 3f),
                new SpellEffect(SpellEffectKind.DamageDealtReduction, 0.35f, 0f, 3f)));

            Add(new SummonerSpellDef(SummonerSpellId.Esuna, "Esuna", "Cleanse", 110f, 0f, SpellTargeting.Self,
                "Removes all disabling effects and negative statuses from you.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.Cleanse, 1f)));

            Add(new SummonerSpellDef(SummonerSpellId.Warp, "Warp", "Flash", 150f, 4.5f, SpellTargeting.Direction,
                "Instantly blink a short distance toward the target point.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.Blink, 4.5f)));

            Add(new SummonerSpellDef(SummonerSpellId.Bio, "Bio", "Ignite", 100f, 5f, SpellTargeting.EnemyChampion,
                "Poisons an enemy champion with true damage over time and cuts their healing.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.DamageOverTime, 50f, 20f, 5f),
                new SpellEffect(SpellEffectKind.HealingReduction, 0.5f, 0f, 5f)));

            Add(new SummonerSpellDef(SummonerSpellId.Ultima, "Ultima", "Smite", 10f, 5f, SpellTargeting.NeutralMonster,
                "Blasts a neutral monster with a burst of true damage.", true, 0f, 0f,
                new SpellEffect(SpellEffectKind.TrueDamage, 600f, 40f)));

            Add(new SummonerSpellDef(SummonerSpellId.Teleport, "Teleport", "Teleport", 150f, 0f, SpellTargeting.AlliedStructureOrUnit,
                "After a short channel, travel to an allied structure or minion. Taking a hard disable cancels it.", true, 3.5f, 0f,
                new SpellEffect(SpellEffectKind.Teleport, 0f, 0f, 3.5f)));

            Add(new SummonerSpellDef(SummonerSpellId.Pray, "Pray", "Clarity", 110f, 8f, SpellTargeting.SelfAndAlly,
                "Restores resource to you and, at a reduced amount, to nearby allies.", false, 0f, 0f,
                new SpellEffect(SpellEffectKind.ResourceRestore, 0.50f),
                new SpellEffect(SpellEffectKind.ResourceRestoreAllies, 0.25f)));

            Add(new SummonerSpellDef(SummonerSpellId.Warpstrike, "Warpstrike", "Snowball", 10f, 14f, SpellTargeting.Direction,
                "Hurl a warp mark in a line. Press again within the window to dash to whatever it hit.", false, 0f, 3f,
                new SpellEffect(SpellEffectKind.MarkProjectile, 14f, 0f, 3f),
                new SpellEffect(SpellEffectKind.DashToMark, 0f)));

            return d;
        }
    }
}
