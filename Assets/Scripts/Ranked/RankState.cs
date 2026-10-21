// Project D  |  Codename: Zantetsuken
// RankState.cs  (Ranked — a player's current standing)
//
// The player's displayed rank position plus hidden MMR and the small amount of
// bookkeeping the progression needs (demotion shield, placement status). Serializable
// so it can be persisted server-side and synced to the account (this is an online
// ranked game — the server owns this, the client mirrors it).

using System;

namespace ProjectD.Ranked
{
    [Serializable]
    public sealed class RankState
    {
        public int globalStep;          // index into RankLadder.Steps (0 = Bronze D)
        public int lp;                  // LP within the current step (0..threshold-1)
        public int mmr = RankLadder.BaseMmr;  // hidden matchmaking rating

        public bool placementsComplete = false;
        public int placementGamesPlayed = 0;
        public int placementWins = 0;

        // Demotion protection: games remaining where a loss at 0 LP won't demote.
        public int demotionShieldGames = 0;

        // Optional promo state (tier boundary best-of series). 0 = not in a series.
        public int promoWins = 0;
        public int promoLosses = 0;
        public bool inPromoSeries = false;

        // Season stats for end-of-season rewards.
        public int peakStep = 0;        // highest ladder step reached this season
        public int rankedWins = 0;      // decisive wins this season (for milestones)
        public int rankedGames = 0;     // decisive games this season

        /// <summary>Raise the season peak if the current step is higher.</summary>
        public void UpdatePeak()
        {
            if (globalStep > peakStep) peakStep = globalStep;
        }

        public RankStep PeakStepInfo => RankLadder.StepAt(peakStep);

        public RankStep Step => RankLadder.StepAt(globalStep);
        public RankTier Tier => Step.Tier;
        public Division Division => Step.Division;
        public int LpThreshold => Step.LpThreshold;

        public bool IsApex => Tier == RankTier.Adamantite || Tier == RankTier.Crystal;
        public bool AtLadderFloor => globalStep <= RankLadder.LowestStep;
        public bool AtLadderTop => globalStep >= RankLadder.HighestStep;

        public RankState Clone() => (RankState)MemberwiseClone();

        /// <summary>A fresh unranked player (before placements).</summary>
        public static RankState NewUnranked(int seedMmr = RankLadder.BaseMmr)
            => new RankState { globalStep = 0, lp = 0, mmr = seedMmr, placementsComplete = false };
    }
}
