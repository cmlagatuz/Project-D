// Project D  |  Codename: Zantetsuken
// ChampionLeaderRow.cs  (Leaderboard UI — one row on the per-champion board)
//
// Two layouts from one component:
//   - "Server No.1" grid mode: champion portrait + the #1 player's name/rank/rating.
//   - "Champion focus" list mode: rank number + player + win-rate on one champion.
// Bind a ChampionTopEntry; a #1 gets a gold crown accent.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.LeaderboardX;

namespace ProjectD.LeaderboardXUI
{
    public sealed class ChampionLeaderRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image championSwatch;
        [SerializeField] private Text championText;    // champion id/name (grid mode)
        [SerializeField] private Text rankNumberText;  // "#1" (list mode)
        [SerializeField] private Text playerText;
        [SerializeField] private Text ratingText;      // "1,920"
        [SerializeField] private Text subText;         // rank label / win-rate
        [SerializeField] private GameObject crown;

        public string ChampionId { get; private set; }
        public string PlayerId { get; private set; }
        public event System.Action<string> OnClicked;

        public void Bind(ChampionTopEntry e, bool showChampion)
        {
            ChampionId = e.ChampionId; PlayerId = e.Entry.PlayerId;
            if (championSwatch && championSwatch.sprite == null) championSwatch.color = Tint(e.ChampionId);
            if (championText) { championText.gameObject.SetActive(showChampion); championText.text = e.ChampionId; }
            if (rankNumberText) { rankNumberText.gameObject.SetActive(!showChampion); rankNumberText.text = $"#{e.Rank}"; }
            if (playerText) playerText.text = string.IsNullOrEmpty(e.Entry.DisplayName) ? e.Entry.PlayerId : e.Entry.DisplayName;
            if (ratingText) ratingText.text = e.Entry.Rating.ToString("N0");
            if (subText)
            {
                int g = e.Entry.Games; float wr = g > 0 ? (float)e.Entry.Wins / g : 0f;
                subText.text = string.IsNullOrEmpty(e.Entry.RankLabel)
                    ? $"{Mathf.RoundToInt(wr * 100)}% WR ({g} games)"
                    : $"{e.Entry.RankLabel} · {Mathf.RoundToInt(wr * 100)}% WR";
            }
            if (crown) crown.SetActive(e.Rank == 1);
            if (background) background.color = e.Rank == 1 ? new Color(0.18f,0.16f,0.10f,0.95f) : new Color(0.13f,0.15f,0.21f,0.9f);

            var btn = GetComponent<Button>(); if (btn == null) btn = gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => OnClicked?.Invoke(ChampionId));
        }

        private static Color Tint(string championId)
        {
            if (string.IsNullOrEmpty(championId)) return new Color(0.4f,0.4f,0.5f);
            int h = 0; foreach (char c in championId) h = h * 31 + c;
            var st = Random.state; Random.InitState(h);
            var col = Color.HSVToRGB(Random.value, 0.5f, 0.85f);
            Random.state = st; return col;
        }
    }
}
