// Project D  |  Codename: Zantetsuken
// MmrModel.cs  (Ranked — hidden matchmaking rating, Elo-based)
//
// The hidden skill number that drives matchmaking and shapes LP gains. Uses the
// standard Elo expected-score formula and a K-factor that shrinks at higher tiers
// (so apex ratings are stable). Pure C# — deterministic and unit-testable.

using System;

namespace ProjectD.Ranked
{
    public static class MmrModel
    {
        /// <summary>Expected win probability of a player vs an opponent (or lobby avg) MMR.</summary>
        public static double ExpectedScore(int playerMmr, int opponentMmr)
            => 1.0 / (1.0 + Math.Pow(10.0, (opponentMmr - playerMmr) / 400.0));

        /// <summary>K-factor by tier: high early (fast calibration), low at apex (stable).</summary>
        public static int KFactor(RankTier tier, bool provisional)
        {
            if (provisional) return 60;           // placements calibrate quickly
            return tier switch
            {
                RankTier.Bronze or RankTier.Silver => 32,
                RankTier.Gold or RankTier.Platinum => 28,
                RankTier.Mythril => 24,
                RankTier.Adamantite => 20,
                RankTier.Crystal => 16,
                _ => 28
            };
        }

        /// <summary>New MMR after a match. actualScore: 1 win, 0 loss, 0.5 draw/remake.</summary>
        public static int Update(int playerMmr, int opponentMmr, double actualScore, int k)
        {
            double expected = ExpectedScore(playerMmr, opponentMmr);
            double delta = k * (actualScore - expected);
            int next = playerMmr + (int)Math.Round(delta);
            return Math.Max(0, next);
        }

        /// <summary>Convenience result score for a MatchResult.</summary>
        public static double ScoreOf(MatchResult r) => r switch
        {
            MatchResult.Win => 1.0,
            MatchResult.Loss => 0.0,
            _ => 0.5   // remake / draw
        };
    }
}
