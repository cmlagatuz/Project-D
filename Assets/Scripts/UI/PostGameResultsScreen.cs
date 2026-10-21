// Project D  |  Codename: Zantetsuken
// PostGameResultsScreen.cs  (PostGame UI — results screen that links to Match Analytics)
//
// The screen shown right after a match: outcome banner, quick KDA summary, rewards,
// and the action buttons — including "VIEW MATCH ANALYTICS", which swaps to the
// MatchAnalyticsView for the same match. Owns both panels and toggles between them,
// with polished fade/slide transitions.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGame;
using ProjectD.Polish;

namespace ProjectD.PostGameUI
{
    public sealed class PostGameResultsScreen : MonoBehaviour
    {
        [Header("Results panel")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private CanvasGroup resultsGroup;
        [SerializeField] private Text outcomeText, summaryText, rewardText;
        [SerializeField] private Button analyticsButton;      // "VIEW MATCH ANALYTICS"
        [SerializeField] private Button continueButton;       // back to menu (demo: logs)

        [Header("Analytics panel")]
        [SerializeField] private GameObject analyticsPanel;
        [SerializeField] private CanvasGroup analyticsGroup;
        [SerializeField] private MatchAnalyticsView analyticsView;
        [SerializeField] private Button backButton;           // analytics -> results

        [SerializeField] private string localPlayerId = "me";
        private MatchAnalytics _match;

        public event System.Action OnContinue;

        public void Present(MatchAnalytics match)
        {
            _match = match; _match.Build();
            if (analyticsView) analyticsView.SetLocalPlayer(localPlayerId);

            // Results summary.
            bool won = LocalTeam() == _match.winningTeam;
            if (outcomeText) { outcomeText.text = won ? "VICTORY" : "DEFEAT"; outcomeText.color = won ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f); }
            var me = FindLine(localPlayerId);
            if (summaryText && me != null) summaryText.text = $"{me.displayName} — {me.kills}/{me.deaths}/{me.assists} on {me.championId}";
            if (rewardText) rewardText.text = won ? "+312 XP   +400 KP (First Win)" : "+180 XP";

            if (analyticsButton) { analyticsButton.onClick.RemoveAllListeners(); analyticsButton.onClick.AddListener(ShowAnalytics); }
            if (backButton) { backButton.onClick.RemoveAllListeners(); backButton.onClick.AddListener(ShowResults); }
            if (continueButton) { continueButton.onClick.RemoveAllListeners(); continueButton.onClick.AddListener(() => OnContinue?.Invoke()); }

            ShowResults();
        }

        private void ShowResults()
        {
            if (analyticsPanel) analyticsPanel.SetActive(false);
            if (resultsPanel) resultsPanel.SetActive(true);
            if (resultsGroup) Tw.FadeIn(resultsGroup, 0.3f);
        }

        private void ShowAnalytics()
        {
            if (resultsPanel) resultsPanel.SetActive(false);
            if (analyticsPanel) analyticsPanel.SetActive(true);
            if (analyticsView) analyticsView.Present(_match);        // (re)build + animate
            if (analyticsGroup) Tw.FadeIn(analyticsGroup, 0.3f);
        }

        public void SetLocalPlayer(string id) => localPlayerId = id;

        private PlayerLine FindLine(string id) { foreach (var p in _match.AllPlayers()) if (p.playerId == id) return p; return null; }
        private Team LocalTeam() { foreach (var p in _match.AllPlayers()) if (p.playerId == localPlayerId) return p.team; return Team.Blue; }
    }
}
