// Project D  |  Codename: Zantetsuken
// RankedSeason.cs  (Ranked — seasons + soft reset for the online ladder)
//
// An online ranked ladder runs in seasons. At a new season the visible rank resets
// but MMR "soft resets" toward the middle so returning players re-place near their
// true skill instead of grinding from Bronze. Pure C# — server-authoritative.

using System;

namespace ProjectD.Ranked
{
    [Serializable]
    public sealed class SeasonInfo
    {
        public int seasonId;
        public string name = "Season 1";
        public long startUnix;     // season window (server clock)
        public long endUnix;
    }

    public static class RankedSeason
    {
        /// <summary>MMR pull toward the midpoint on soft reset (0 = none, 1 = full reset).</summary>
        public const double SoftResetPull = 0.35;

        /// <summary>The neutral MMR the soft reset pulls toward (roughly Gold/Platinum).</summary>
        public static int MidpointMmr => RankLadder.StepAt(RankLadder.StepCount / 2).TargetMmr;

        /// <summary>
        /// Produce a fresh state for a new season: MMR soft-reset, placements required
        /// again, LP/rank cleared. Carries a fraction of last season's MMR skill.
        /// </summary>
        public static RankState BeginNewSeason(RankState previous)
        {
            int prevMmr = previous != null ? previous.mmr : MidpointMmr;
            int mid = MidpointMmr;
            int softMmr = (int)Math.Round(prevMmr + (mid - prevMmr) * SoftResetPull);

            return new RankState
            {
                globalStep = 0,
                lp = 0,
                mmr = softMmr,
                placementsComplete = false,
                placementGamesPlayed = 0,
                placementWins = 0,
                demotionShieldGames = 0
            };
        }
    }
}
