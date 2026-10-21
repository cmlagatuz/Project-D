// Project D  |  Codename: Zantetsuken
// RankProgression.cs  (Ranked — apply a match result to a RankState)
//
// The authoritative state machine: takes a RankState + match result + lobby MMR,
// computes LP/MMR (via LpCalculator), then moves the player up/down the ladder:
//   - LP >= threshold      -> PROMOTE to the next step (overflow LP carries over),
//                             grants a demotion shield of a few games.
//   - LP < 0               -> at 0 LP, demotion is blocked while a shield remains;
//                             otherwise DEMOTE to the previous step near its top.
//   - Ladder floor/ceiling -> clamped (no demote below Bronze D, no promote past Crystal A).
//
// Emits a RankChange describing what happened so the UI can show a rank-up/down
// screen. Pure C# — deterministic, server-authoritative, unit-testable.

using System;

namespace ProjectD.Ranked
{
    public enum RankChangeKind { None, LpOnly, Promotion, Demotion, TierPromotion, TierDemotion, Placed }

    public struct RankChange
    {
        public RankChangeKind Kind;
        public int LpDelta;
        public int MmrDelta;
        public RankStep Before;
        public RankStep After;
        public int LpAfter;
        public bool CrossedTier;
    }

    public static class RankProgression
    {
        /// <summary>Games of demotion protection granted immediately after a promotion.</summary>
        public const int DemotionShieldOnPromote = 3;

        public static RankChange Apply(RankState state, MatchResult result, int lobbyAvgOpponentMmr)
        {
            var before = state.Step;
            int beforeStepIndex = state.globalStep;

            var lp = LpCalculator.Evaluate(state, result, lobbyAvgOpponentMmr);
            state.mmr = lp.NewMmr;

            // Tick down the demotion shield on any decisive game.
            if (result != MatchResult.Remake && state.demotionShieldGames > 0)
                state.demotionShieldGames--;

            if (lp.LpDelta == 0)
                return Result(RankChangeKind.LpOnly, lp, before, state, false);

            state.lp += lp.LpDelta;

            // ---- PROMOTION (may cascade across multiple divisions on a big overflow) ----
            while (state.lp >= state.LpThreshold && !state.AtLadderTop)
            {
                state.lp -= state.LpThreshold;
                state.globalStep++;
                state.demotionShieldGames = DemotionShieldOnPromote;
            }
            if (state.AtLadderTop && state.lp >= state.LpThreshold)
                state.lp = state.LpThreshold; // apex cap; extra LP held at threshold

            // ---- DEMOTION ----
            while (state.lp < 0)
            {
                if (state.AtLadderFloor) { state.lp = 0; break; }

                // Demotion protection: hold at 0 LP while a shield remains.
                if (state.demotionShieldGames > 0) { state.lp = 0; break; }

                state.globalStep--;
                // Drop into the previous step near its top (a few LP of breathing room).
                int prevThreshold = state.LpThreshold;
                state.lp += prevThreshold;                // carry the negative overflow up
                state.lp = Math.Min(state.lp, prevThreshold - 1);
                state.lp = Math.Max(state.lp, Math.Max(0, prevThreshold - 25)); // land ~75/100 or ~25/50
            }

            bool crossedTier = state.Step.Tier != before.Tier;
            RankChangeKind kind;
            if (state.globalStep > beforeStepIndex)
                kind = crossedTier ? RankChangeKind.TierPromotion : RankChangeKind.Promotion;
            else if (state.globalStep < beforeStepIndex)
                kind = crossedTier ? RankChangeKind.TierDemotion : RankChangeKind.Demotion;
            else
                kind = RankChangeKind.LpOnly;

            return Result(kind, lp, before, state, crossedTier);
        }

        private static RankChange Result(RankChangeKind kind, LpResult lp, RankStep before, RankState state, bool crossedTier)
            => new RankChange
            {
                Kind = kind,
                LpDelta = lp.LpDelta,
                MmrDelta = lp.MmrDelta,
                Before = before,
                After = state.Step,
                LpAfter = state.lp,
                CrossedTier = crossedTier
            };
    }
}
