// Project D  |  Codename: Zantetsuken
// PlacementService.cs  (Ranked — placement matches -> initial rank)
//
// New/seasonal players play a set of PLACEMENT games at a high (provisional) K so
// MMR calibrates fast, then get seeded onto the ladder at the step nearest their
// calibrated MMR (a touch below, so there's immediate climb). During placements the
// displayed rank is hidden; only MMR moves. Pure C# — server-authoritative + testable.

using System;

namespace ProjectD.Ranked
{
    public static class PlacementService
    {
        public const int PlacementGames = 7;         // matches before a rank is shown
        public const int SeedStepOffset = 1;         // seed one step BELOW calibrated MMR

        /// <summary>Record a placement game (MMR-only). Returns true once placements finish.</summary>
        public static bool RecordPlacement(RankState state, MatchResult result, int lobbyAvgOpponentMmr)
        {
            if (state.placementsComplete) return true;

            int k = MmrModel.KFactor(RankTier.Gold, provisional: true); // tier irrelevant while provisional
            double score = MmrModel.ScoreOf(result);
            state.mmr = MmrModel.Update(state.mmr, lobbyAvgOpponentMmr, score, k);

            if (result != MatchResult.Remake)
            {
                state.placementGamesPlayed++;
                if (result == MatchResult.Win) state.placementWins++;
            }

            if (state.placementGamesPlayed >= PlacementGames)
            {
                Finalize(state);
                return true;
            }
            return false;
        }

        /// <summary>Seed the visible rank from the calibrated MMR.</summary>
        public static RankChange Finalize(RankState state)
        {
            var before = state.Step;

            int nearest = RankLadder.NearestStepForMmr(state.mmr);
            int seeded = Math.Max(RankLadder.LowestStep, nearest - SeedStepOffset);
            state.globalStep = seeded;
            // Land around the middle of the seeded division.
            state.lp = Math.Min(state.LpThreshold - 1, state.LpThreshold / 2);
            state.placementsComplete = true;
            state.demotionShieldGames = RankProgression.DemotionShieldOnPromote;

            return new RankChange
            {
                Kind = RankChangeKind.Placed,
                LpDelta = 0, MmrDelta = 0,
                Before = before, After = state.Step, LpAfter = state.lp,
                CrossedTier = state.Step.Tier != before.Tier
            };
        }
    }
}
