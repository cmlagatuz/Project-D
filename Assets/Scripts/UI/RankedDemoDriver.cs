// Project D  |  Codename: Zantetsuken
// RankedDemoDriver.cs  (Ranked UI — a play-in-editor sandbox)
//
// Wires a RankService to a badge + rank-up screen and offers Win / Loss / Remake
// buttons so you can climb the ladder and watch promotions/demotions and the
// rank-up screen. In a real build the RankService pulls/pushes the authoritative
// state from the account backend; here it runs locally.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;

namespace ProjectD.RankedUI
{
    public sealed class RankedDemoDriver : MonoBehaviour
    {
        [SerializeField] private RankBadge badge;
        [SerializeField] private RankUpScreen rankUpScreen;
        [SerializeField] private SeasonRewardsScreen rewardsScreen;
        [SerializeField] private SeasonRewardConfig rewardConfig;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button winButton, lossButton, remakeButton, newSeasonButton;
        [SerializeField] private int lobbyAvgOpponentMmr = 1000;

        private readonly RankService _service = new();
        private readonly SeasonRewardService _rewardService = new();
        private PendingSeasonReward _localPending;

        private void Start()
        {
            _service.Initialize();
            _service.OnRankChanged += HandleChange;

            if (winButton) winButton.onClick.AddListener(() => Report(MatchResult.Win));
            if (lossButton) lossButton.onClick.AddListener(() => Report(MatchResult.Loss));
            if (remakeButton) remakeButton.onClick.AddListener(() => Report(MatchResult.Remake));
            if (newSeasonButton) newSeasonButton.onClick.AddListener(EndSeasonAndReward);

            // Wire the reward service to local storage (offline demo).
            _rewardService.PushPendingReward = p => _localPending = p;
            _rewardService.PullPendingReward = () => _localPending;
            _rewardService.GrantToAccount = p => Debug.Log($"[Project D] Granted {p.emblemIds.Length} emblems, +{p.totalHonor} honor.");

            Refresh();
        }

        private void EndSeasonAndReward()
        {
            // Compute this season's rewards from the finished state, reveal them, then reset.
            if (rewardConfig != null && rewardsScreen != null)
            {
                var result = _rewardService.FinalizeSeasonForPlayer(_service.State, rewardConfig, _service.Season.seasonId);
                rewardsScreen.Present(_rewardService, _localPending, result, _service.Season.name);
                rewardsScreen.OnClaimed += RollNextSeason;
            }
            else
            {
                RollNextSeason();
            }
        }

        private void RollNextSeason()
        {
            _service.StartNewSeason(new SeasonInfo { seasonId = _service.Season.seasonId + 1, name = $"Season {_service.Season.seasonId + 1}" });
            _localPending = null;
            Refresh();
        }

        private void Report(MatchResult r)
        {
            // Opponent lobby MMR tracks the player a bit so the demo climbs believably.
            lobbyAvgOpponentMmr = Mathf.Clamp(_service.State.mmr + Random.Range(-60, 60), 0, 3000);
            _service.ReportMatch(r, lobbyAvgOpponentMmr);
        }

        private void HandleChange(RankChange change)
        {
            if (rankUpScreen) rankUpScreen.Present(change, _service.State);
            Refresh();
        }

        private void Refresh()
        {
            if (badge) badge.Bind(_service.State);
            var s = _service.State;
            if (statusLabel)
                statusLabel.text = s.placementsComplete
                    ? $"{RankFormatter.Full(s)}    (MMR {s.mmr}, shield {s.demotionShieldGames})"
                    : $"{RankFormatter.PlacementProgress(s)}    (MMR {s.mmr})";
        }
    }
}
