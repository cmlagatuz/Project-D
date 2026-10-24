// Project D | Codename: Zantetsuken — DuelTypes.cs
// Engine-agnostic data types for the 1v1 duel demo: 2D vector, statuses, events, ability and champion templates.
// Nothing here touches UnityEngine, so the whole duel can be unit-tested and replayed deterministically.
using System;
using System.Collections.Generic;
using ProjectD.SummonerSpells;

namespace ProjectD.Duel
{
    /// <summary>A point or direction on the ground plane (X right, Y "up the screen", mapped to world Z in Unity).</summary>
    public readonly struct DVec2
    {
        public readonly float X;
        public readonly float Y;

        public DVec2(float x, float y) { X = x; Y = y; }

        public static readonly DVec2 Zero = new DVec2(0f, 0f);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public DVec2 Normalized
        {
            get { float l = Length; return l < 1e-5f ? Zero : new DVec2(X / l, Y / l); }
        }

        public static DVec2 operator +(DVec2 a, DVec2 b) => new DVec2(a.X + b.X, a.Y + b.Y);
        public static DVec2 operator -(DVec2 a, DVec2 b) => new DVec2(a.X - b.X, a.Y - b.Y);
        public static DVec2 operator *(DVec2 a, float s) => new DVec2(a.X * s, a.Y * s);

        public static float Distance(DVec2 a, DVec2 b) => (a - b).Length;
        public static float Dot(DVec2 a, DVec2 b) => a.X * b.X + a.Y * b.Y;
    }

    public enum UnitKind { Champion, Monster, Ward }

    public enum DuelDamage { Physical, Magical, True }

    public enum StatusKind
    {
        MoveSpeedBuff, MoveSpeedSlow, DamageDealtReduction, Shield, Stun,
        DisableImmunity, DamageOverTime, HealingReduction
    }

    /// <summary>A timed effect on a unit. The value can ease from StartValue to EndValue over DecaySeconds.</summary>
    public sealed class Status
    {
        public StatusKind Kind;
        public float StartValue;
        public float EndValue;
        public float DecaySeconds;
        public float StartedAt;
        public float ExpiresAt;
        public int SourceId;
        /// <summary>Shield: absorption left. DamageOverTime: unused (StartValue is damage per second).</summary>
        public float Amount;

        public bool IsActive(float now) => now < ExpiresAt;

        public float Value(float now)
        {
            if (DecaySeconds <= 0f) return EndValue;
            float t = Math.Max(0f, Math.Min(1f, (now - StartedAt) / DecaySeconds));
            return StartValue + (EndValue - StartValue) * t;
        }
    }

    /// <summary>Why a command did or did not happen. The UI turns these into short messages.</summary>
    public enum CastOutcome
    {
        Ok, NoUnit, Dead, Stunned, OnCooldown, NoMana, NoTarget, OutOfRange, Busy, EmptySlot
    }

    public enum DuelEventKind { Damage, Heal, Shield, Kill, Respawn, Text, Area, Gold }

    public enum AreaShape { Circle, Line }

    public struct DuelEvent
    {
        public float Time;
        public DuelEventKind Kind;
        public int UnitId;       // who it happened to
        public int OtherId;      // who caused it (or -1)
        public float Amount;
        public string Text;
        // Area flashes (visual only)
        public AreaShape Shape;
        public DVec2 Pos;
        public DVec2 Dir;
        public float Length;
        public float Width;      // circle: diameter
    }

    public enum AbilityShape { Cone, Line, Circle, Shield, Dash }

    public sealed class DuelAbility
    {
        public string Name;
        public AbilityShape Shape;
        public DuelDamage Type = DuelDamage.Physical;
        public float Range;
        public float Radius;            // circle radius, or line half-width, or dash landing radius
        public float AngleDegrees;      // cone only (full angle)
        public float[] Damage = new float[0];   // per rank
        public float Ratio;             // of AD (physical) or AP (magical)
        public float Cooldown;
        public float Cost;
        public float SlowPct;
        public float SlowSeconds;
        public float StunSeconds;
        public float ShieldAmount;
        public float ShieldSeconds;
    }

