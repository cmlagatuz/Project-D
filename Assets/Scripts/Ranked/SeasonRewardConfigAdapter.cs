// Project D  |  Codename: Zantetsuken
// SeasonRewardConfigAdapter.cs  (Ranked — bridge the SO config to the calculator)
//
// Wraps a SeasonRewardConfig ScriptableObject as the engine-agnostic IRewardConfig
// the calculator consumes, so the reward math stays UnityEngine-free and testable.

using System.Collections.Generic;

namespace ProjectD.Ranked
{
    public sealed class SeasonRewardConfigAdapter : IRewardConfig
    {
        private readonly SeasonRewardConfig _config;

        public SeasonRewardConfigAdapter(SeasonRewardConfig config) { _config = config; }

        public bool CumulativeUpToPeak => _config != null && _config.cumulativeUpToPeak;

        public bool TryGetTierReward(RankTier tier, out RewardView view, out int minGames)
        {
            view = default; minGames = 0;
            var r = _config != null ? _config.RewardFor(tier) : null;
            if (r == null) return false;
            minGames = r.minRankedGames;
            view = ToView(r.bundle, tier, false, 0);
            return true;
        }

        public IReadOnlyList<(int wins, RewardView view)> WinMilestones
        {
            get
            {
                var list = new List<(int, RewardView)>();
                if (_config == null || _config.winMilestones == null) return list;
                for (int i = 0; i < _config.winMilestones.Length; i++)
                {
                    var bundle = (_config.winMilestoneRewards != null && i < _config.winMilestoneRewards.Length)
                        ? _config.winMilestoneRewards[i] : null;
                    if (bundle == null) continue;
                    list.Add((_config.winMilestones[i], ToView(bundle, RankTier.Bronze, true, _config.winMilestones[i])));
                }
                return list;
            }
        }

        private static RewardView ToView(RewardBundle b, RankTier tier, bool milestone, int wins)
            => new RewardView
            {
                EmblemId = b.emblemId,
                BannerId = b.bannerId,
                HonorPoints = b.honorPoints,
                BonusCosmeticIds = b.bonusCosmeticIds ?? new string[0],
                DisplayName = b.displayName,
                NameKey = b.nameKey,
                Tier = tier,
                IsMilestone = milestone,
                MilestoneWins = wins
            };
    }
}
