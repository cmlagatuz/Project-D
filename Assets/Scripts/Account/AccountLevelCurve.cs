// Project D  |  Codename: Zantetsuken
// AccountLevelCurve.cs  (Account — XP->level curve + per-level KP rewards)
//
// Defines how much XP each account level costs and how much KP a player earns on
// each level-up. KP is the OUT-OF-GAME currency (buys champions/borders/icons); it
// comes ONLY from account/profile progression, never from matches. Pure C# —
// deterministic + unit-testable so the server (source of truth) and client agree.

using System;

namespace ProjectD.Account
{
    public static class AccountLevelCurve
    {
        public const int MaxLevel = 300;      // soft cap; XP keeps flowing past it as "prestige"
        public const int BaseXpPerLevel = 600; // XP to go from level 1 -> 2
        public const int XpGrowthPerLevel = 120; // each level costs this much more than the last

        /// <summary>XP required to advance FROM the given level to the next.</summary>
        public static int XpToNext(int level)
        {
            if (level < 1) level = 1;
            // Linear ramp: cost(level) = base + growth*(level-1), capped past MaxLevel.
            int L = Math.Min(level, MaxLevel);
            return BaseXpPerLevel + XpGrowthPerLevel * (L - 1);
        }

        /// <summary>Total cumulative XP to REACH the given level from level 1.</summary>
        public static long TotalXpToReach(int level)
        {
            if (level <= 1) return 0;
            long sum = 0;
            for (int l = 1; l < level; l++) sum += XpToNext(l);
            return sum;
        }

        // ---- KP rewards per level-up ----
        public const int KpPerLevelBase = 60;      // KP granted for a normal level-up
        public const int KpMilestoneEvery = 10;    // every Nth level grants a bonus
        public const int KpMilestoneBonus = 250;   // bonus KP at milestone levels

        /// <summary>KP granted for reaching a given level (level-up reward).</summary>
        public static int KpRewardForReaching(int level)
        {
            if (level <= 1) return 0;
            int kp = KpPerLevelBase;
            if (level % KpMilestoneEvery == 0) kp += KpMilestoneBonus;
            return kp;
        }

        /// <summary>Resolve a level + leftover XP from a total XP amount (for migration/loads).</summary>
        public static (int level, int xpIntoLevel) LevelFromTotalXp(long totalXp)
        {
            int level = 1;
            long remaining = Math.Max(0, totalXp);
            while (true)
            {
                int need = XpToNext(level);
                if (remaining < need) return (level, (int)remaining);
                remaining -= need;
                level++;
                if (level >= MaxLevel && remaining >= XpToNext(level))
                {
                    // Past soft cap: keep leveling at the capped cost.
                }
            }
        }
    }
}
