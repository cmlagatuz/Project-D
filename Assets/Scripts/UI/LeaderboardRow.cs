// Project D  |  Codename: Zantetsuken
// LeaderboardRow.cs  (Leaderboard UI — one row on the apex board)
//
// Displays a ranked player's standing: rank #, name (+ friend/self accent), a small
// rank badge (tier color + tier/division), LP, most-played champion + lane, region,
// and win-rate. Bind a RankedRow; a "highlight" flag marks the viewer's own row.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;
using ProjectD.Leaderboard;
using ProjectD.RankedUI;

namespace ProjectD.LeaderboardUI
{
    public sealed class LeaderboardRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text rankText;
        [SerializeField] private Text nameText;
        [SerializeField] private Image tierSwatch;
        [SerializeField] private Text tierText;        // "Crystal A"
        [SerializeField] private Text lpText;          // "45 LP"
        [SerializeField] private Text championText;    // champion id / name
        [SerializeField] private Text laneText;        // "Jungle"
        [SerializeField] private Text regionText;      // "NA"
        [SerializeField] private Text winRateText;     // "62% (50-30)"
        [SerializeField] private Image friendIcon;

        public void Bind(RankedRow row, bool isViewer)
        {
            var e = row.Entry;
            if (rankText) rankText.text = "#" + row.Rank;
            if (nameText) { nameText.text = e.displayName; nameText.color = isViewer ? new Color(0.55f,0.85f,1f) : (e.isFriend ? new Color(0.6f,0.9f,0.6f) : Color.white); }

            if (tierSwatch) tierSwatch.color = RankBadge.ToColor(RankFormatter.TierColor(e.tier));
            if (tierText) tierText.text = $"{RankFormatter.TierName(e.tier)} {e.division}";
            if (lpText) lpText.text = $"{e.lp} LP";
            if (championText) championText.text = e.topChampionId;
            if (laneText) laneText.text = e.mainLane == Lane.Any ? "-" : e.mainLane.ToString();
            if (regionText) regionText.text = e.region;
            if (winRateText) winRateText.text = $"{Mathf.RoundToInt(e.WinRate * 100)}% ({e.wins}-{e.losses})";
            if (friendIcon) friendIcon.enabled = e.isFriend && !isViewer;

            if (background)
                background.color = isViewer ? new Color(0.20f,0.28f,0.42f,0.95f)
                    : (row.Rank % 2 == 0 ? new Color(0.12f,0.14f,0.20f,0.9f) : new Color(0.14f,0.16f,0.23f,0.9f));
        }
    }
}
