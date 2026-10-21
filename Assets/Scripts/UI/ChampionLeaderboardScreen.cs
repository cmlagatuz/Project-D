// Project D  |  Codename: Zantetsuken
// ChampionLeaderboardScreen.cs  (Leaderboard UI — per-champion "Server No.1" board)
//
// Two views over the champion-rating pool:
//   - SERVER No.1 grid: the single best-rated player per champion (default).
//   - CHAMPION FOCUS list: click a champion to see its full top-N ladder.
// A region filter narrows the pool. Distinct from the apex ladder — this answers
// "who is the #1 <champion>?" for every champion. Binds a list of ChampionRating.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.LeaderboardX;

namespace ProjectD.LeaderboardXUI
{
    public sealed class ChampionLeaderboardScreen : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text titleText;
        [SerializeField] private Dropdown regionDropdown;
        [SerializeField] private Button backToGridButton;    // shown in focus mode

        [Header("List")]
        [SerializeField] private RectTransform rowParent;
        [SerializeField] private ChampionLeaderRow rowPrefab;
        [SerializeField] private Text resultCountText;

        private readonly List<ChampionLeaderRow> _rows = new();
        private IReadOnlyList<ChampionRating> _pool = new List<ChampionRating>();
        private string _region = null;
        private string _focusChampion = null;   // null = grid mode

        public void Bind(IReadOnlyList<ChampionRating> pool, IReadOnlyList<string> regions = null)
        {
            _pool = pool ?? new List<ChampionRating>();

            if (regionDropdown)
            {
                var opts = new List<Dropdown.OptionData> { new Dropdown.OptionData("All Regions") };
                if (regions != null) foreach (var r in regions) opts.Add(new Dropdown.OptionData(r));
                regionDropdown.ClearOptions(); regionDropdown.AddOptions(opts);
                regionDropdown.onValueChanged.RemoveAllListeners();
                regionDropdown.onValueChanged.AddListener(i => { _region = i <= 0 ? null : opts[i].text; Refresh(); });
            }
            if (backToGridButton) backToGridButton.onClick.AddListener(() => { _focusChampion = null; Refresh(); });

            Refresh();
        }

        private void Refresh()
        {
            foreach (var r in _rows) if (r) Destroy(r.gameObject);
            _rows.Clear();

            if (_focusChampion == null) BuildServerNumberOne();
            else BuildChampionFocus(_focusChampion);

            if (backToGridButton) backToGridButton.gameObject.SetActive(_focusChampion != null);
        }

        private void BuildServerNumberOne()
        {
            if (titleText) titleText.text = "SERVER No.1 — Best per Champion";
            var top = ChampionLeaderboard.ServerNumberOne(_pool, _region);
            // Sort the grid by rating descending for a nice ordering.
            var entries = new List<ChampionTopEntry>(top.Values);
            entries.Sort((a, b) => b.Entry.Rating.CompareTo(a.Entry.Rating));

            foreach (var e in entries)
            {
                var row = Instantiate(rowPrefab, rowParent);
                row.Bind(e, showChampion: true);
                row.OnClicked += champ => { _focusChampion = champ; Refresh(); };
                _rows.Add(row);
            }
            if (resultCountText) resultCountText.text = $"{entries.Count} champions";
        }

        private void BuildChampionFocus(string championId)
        {
            if (titleText) titleText.text = $"TOP {championId.ToUpperInvariant()} — Champion Ladder";
            var list = ChampionLeaderboard.TopForChampion(_pool, championId, topN: 100, region: _region);
            foreach (var e in list)
            {
                var row = Instantiate(rowPrefab, rowParent);
                row.Bind(e, showChampion: false);
                _rows.Add(row);
            }
            if (resultCountText) resultCountText.text = $"{list.Count} players";
        }
    }
}