    public sealed class DuelChampionTemplate
    {
        public string Id;
        public string Name;
        public float Hp, HpGrow, Mana, Ad, AdGrow, Ap, Armor, ArmorGrow, Mr, MrGrow;
        public float MoveSpeed, AttackRange, AttackSpeed;
        public DuelAbility[] Abilities = new DuelAbility[4];   // Q, W, E, R
    }

    /// <summary>Roster for the demo. Numbers follow Assets/GameData/champions.seed.json; cooldowns are shortened for testing.</summary>
    public static class DuelRoster
    {
        public static DuelChampionTemplate WarriorOfLight()
        {
            return new DuelChampionTemplate
            {
                Id = "wol", Name = "Warrior of Light",
                Hp = 660f, HpGrow = 100f, Mana = 300f, Ad = 62f, AdGrow = 3.4f, Ap = 0f,
                Armor = 38f, ArmorGrow = 4.4f, Mr = 34f, MrGrow = 1.6f,
                MoveSpeed = 3.5f, AttackRange = 1.75f, AttackSpeed = 0.65f,
                Abilities = new[]
                {
                    new DuelAbility { Name = "Radiant Cut", Shape = AbilityShape.Cone, Range = 3.0f, AngleDegrees = 100f,
                        Damage = new[] { 70f, 110f, 150f, 190f }, Ratio = 0.6f, Cooldown = 7f, Cost = 35f,
                        SlowPct = 0.30f, SlowSeconds = 1.5f },
                    new DuelAbility { Name = "Shield of Light", Shape = AbilityShape.Shield, Range = 0f, Radius = 2.0f,
                        Type = DuelDamage.Magical, Damage = new[] { 40f, 70f, 100f, 130f }, Ratio = 0.4f,
                        Cooldown = 10f, Cost = 50f, ShieldAmount = 150f, ShieldSeconds = 3f },
                    new DuelAbility { Name = "Vanguard Step", Shape = AbilityShape.Dash, Range = 5.0f, Radius = 1.4f,
                        Damage = new[] { 30f, 50f, 70f, 90f }, Ratio = 0.3f, Cooldown = 9f, Cost = 50f, StunSeconds = 0.5f },
                    new DuelAbility { Name = "Shining Bulwark", Shape = AbilityShape.Shield, Range = 0f, Radius = 0f,
                        Cooldown = 45f, Cost = 100f, ShieldAmount = 300f, ShieldSeconds = 4f }
                }
            };
        }

        public static DuelChampionTemplate Garland()
        {
            return new DuelChampionTemplate
            {
                Id = "garland", Name = "Garland",
                Hp = 640f, HpGrow = 96f, Mana = 300f, Ad = 66f, AdGrow = 3.8f, Ap = 0f,
                Armor = 36f, ArmorGrow = 4.2f, Mr = 32f, MrGrow = 1.5f,
                MoveSpeed = 3.5f, AttackRange = 1.75f, AttackSpeed = 0.62f,
                Abilities = new[]
                {
                    new DuelAbility { Name = "Greatblade Reap", Shape = AbilityShape.Cone, Range = 3.2f, AngleDegrees = 120f,
                        Damage = new[] { 80f, 120f, 160f, 200f }, Ratio = 0.7f, Cooldown = 8f, Cost = 40f },
                    new DuelAbility { Name = "Iron Advance", Shape = AbilityShape.Dash, Range = 4.5f, Radius = 1.5f,
                        Damage = new[] { 60f, 95f, 130f, 165f }, Ratio = 0.5f, Cooldown = 12f, Cost = 45f, StunSeconds = 0.6f },
                    new DuelAbility { Name = "Chain of Ruin", Shape = AbilityShape.Line, Range = 6.0f, Radius = 0.6f,
                        Damage = new[] { 50f, 80f, 110f, 140f }, Ratio = 0.4f, Cooldown = 10f, Cost = 50f,
                        SlowPct = 0.35f, SlowSeconds = 2f },
                    new DuelAbility { Name = "Chaosbringer", Shape = AbilityShape.Circle, Range = 0f, Radius = 3.2f,
                        Damage = new[] { 100f, 150f, 200f, 200f }, Ratio = 0.5f, Cooldown = 40f, Cost = 100f, StunSeconds = 0.8f }
                }
            };
        }
    }
}
