// Project D  |  Codename: Zantetsuken
// SeasonRewardData.cs  (Ranked — end-of-season reward definitions)
//
// Defines what a player earns at season end based on the HIGHEST tier they reached
// that season (peak rank). Rewards are cosmetic + progression currency only (no
// combat power) per the monetization principles: an animated tier EMBLEM/border, a
// profile banner, honor points, and optional bonus loot at higher tiers. Data-driven
// so live-ops can tune per season without code (GCD Appendix A).

using System;
using UnityEngine;

namespace ProjectD.Ranked
{
    // What a single reward line grants.
    [Serializable]
    public sealed class RewardBundle
    {
        [Tooltip("Cosmetic emblem/border id shown on the profile + loading screen.")]
        public string emblemId;
        [Tooltip("Profile banner/frame id.")]
        public string bannerId;
        [Tooltip("Honor / progression currency (not combat power).")]
        public int honorPoints;
        [Tooltip("Optional bonus cosmetic loot ids (e.g. an icon, a recall effect).")]
        public string[] bonusCosmeticIds = new string[0];
        [Tooltip("Human label for the reward screen, e.g. 'Gold Season Emblem'.")]
        public string displayName;
        [Tooltip("Localization key for the reward name, e.g. reward.emblem.gold.")]
        public string nameKey;
    }

    // One reward row per tier; the player receives everything up to their peak tier
    // (inclusive) unless cumulativeUpToPeak is false (then only the peak tier's row).
    [Serializable]
    public sealed class TierReward
    {
        public RankTier tier;
        public RewardBundle bundle = new RewardBundle();
        [Tooltip("Minimum games required to be eligible for this tier's reward.")]
        public int minRankedGames = 5;
    }

    [CreateAssetMenu(fileName = "SeasonRewards", menuName = "Project D/Season Reward Config")]
    public sealed class SeasonRewardConfig : ScriptableObject
    {
        public int seasonId = 1;
        public string seasonName = "Season 1";

        [Tooltip("If true, grant every tier reward up to and including the peak tier.")]
        public bool cumulativeUpToPeak = true;

        [Tooltip("Reward rows, one per tier (Bronze..Crystal).")]
        public TierReward[] tierRewards = new TierReward[0];

        [Header("Win milestone bonuses (independent of rank)")]
        [Tooltip("Wins needed for each milestone, ascending (e.g. 25, 50, 100).")]
        public int[] winMilestones = { 25, 50, 100 };
        [Tooltip("Reward bundle per milestone (index-aligned with winMilestones).")]
        public RewardBundle[] winMilestoneRewards = new RewardBundle[0];

        public TierReward RewardFor(RankTier tier)
        {
            foreach (var r in tierRewards) if (r != null && r.tier == tier) return r;
            return null;
        }
    }
}
