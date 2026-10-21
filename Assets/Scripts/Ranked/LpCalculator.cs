// Project D  |  Codename: Zantetsuken
// LpCalculator.cs  (Ranked — LP gained/lost per match, MMR-aligned)
//
// LP change is a base value shaped by how the player's hidden MMR compares to the
// target MMR of their DISPLAYED rank step:
//   - MMR far ABOVE the rank  -> gain more, lose less  (climb quickly to true skill)
//   - MMR far BELOW the rank   -> gain less, lose more   (settle down to true skill)
//   - aligned                  -> roughly symmetric ~base LP
// Also factors the lobby-average opponent MMR via the Elo expectation, so beating a
// stronger lobby is worth a little more. Pure C# — unit-testable.

using System;

namespace ProjectD.Ranked
{
    public struct LpResult
    {
        public int LpDelta;        // signed LP change applied to the step
        public int MmrDelta;       // signed MMR change
        public int NewMmr;         // resulting MMR
    }

    public static class LpCalculator
    {
        public const int BaseWin = 22;
        public const int BaseLoss = 18;
        public const int MinGain = 10, MaxGain = 35;
        public const int MinLoss = 8, MaxLoss = 30;

        // How strongly MMR-vs-rank misalignment bends LP (per 100 MMR of gap).
        public const double AlignmentPer100Mmr = 6.0;
        public const int AlignmentCap = 12;

        /// <summary>
        /// Compute LP + MMR change for a match. lobbyAvgOpponentMmr is the average MMR
        /// of the enemy team (or overall lobby) for the Elo expectation.
        /// </summary>
        public static LpResult Evaluate(RankState state, MatchResult result, int lobbyAvgOpponentMmr)
        {
            bool provisional = !state.placementsComplete;
            int k = MmrModel.KFactor(state.Tier, provisional);
            double score = MmrModel.ScoreOf(result);

            int newMmr = MmrModel.Update(state.mmr, lobbyAvgOpponentMmr, score, k);
            int mmrDelta = newMmr - state.mmr;

            // Remakes don't move LP.
            if (result == MatchResult.Remake)
                return new LpResult { LpDelta = 0, MmrDelta = mmrDelta, NewMmr = newMmr };

            // Alignment: positive when the player's MMR outranks their displayed step.
            int rankTargetMmr = state.Step.TargetMmr;
            double gapHundreds = (state.mmr - rankTargetMmr) / 100.0;
            int alignment = (int)Math.Round(gapHundreds * AlignmentPer100Mmr);
            alignment = Math.Max(-AlignmentCap, Math.Min(AlignmentCap, alignment));

            int lpDelta;
            if (result == MatchResult.Win)
            {
                lpDelta = Clamp(BaseWin + alignment, MinGain, MaxGain);
            }
            else // Loss: alignment above rank REDUCES the loss, below rank INCREASES it.
            {
                int loss = BaseLoss - alignment;
                lpDelta = -Clamp(loss, MinLoss, MaxLoss);
            }

            return new LpResult { LpDelta = lpDelta, MmrDelta = mmrDelta, NewMmr = newMmr };
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
