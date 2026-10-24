// Project D | Codename: Zantetsuken — SmiteProgress.cs
// Ultima (the Smite-style spell): damage tiers, the Hunter's Resource stack economy and the numbers around them.
// Engine-agnostic and deterministic. Numbers marked "placeholder" are tunable guesses.
using System;

namespace ProjectD.SummonerSpells
{
    public enum SmiteTarget { Minion, Monster, Champion }

    public static class SmiteMath
    {
        public const float ChampionDamage = 40f;            // true damage, upgraded Ultima on a champion
        public const float ChampionSpeedSteal = 0.25f;      // fraction of the target's movement speed
        public const float ChampionSpeedStealSeconds = 2f;
        public const float MonsterDamageMultiplier = 1.10f; // +10% against monsters
        public const float CampBurnSeconds = 2f;            // area burn after attacking a monster

        /// <summary>Tier 0 = 600, tier 1 = 1000, tier 2 = 1400.</summary>
        public static float BaseDamage(int tier)
        {
            switch (Math.Max(0, Math.Min(2, tier)))
            {
                case 0: return 600f;
                case 1: return 1000f;
                default: return 1400f;
            }
        }

        /// <summary>
        /// Damage of one cast. Minions take the base; monsters take +10%. Champions can only be hit by an upgraded
        /// Ultima (tier 1+) and take a flat amount; below that this returns 0.
        /// </summary>
        public static float DamageVs(SmiteTarget target, int tier)
        {
            switch (target)
            {
                case SmiteTarget.Minion: return BaseDamage(tier);
                case SmiteTarget.Monster: return BaseDamage(tier) * MonsterDamageMultiplier;
                case SmiteTarget.Champion: return tier >= 1 ? ChampionDamage : 0f;
                default: return 0f;
            }
        }

        /// <summary>Per-second burn on a camp after attacking it: 22 at level 1 (growth per level is a placeholder) plus bonus-stat scaling.</summary>
        public static float CampBurnPerSecond(int level, float bonusAd, float bonusAp, float bonusArmor, float bonusMagicResist, float bonusHealth)
        {
            int lv = Math.Max(1, Math.Min(SummonerSpellCatalog.MaxLevel, level));
            float flat = 22f + 4f * (lv - 1);   // placeholder growth
            return flat + 0.10f * bonusAd + 0.12f * bonusAp + 0.25f * bonusArmor + 0.25f * bonusMagicResist + 0.04f * bonusHealth;
        }

        /// <summary>Health restored per second while attacking monsters: 5 at level 1 up to 35 at level 15.</summary>
        public static float HealPerSecondVsMonsters(int level)
        {
            int lv = Math.Max(1, Math.Min(SummonerSpellCatalog.MaxLevel, level));
            return 5f + 30f * (lv - 1) / (SummonerSpellCatalog.MaxLevel - 1);
        }

        /// <summary>Fully upgraded (tier 2) only: +10% out of combat, +5% in combat, while in the jungle or river.</summary>
        public static float JungleMoveSpeedBonus(int tier, bool inJungleOrRiver, bool inCombat)
        {
            if (tier < 2 || !inJungleOrRiver) return 0f;
            return inCombat ? 0.05f : 0.10f;
        }
    }

    /// <summary>One player's Hunter's Resource stacks and Ultima upgrade tier.</summary>
    public sealed class SmiteProgress
    {
        public const int FirstUpgradeConsumed = 8;
        public const int SecondUpgradeConsumed = 20;
        public const float BonusGoldPerStack = 40f;
        public const float StackIntervalSeconds = 30f;   // placeholder: how often a stack is earned
        public const int MaxStoredStacks = 4;            // placeholder: stacks cannot be stockpiled forever

        private int _stacks;
        private int _consumed;
        private float _nextStackAt;

        public SmiteProgress(float startSeconds = 0f)
        {
            _nextStackAt = startSeconds + StackIntervalSeconds;
        }

        public int StacksConsumed => _consumed;

        public int Tier => _consumed >= SecondUpgradeConsumed ? 2 : _consumed >= FirstUpgradeConsumed ? 1 : 0;

        /// <summary>Upgraded Ultima may target champions.</summary>
        public bool CanTargetChampions => Tier >= 1;

        public int Stacks(float now)
        {
            Accrue(now);
            return _stacks;
        }

        /// <summary>Call when a monster camp is cleared. Spends one stack for bonus gold; returns the gold (0 if none).</summary>
        public float OnCampCleared(float now)
        {
            Accrue(now);
            if (_stacks <= 0) return 0f;
            _stacks--;
            _consumed++;
            return BonusGoldPerStack;
        }

        private void Accrue(float now)
        {
            while (now >= _nextStackAt)
            {
                if (_stacks < MaxStoredStacks) _stacks++;
                _nextStackAt += StackIntervalSeconds;
            }
        }
    }
}
