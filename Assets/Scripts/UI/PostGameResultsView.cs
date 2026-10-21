// Project D  |  Codename: Zantetsuken
// PostGameResultsView.cs  (PostGame UI — the results screen)
//
// Renders the end-of-match results: a Victory/Defeat banner, both teams' scoreboards
// (sorted by performance), the MVP crown on the top performer, an MVP vote (each
// player votes once), and a rewards reveal (match XP -> account level/KP + bonuses).
// Binds a PostGameResult; the vote tally is server-authoritative in a real build.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGame;

namespace ProjectD.PostGameUI
{
    public sealed class PostGameResultsView : MonoBehaviour
    {
        [Header("Outcome")]
        [SerializeField] private Text outcomeBanner;      // "VICTORY" / "DEFEAT"
        [SerializeField] private Image outcomeAccent;
        [SerializeField] private Text durationLabel;

        [Header("Scoreboards")]
        [SerializeField] private RectTransform blueRowParent;
        [SerializeField] private RectTransform redRowParent;
        [SerializeField] private ScoreboardRow rowPrefab;

        [Header("MVP")]
        [SerializeField] private Text mvpNameLabel;        // "MVP: <name>"
        [SerializeField] private Text voteStatusLabel;     // "Vote for MVP" / "Voting closed"

        [Header("Rewards")]
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private Text rewardXpLabel;       // "+312 XP"
        [SerializeField] private Text rewardKpLabel;       // "+400 KP (First Win)"
        [SerializeField] private Image xpBarFill;
        [SerializeField] private Button continueButton;

        [Header("Behaviour")]
        [SerializeField] private string localPlayerId = "me";
        [SerializeField] private float rowRevealDelay = 0.08f;

        private readonly List<ScoreboardRow> _rows = new();
        private readonly MvpVoteTally _tally = new();
        private PostGameResult _result;
        private bool _localVoted;

        /// <summary>Fired when the player taps Continue (hand off to menu / leveling).</summary>
        public event System.Action<PostGameResult> OnContinue;
        /// <summary>Fired when the local player casts an MVP vote (send to server).</summary>
        public event System.Action<string> OnLocalVote;

        private static readonly Color VictoryColor = new Color(0.30f, 0.62f, 0.92f);
        private static readonly Color DefeatColor = new Color(0.85f, 0.35f, 0.35f);

        public void Present(PostGameResult result)
        {
            _result = result;
            _localVoted = false;

            // Outcome banner.
            bool win = result.Outcome == MatchOutcome.Victory;
            if (outcomeBanner) { outcomeBanner.text = win ? "VICTORY" : (result.Outcome == MatchOutcome.Defeat ? "DEFEAT" : "MATCH ENDED"); outcomeBanner.color = win ? VictoryColor : DefeatColor; }
            if (outcomeAccent) outcomeAccent.color = win ? VictoryColor : DefeatColor;
            if (durationLabel) durationLabel.text = FormatDuration(result.DurationSeconds);

            // MVP label.
            var mvp = FindStats(result.MvpPlayerId);
            if (mvpNameLabel) mvpNameLabel.text = mvp != null ? $"MVP: {mvp.displayName}" : "MVP: —";
            if (voteStatusLabel) voteStatusLabel.text = "Vote for your MVP";

            BuildScoreboards();

            // Rewards for the local player.
            if (result.Rewards.TryGetValue(localPlayerId, out var reward))
            {
                if (rewardsPanel) rewardsPanel.SetActive(true);
                if (rewardXpLabel) rewardXpLabel.text = $"+{reward.accountXp} XP";
                if (rewardKpLabel) rewardKpLabel.text = reward.bonusKp > 0 ? $"+{reward.bonusKp} KP (First Win)" : "";
            }
            else if (rewardsPanel) rewardsPanel.SetActive(false);

            if (continueButton) { continueButton.onClick.RemoveAllListeners(); continueButton.onClick.AddListener(() => OnContinue?.Invoke(_result)); }

            StopAllCoroutines();
            StartCoroutine(RevealRows());
        }

        private void BuildScoreboards()
        {
            foreach (var r in _rows) if (r) Destroy(r.gameObject);
            _rows.Clear();

            AddTeam(PostGameService.ScoreboardFor(_result, Team.Blue), blueRowParent);
            AddTeam(PostGameService.ScoreboardFor(_result, Team.Red), redRowParent);
        }

        private void AddTeam(List<PlayerMatchStats> team, RectTransform parent)
        {
            if (parent == null || rowPrefab == null) return;
            foreach (var p in team)
            {
                var row = Instantiate(rowPrefab, parent);
                bool isLocal = p.playerId == localPlayerId;
                bool isMvp = p.playerId == _result.MvpPlayerId;
                bool canVote = !isLocal;  // can't vote for yourself
                row.Bind(p, isLocal, isMvp, canVote, _tally.VotesFor(p.playerId));
                row.OnVote += HandleVote;
                row.gameObject.SetActive(false);
                _rows.Add(row);
            }
        }

        private void HandleVote(string targetId)
        {
            if (_localVoted) return;
            if (!_tally.CastVote(localPlayerId, targetId)) return;
            _localVoted = true;
            OnLocalVote?.Invoke(targetId);

            // Update vote counts + lock voting.
            foreach (var row in _rows) row.SetVoteCount(_tally.VotesFor(row.PlayerId), votingDone: true);
            if (voteStatusLabel) voteStatusLabel.text = "Vote cast — thanks!";
        }

        /// <summary>Server pushes the final community-voted MVP (optional highlight).</summary>
        public void ApplyCommunityMvp(string mvpPlayerId)
        {
            var mvp = FindStats(mvpPlayerId);
            if (mvp != null && mvpNameLabel) mvpNameLabel.text = $"Community MVP: {mvp.displayName}";
        }

        private IEnumerator RevealRows()
        {
            float t = 0f; while (t < 0.3f) { t += Time.deltaTime; if (outcomeAccent) { var c = outcomeAccent.color; c.a = Mathf.Clamp01(t / 0.3f); outcomeAccent.color = c; } yield return null; }
            foreach (var row in _rows) { row.gameObject.SetActive(true); yield return new WaitForSeconds(rowRevealDelay); }

            // Animate the XP bar fill as a small flourish.
            if (xpBarFill)
            {
                float f = 0f; while (f < 1f) { f += Time.deltaTime; xpBarFill.fillAmount = Mathf.Clamp01(f); yield return null; }
            }
        }

        private PlayerMatchStats FindStats(string id)
        {
            if (_result == null || string.IsNullOrEmpty(id)) return null;
            foreach (var p in _result.AllPlayers()) if (p.playerId == id) return p;
            return null;
        }

        public void SetLocalPlayer(string id) => localPlayerId = id;

        private static string FormatDuration(float seconds)
        {
            int m = (int)(seconds / 60f), s = (int)(seconds % 60f);
            return $"{m:00}:{s:00}";
        }
    }
}
