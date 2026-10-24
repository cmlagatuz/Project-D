// Project D | Codename: Zantetsuken — DuelUnit.cs
// One unit in the duel (champion, training monster or ward) with its stats, statuses and summoner-spell state.
using System;
using System.Collections.Generic;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.Duel
{
    public sealed class DuelUnit
    {
        public const float Radius = 0.5f;

        public int Id;
        public string Name;
        public UnitKind Kind;
        public int Team;                 // 0 blue, 1 red, 2 neutral
        public DuelChampionTemplate Template;

        public DVec2 Pos;
        public DVec2 SpawnPos;
        public DVec2 Facing = new DVec2(0f, 1f);
        public DVec2 MoveInput = DVec2.Zero;

        public int Level = 1;
        public bool Alive = true;
        public float RespawnAt;

        public float Hp, MaxHp, Mana, MaxMana;
        public float Ad, Ap, Armor, Mr, BaseMoveSpeed, AttackRange, AttackSpeed;
        public float HpRegen = 2f;
        public float ManaRegen = 8f;     // generous on purpose: the demo is about testing spells

        public float NextAttackAt;
        public readonly float[] AbilityReadyAt = new float[4];

        public readonly List<Status> Statuses = new List<Status>();
        public readonly HasteBuff Haste = new HasteBuff();

        public SummonerLoadout Loadout = SummonerLoadout.Default();
        public SummonerSpellTracker Spells;
        public SmiteProgress Smite;

        public int Gold;
        public int Kills;
        public int Deaths;

        // Teleport channel
        public bool Channeling;
        public float ChannelEndsAt;
        public DVec2 ChannelDest;

        // Warpstrike mark
        public int MarkTargetId = -1;
        public float MarkExpiresAt;

        public bool IsEnemyOf(DuelUnit other) => other != null && other.Team != Team && other.Team != 2 && Team != 2;

        public bool IsActive(StatusKind kind, float now)
        {
            for (int i = 0; i < Statuses.Count; i++)
                if (Statuses[i].Kind == kind && Statuses[i].IsActive(now)) return true;
            return false;
        }

        public float SumValue(StatusKind kind, float now)
        {
            float sum = 0f;
            for (int i = 0; i < Statuses.Count; i++)
                if (Statuses[i].Kind == kind && Statuses[i].IsActive(now)) sum += Statuses[i].Value(now);
            return sum;
        }

        public float MaxValue(StatusKind kind, float now)
        {
            float best = 0f;
            for (int i = 0; i < Statuses.Count; i++)
                if (Statuses[i].Kind == kind && Statuses[i].IsActive(now)) best = Math.Max(best, Statuses[i].Value(now));
            return best;
        }

        public bool IsStunned(float now) => IsActive(StatusKind.Stun, now);

        public float MoveSpeed(float now)
        {
            if (IsStunned(now)) return 0f;
            float mult = 1f + Haste.SpeedBonus(now) + SumValue(StatusKind.MoveSpeedBuff, now) - MaxValue(StatusKind.MoveSpeedSlow, now);
            return BaseMoveSpeed * Math.Max(0.2f, mult);
        }

        /// <summary>1 = full damage; Slow's damage reduction lowers it.</summary>
        public float DamageDealtMultiplier(float now) => Math.Max(0f, 1f - MaxValue(StatusKind.DamageDealtReduction, now));

        /// <summary>1 = full healing; Bio's healing reduction lowers it.</summary>
        public float HealingMultiplier(float now) => Math.Max(0f, 1f - MaxValue(StatusKind.HealingReduction, now));

        public float ShieldAmount(float now)
        {
            float total = 0f;
            for (int i = 0; i < Statuses.Count; i++)
                if (Statuses[i].Kind == StatusKind.Shield && Statuses[i].IsActive(now)) total += Statuses[i].Amount;
            return total;
        }

        public void RemoveStatuses(Predicate<Status> match) => Statuses.RemoveAll(match);

        /// <summary>Snapshots the current loadout into a fresh cooldown tracker (call after changing the loadout).</summary>
        public void RebuildSpells(GameMode mode)
        {
            Spells = new SummonerSpellTracker(Loadout, mode);
        }
    }
}
