// Project D  |  Codename: Zantetsuken
// RewardRow.cs  (Ranked UI — a single earned-reward line on the rewards screen)
//
// Displays one reward: an emblem/icon swatch (tier-colored until art), the reward
// name, and its honor value. Milestone rewards show a "wins" tag; tier rewards show
// the tier. Art (emblem sprites) is looked up by id in a real build; here the swatch
// uses the tier color so the screen reads without assets.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;

namespace ProjectD.RankedUI
{
    public sealed class RewardRow : MonoBehaviour
    {
        [SerializeField] private Image swatch;
        [SerializeField] private Text nameText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text honorText;

        public void Bind(RewardView reward)
        {
            if (nameText) nameText.text = string.IsNullOrEmpty(reward.DisplayName)
                ? (reward.IsMilestone ? $"{reward.MilestoneWins} Ranked Wins" : $"{RankFormatter.TierName(reward.Tier)} Emblem")
                : reward.DisplayName;

            if (detailText)
            {
                if (reward.IsMilestone) detailText.text = "Win milestone";
                else
                {
                    int bonus = reward.BonusCosmeticIds != null ? reward.BonusCosmeticIds.Length : 0;
                    detailText.text = bonus > 0 ? $"{RankFormatter.TierName(reward.Tier)} - +{bonus} bonus" : RankFormatter.TierName(reward.Tier);
                }
            }

            if (honorText) honorText.text = reward.HonorPoints > 0 ? $"+{reward.HonorPoints}" : "";

            if (swatch)
            {
                // Tier reward -> tier color; milestone -> neutral gold.
                uint rgb = reward.IsMilestone ? 0xE6B422u : RankFormatter.TierColor(reward.Tier);
                swatch.color = RankBadge.ToColor(rgb);
            }
        }

        public void SetVisible(bool v) => gameObject.SetActive(v);
    }
}
