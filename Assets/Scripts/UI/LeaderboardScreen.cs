// Project D  |  Codename: Zantetsuken
// LeaderboardScreen.cs  (Leaderboard UI — the apex board with filters)
//
// Binds the LeaderboardService to the UI: a Server/Friends scope toggle, a tier
// filter (Both / Adamantite / Crystal), a champion dropdown, a lane dropdown, a
// paged row list, and a pinned "your rank" card that shows the viewer's position
// even when it's off the current page. Re-queries on any filter change; the actual
// filtering/ranking is done by the engine so it stays consistent with the server.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;
using ProjectD.Leaderboard;

namespace ProjectD.LeaderboardUI
{
    public sealed class LeaderboardScreen : MonoBehaviour
    {
        [Header("Header / filters")]
        [SerializeField] private Button serverScopeButton, friendsScopeButton;
        [SerializeField] private Button[] tierButtons;          // Both, Adamantite, Crystal
        [SerializeField] private Dropdown championDropdown;
        [SerializeField] private Dropdown laneDropdown;
        [SerializeField] private Text resultCountText;

        [Header("Rows")]
        [SerializeField] private RectTransform rowParent;
        [SerializeField] private LeaderboardRow rowPrefab;
        [SerializeField] private Button prevPageButton, nextPageButton;
        [SerializeField] private Text pageLabel;

        [Header("Your rank card")]
        [SerializeField] private LeaderboardRow viewerRow;
        [SerializeField] private GameObject viewerCard;
        [SerializeField] private Text viewerMissingLabel;       // "You're not in the apex tiers"

        private readonly LeaderboardService _service = new();
        private readonly List<LeaderboardRow> _rows = new();
        private LeaderboardQuery _query;
        private List<string> _championIds = new();

        /// <summary>Provide the service's data source + viewer id (from the app).</summary>
        public void Initialize(string viewerId, System.Func<LeaderboardScope, IReadOnlyList<LeaderboardEntry>> fetchPool)
        {
            _service.ViewerId = viewerId;
            _service.FetchPool = fetchPool;
            _query = LeaderboardQuery.Default(LeaderboardScope.Server);
            _query.PageSize = 50;

            HookFilters();
            BuildChampionDropdown();
            BuildLaneDropdown();
            Refresh();
        }

        /// <summary>Offline/demo entry: inject a pool directly.</summary>
        public void InitializeLocal(string viewerId, IReadOnlyList<LeaderboardEntry> pool)
        {
            _service.SetLocalPool(pool);
            Initialize(viewerId, null);
        }

        // ---- filter wiring ----
        private void HookFilters()
        {
            if (serverScopeButton) serverScopeButton.onClick.AddListener(() => SetScope(LeaderboardScope.Server));
            if (friendsScopeButton) friendsScopeButton.onClick.AddListener(() => SetScope(LeaderboardScope.Friends));

            for (int i = 0; i < tierButtons.Length; i++)
            {
                RankTier? tier = i switch { 1 => RankTier.Adamantite, 2 => RankTier.Crystal, _ => (RankTier?)null };
                if (tierButtons[i]) tierButtons[i].onClick.AddListener(() => { _query.Tier = tier; _query.Page = 0; Refresh(); });
            }

            if (championDropdown) championDropdown.onValueChanged.AddListener(i =>
            {
                _query.ChampionId = (i <= 0 || i - 1 >= _championIds.Count) ? null : _championIds[i - 1];
                _query.Page = 0; Refresh();
            });
            if (laneDropdown) laneDropdown.onValueChanged.AddListener(i =>
            {
                var lanes = LeaderboardService.LaneFilterOptions();
                _query.Lane = (i >= 0 && i < lanes.Length) ? lanes[i] : Lane.Any;
                _query.Page = 0; Refresh();
            });

            if (prevPageButton) prevPageButton.onClick.AddListener(() => { if (_query.Page > 0) { _query.Page--; Refresh(); } });
            if (nextPageButton) nextPageButton.onClick.AddListener(() => { _query.Page++; Refresh(); });
        }

        private void SetScope(LeaderboardScope scope)
        {
            _query.Scope = scope; _query.Page = 0;
            HighlightScope(scope);
            BuildChampionDropdown();  // champion options can differ by scope
            Refresh();
        }

        private void BuildChampionDropdown()
        {
            if (championDropdown == null) return;
            _championIds = _service.ChampionFilterOptions(_query.Scope);
            var options = new List<Dropdown.OptionData> { new Dropdown.OptionData("All Champions") };
            foreach (var id in _championIds) options.Add(new Dropdown.OptionData(id));
            championDropdown.ClearOptions();
            championDropdown.AddOptions(options);
            championDropdown.SetValueWithoutNotify(0);
        }

        private void BuildLaneDropdown()
        {
            if (laneDropdown == null) return;
            var options = new List<Dropdown.OptionData>();
            foreach (var lane in LeaderboardService.LaneFilterOptions())
                options.Add(new Dropdown.OptionData(lane == Lane.Any ? "All Lanes" : lane.ToString()));
            laneDropdown.ClearOptions();
            laneDropdown.AddOptions(options);
            laneDropdown.SetValueWithoutNotify(0);
        }

        // ---- render ----
        public void Refresh()
        {
            var page = _service.Query(_query);

            foreach (var r in _rows) if (r) Destroy(r.gameObject);
            _rows.Clear();

            if (rowParent && rowPrefab)
                foreach (var row in page.Rows)
                {
                    var ui = Instantiate(rowPrefab, rowParent);
                    ui.Bind(row, row.Entry.playerId == _service.ViewerId);
                    _rows.Add(ui);
                }

            if (resultCountText) resultCountText.text = $"{page.TotalMatching} players";
            int totalPages = Mathf.Max(1, Mathf.CeilToInt(page.TotalMatching / (float)Mathf.Max(1, _query.PageSize)));
            if (pageLabel) pageLabel.text = $"Page {_query.Page + 1} / {totalPages}";
            if (prevPageButton) prevPageButton.interactable = _query.Page > 0;
            if (nextPageButton) nextPageButton.interactable = _query.Page + 1 < totalPages;

            // Viewer own-rank card.
            bool present = page.ViewerRank > 0;
            if (viewerCard) viewerCard.SetActive(present);
            if (viewerMissingLabel) viewerMissingLabel.gameObject.SetActive(!present);
            if (present && viewerRow) viewerRow.Bind(page.ViewerRow, true);
        }

        private void HighlightScope(LeaderboardScope scope)
        {
            Tint(serverScopeButton, scope == LeaderboardScope.Server);
            Tint(friendsScopeButton, scope == LeaderboardScope.Friends);
        }

        private static void Tint(Button b, bool on)
        {
            if (!b) return; var img = b.GetComponent<Image>();
            if (img) img.color = on ? new Color(0.36f,0.62f,0.92f) : new Color(0.16f,0.18f,0.24f);
        }
    }
}
