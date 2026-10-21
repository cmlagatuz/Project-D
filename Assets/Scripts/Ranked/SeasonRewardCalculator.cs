// Project D  |  Codename: Zantetsuken
// SeasonRewardCalculator.cs  (Ranked — resolve end-of-season rewards)
//
// Given a player's finished-season RankState + a reward config, produces the list of
// rewards they earned: the tier emblem(s) up to their PEAK tier, any win-milestone
// bundles, and a summary line. Server-authoritative + engine-agnostic (works on a
// tiny reward view so it needs no UnityEngine and is unit-testable).

using System.Collections.Generic;

namespace ProjectD.Ranked
{
    // Engine-agnostic view of a reward bundle so the calculator has no UnityEngine dep.
    public struct RewardView
    {
        public string EmblemId;
        public string BannerId;
        public int HonorPoints;
        public string[] BonusCosmeticIds;
        public string DisplayName;
        public string NameKey;
        public RankTier Tier;       // Tier this reward is associated with (or Bronze for milestones)
        public bool IsMilestone;    // true for win-milestone rewards
        public int MilestoneWins;   // for milestone rewards
    }

    public struct SeasonRewardResult
    {
        public bool Eligible;          // met the minimum games for the peak tier
        public RankTier PeakTier;
        public string PeakRankLabel;   // e.g. "Adamantite C"
        public int TotalHonor;
        public List<RewardView> Rewards;
    }

    // Config provider abstraction so tests can feed data without ScriptableObjects.
    public interface IRewardConfig
    {
        bool CumulativeUpToPeak { get; }
        bool TryGetTierReward(RankTier tier, out RewardView view, out int minGames);
        IReadOnlyList<(int wins, RewardView view)> WinMilestones { get; }
    }

    public static class SeasonRewardCalculator
    {
        public static SeasonRewardResult Evaluate(RankState finishedSeason, IRewardConfig config)
        {
            var result = new SeasonRewardResult { Rewards = new List<RewardView>() };
            if (finishedSeason == null || config == null) return result;

            // Placements never completed -> no ranked reward (still may get milestones).
            RankTier peakTier = finishedSeason.placementsComplete
                ? finishedSeason.PeakStepInfo.Tier
                : RankTier.Bronze;

            result.PeakTier = peakTier;
            result.PeakRankLabel = $"{RankFormatter.TierName(peakTier)} {finishedSeason.PeakStepInfo.Division}";

            // ---- tier emblem(s) ----
            if (finishedSeason.placementsComplete)
            {
                if (config.CumulativeUpToPeak)
                {
                    // Grant each tier reward from Bronze up to (and including) the peak tier.
                    for (RankTier t = RankTier.Bronze; t <= peakTier; t++)
                        AddTierReward(config, t, finishedSeason, result);
                }
                else
                {
                    AddTierReward(config, peakTier, finishedSeason, result);
                }
            }

            // Eligibility = earned at least the peak-tier emblem (met min games).
            result.Eligible = result.Rewards.Count > 0;

            // ---- win milestones (independent of rank) ----
            foreach (var (wins, view) in config.WinMilestones)
            {
                if (finishedSeason.rankedWins >= wins)
                {
                    var v = view; v.IsMilestone = true; v.MilestoneWins = wins;
                    result.Rewards.Add(v);
                    result.TotalHonor += v.HonorPoints;
                }
            }

            return result;
        }

        private static void AddTierReward(IRewardConfig config, RankTier tier, RankState s, SeasonRewardResult result)
        {
            if (!config.TryGetTierReward(tier, out var view, out int minGames)) return;
            // Higher tiers may require a minimum number of ranked games to earn the emblem.
            if (s.rankedGames < minGames) return;
            view.Tier = tier;
            result.Rewards.Add(view);
            result.TotalHonor += view.HonorPoints;
        }
    }
}
