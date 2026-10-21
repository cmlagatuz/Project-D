// Project D  |  Codename: Zantetsuken
// RankBadge.cs  (Ranked UI — a compact rank display)
//
// Shows a player's current tier + division + LP with a tier-colored frame and an
// LP progress bar. Used on the profile, the queue screen, the loading screen, and
// the scoreboard. Bind a RankState (or subscribe to RankService.OnRankChanged).

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;

namespace ProjectD.RankedUI
{
    public sealed class RankBadge : MonoBehaviour
    {
        [SerializeField] private Image frame;          // tier-colored border
        [SerializeField] private Image emblem;         // tier emblem (art optional)
        [SerializeField] private Text tierText;        // "Gold A" (or localized tier)
        [SerializeField] private Text lpText;          // "75 / 100 LP" or "Placement 3/7"
        [SerializeField] private Image lpFill;         // progress bar (Filled Horizontal)

        public void Bind(RankState state)
        {
            if (state == null) return;

            var color = ToColor(RankFormatter.TierColor(state.Tier));
            if (frame) frame.color = color;
            if (emblem && emblem.sprite == null) emblem.color = color;

            if (!state.placementsComplete)
            {
                if (tierText) tierText.text = "Unranked";
                if (lpText) lpText.text = RankFormatter.PlacementProgress(state);
                if (lpFill) lpFill.fillAmount = PlacementService.PlacementGames == 0 ? 0f
                    : (float)state.placementGamesPlayed / PlacementService.PlacementGames;
                return;
            }

            if (tierText) tierText.text = RankFormatter.Short(state);
            if (lpText) lpText.text = RankFormatter.LpOfThreshold(state);
            if (lpFill) lpFill.fillAmount = RankFormatter.StepProgress(state);
        }

        public static Color ToColor(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
