// Project D  |  Codename: Zantetsuken
// AccountProgression.cs  (Account — XP/level/KP state + level-up logic)
//
// The player's persistent profile progression: level, XP into the current level, KP
// balance, and lifetime totals. AddXp advances the level (possibly several at once
// on a big grant) and accumulates the KP earned from each level-up. Server-
// authoritative + engine-agnostic (no UnityEngine) so it's unit-testable and the
// account backend owns the truth.

using System;
using System.Collections.Generic;

namespace ProjectD.Account
{
    [Serializable]
    public sealed class AccountProgression
    {
        public int level = 1;
        public int xpIntoLevel = 0;
        public long lifetimeXp = 0;
        public long kp = 0;              // spendable KP balance
        public long lifetimeKp = 0;      // total KP ever earned (for stats)

        public int XpToNext => AccountLevelCurve.XpToNext(level);
        public float LevelProgress => XpToNext <= 0 ? 0f : (float)xpIntoLevel / XpToNext;

        public AccountProgression Clone() => (AccountProgression)MemberwiseClone();
    }

    // Result of applying an XP grant (for the level-up screen).
    public struct XpGrantResult
    {
        public int XpGained;
        public int StartLevel;
        public int EndLevel;
        public int LevelsGained;
        public int KpGained;             // KP from all level-ups in this grant
        public bool LeveledUp => LevelsGained > 0;
        public List<int> LevelsReached; // e.g. [24, 25] for a double level-up
    }

    public static class AccountProgressionService
    {
        /// <summary>Add XP (e.g. post-match reward) and grant KP for any level-ups.</summary>
        public static XpGrantResult AddXp(AccountProgression p, int xp)
        {
            var result = new XpGrantResult
            {
                XpGained = Math.Max(0, xp),
                StartLevel = p.level,
                LevelsReached = new List<int>()
            };
            if (xp <= 0) { result.EndLevel = p.level; return result; }

            p.lifetimeXp += xp;
            p.xpIntoLevel += xp;

            int kpGained = 0;
            while (p.xpIntoLevel >= p.XpToNext)
            {
                p.xpIntoLevel -= p.XpToNext;
                p.level++;
                int kp = AccountLevelCurve.KpRewardForReaching(p.level);
                kpGained += kp;
                result.LevelsReached.Add(p.level);
            }

            p.kp += kpGained;
            p.lifetimeKp += kpGained;

            result.EndLevel = p.level;
            result.LevelsGained = p.level - result.StartLevel;
            result.KpGained = kpGained;
            return result;
        }

        /// <summary>Spend KP (Store purchase). Returns false if insufficient.</summary>
        public static bool SpendKp(AccountProgression p, long amount)
        {
            if (amount < 0 || p.kp < amount) return false;
            p.kp -= amount;
            return true;
        }

        /// <summary>Grant KP directly (e.g. event/mission reward, refunds).</summary>
        public static void GrantKp(AccountProgression p, long amount)
        {
            if (amount <= 0) return;
            p.kp += amount;
            p.lifetimeKp += amount;
        }
    }
}
