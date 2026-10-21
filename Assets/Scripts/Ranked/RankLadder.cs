// Project D  |  Codename: Zantetsuken
// RankLadder.cs  (Ranked — the ordered ladder of tier+division steps)
//
// Builds the full climb from the lowest step (Bronze D) to the highest (Crystal A)
// per the design:
//   Bronze, Silver, Gold      -> divisions D, C, B, A          (100 LP each)
//   Platinum, Mythril         -> divisions E, D, C, B, A        (100 LP each)
//   Adamantite, Crystal       -> divisions E, D, C, B, A        ( 50 LP each, apex)
//
// A "global step" is an index 0..N-1 into this ladder; promotion = step+1,
// demotion = step-1. Each step carries its LP threshold and a target MMR anchor
// used by the LP/MMR math. Pure C# — deterministic and unit-testable.

using System.Collections.Generic;

namespace ProjectD.Ranked
{
    public readonly struct RankStep
    {
        public readonly RankTier Tier;
        public readonly Division Division;
        public readonly int LpThreshold;   // LP needed to promote out of this step
        public readonly int TargetMmr;     // MMR a player "should" have at this step
        public RankStep(RankTier tier, Division division, int lpThreshold, int targetMmr)
        { Tier = tier; Division = division; LpThreshold = lpThreshold; TargetMmr = targetMmr; }
    }

    public static class RankLadder
    {
        // Divisions per tier (ordered low -> high within the tier).
        private static readonly Division[] BronzeToGold = { Division.D, Division.C, Division.B, Division.A };
        private static readonly Division[] PlatToCrystal = { Division.E, Division.D, Division.C, Division.B, Division.A };

        // LP required to advance a division. Apex tiers advance in 50s.
        public const int StandardLpPerDivision = 100;
        public const int ApexLpPerDivision = 50;

        // MMR anchoring: linear from base, spread across every step.
        public const int BaseMmr = 800;      // Bronze D target
        public const int MmrPerStep = 40;     // per division step

        private static RankStep[] _steps;

        public static IReadOnlyList<RankStep> Steps
        {
            get { if (_steps == null) Build(); return _steps; }
        }

        public static int StepCount => Steps.Count;

        public static Division[] DivisionsFor(RankTier tier) => tier switch
        {
            RankTier.Bronze or RankTier.Silver or RankTier.Gold => BronzeToGold,
            _ => PlatToCrystal
        };

        public static int LpPerDivisionFor(RankTier tier)
            => (tier == RankTier.Adamantite || tier == RankTier.Crystal) ? ApexLpPerDivision : StandardLpPerDivision;

        public static RankStep StepAt(int globalStep)
        {
            var s = Steps;
            if (globalStep < 0) globalStep = 0;
            if (globalStep >= s.Count) globalStep = s.Count - 1;
            return s[globalStep];
        }

        public static int LowestStep => 0;
        public static int HighestStep => StepCount - 1;

        /// <summary>Find the global step index whose target MMR is nearest to a value.</summary>
        public static int NearestStepForMmr(int mmr)
        {
            var s = Steps;
            int best = 0, bestDist = int.MaxValue;
            for (int i = 0; i < s.Count; i++)
            {
                int d = System.Math.Abs(s[i].TargetMmr - mmr);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        private static void Build()
        {
            var list = new List<RankStep>();
            int step = 0;
            for (RankTier t = RankTier.Bronze; t <= RankTier.Crystal; t++)
            {
                var divs = DivisionsFor(t);
                int lp = LpPerDivisionFor(t);
                foreach (var d in divs)
                {
                    int mmr = BaseMmr + step * MmrPerStep;
                    list.Add(new RankStep(t, d, lp, mmr));
                    step++;
                }
            }
            _steps = list.ToArray();
        }
    }
}
