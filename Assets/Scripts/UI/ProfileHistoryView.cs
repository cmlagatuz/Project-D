// Project D  |  Codename: Zantetsuken
// ProfileHistoryView.cs  (Profile UI — the match-history list on the player's profile)
//
// Renders the recent-matches list on the profile: each row shows outcome (win/loss),
// champion, KDA, CS, mode, and duration, with a win/loss accent and an MVP crown. A
// header summarizes record (W-L, win-rate, current streak) derived from history.
// Tapping a row raises OnOpenMatch so the app can open the full post-game analytics.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.History;

namespace ProjectD.HistoryUI
{
    public sealed class ProfileHistoryView : MonoBehaviour
    {
        [Header("Summary header")]
        [SerializeField] private Text recordText, winRateText, streakText, mostPlayedText;

        [Header("List")]
        [SerializeField] private RectTransform rowParent;
        [SerializeField] private Button rowPrefab;   // [outcome][champ][kda][cs][mode] + crown Image + accent Image
        [SerializeField] private Text emptyText;

        public event System.Action<string> OnOpenMatch;   // matchId

        public void Bind(MatchHistory history, int count = 10)
        {
            // Header (derived from history — this is the profile's real record).
            if (recordText) recordText.text = $"{history.Wins()}W - {history.Games - history.Wins()}L";
            if (winRateText) winRateText.text = history.Games > 0 ? $"{Mathf.RoundToInt(history.WinRate * 100)}% WR" : "No games";
            if (streakText) { int s = history.WinStreak(); streakText.text = s >= 2 ? $"{s}-win streak" : ""; streakText.color = new Color(0.55f,0.85f,0.6f); }
            if (mostPlayedText) { var mp = history.MostPlayedChampion(); mostPlayedText.text = mp != null ? $"Most played: {mp}" : ""; }

            // List.
            if (rowParent != null && rowPrefab != null)
            {
                foreach (Transform c in rowParent) Destroy(c.gameObject);
                var recent = history.Recent(count);
                if (emptyText) emptyText.gameObject.SetActive(recent.Count == 0);
                foreach (var e in recent)
                {
                    var row = Instantiate(rowPrefab, rowParent); row.gameObject.SetActive(true);
                    var t = row.GetComponentsInChildren<Text>();
                    bool win = e.outcome == MatchOutcome.Victory;
                    if (t.Length > 0) { t[0].text = win ? "WIN" : (e.outcome == MatchOutcome.Defeat ? "LOSS" : "—"); t[0].color = win ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f); }
                    if (t.Length > 1) t[1].text = e.championId;
                    if (t.Length > 2) t[2].text = $"{e.kills}/{e.deaths}/{e.assists}";
                    if (t.Length > 3) t[3].text = $"{e.cs} cs";
                    if (t.Length > 4) t[4].text = $"{PrettyMode(e.mode)} · {FormatTime(e.durationSeconds)}";
                    var imgs = row.GetComponentsInChildren<Image>();
                    // imgs[0] = row bg (accent by outcome), imgs[1] = MVP crown
                    if (imgs.Length > 0) imgs[0].color = win ? new Color(0.14f,0.19f,0.30f,0.95f) : new Color(0.24f,0.15f,0.17f,0.95f);
                    if (imgs.Length > 1) imgs[1].enabled = e.wasMvp;
                    string id = e.matchId;
                    row.onClick.RemoveAllListeners();
                    row.onClick.AddListener(() => OnOpenMatch?.Invoke(id));
                }
            }
        }

        private static string PrettyMode(string mode) => string.IsNullOrEmpty(mode) ? "Match" : mode;
        private static string FormatTime(float s) { int m = (int)(s / 60), sec = (int)(s % 60); return $"{m}:{sec:00}"; }
    }
}
