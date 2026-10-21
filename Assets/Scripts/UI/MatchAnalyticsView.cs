// Project D  |  Codename: Zantetsuken
// MatchAnalyticsView.cs  (PostGame UI — a single match's analytics screen)
//
// Opened from the post-game results screen ("View Match Analytics"). Renders ONE
// finished match's breakdown:
//   - Header      : outcome + duration + a fairness gauge/grade + MMR spread.
//   - Timeline    : a gold-lead line (blue up / red down) with peak leads + final diff.
//   - Scoreboards : both teams sorted by performance, with KDA/CS/gold/damage-share,
//                   MVP crowned; click a player to see their coaching insights.
//   - Coaching    : the selected player's tips (praise/tip/warning) from the P10 core.
// Read-only; the server supplies the MatchAnalytics payload.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGame;
using ProjectD.P10.Coaching;

namespace ProjectD.PostGameUI
{
    public sealed class MatchAnalyticsView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text outcomeText, durationText, mmrSpreadText, fairnessGradeText;
        [SerializeField] private Image fairnessGauge;

        [Header("Timeline")]
        [SerializeField] private RectTransform timelineArea;
        [SerializeField] private Image segmentPrefab;
        [SerializeField] private Text peakBlueText, peakRedText, finalDiffText;

        [Header("Scoreboards")]
        [SerializeField] private RectTransform blueParent, redParent;
        [SerializeField] private Button playerRowPrefab;   // Text[]: name, kda, cs, gold, dmg%  + crown Image
        [SerializeField] private string localPlayerId = "me";

        [Header("Coaching")]
        [SerializeField] private Text coachingHeader;
        [SerializeField] private RectTransform coachingListParent;
        [SerializeField] private RectTransform coachingRowPrefab;  // [icon][message]

        private MatchAnalytics _m;
        private readonly List<Image> _segments = new();

        public void Present(MatchAnalytics analytics)
        {
            _m = analytics; _m.Build();

            bool localWon = LocalTeam() == _m.winningTeam;
            if (outcomeText) { outcomeText.text = localWon ? "VICTORY" : "DEFEAT"; outcomeText.color = localWon ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f); }
            if (durationText) durationText.text = FormatTime(_m.durationSeconds);
            if (mmrSpreadText) mmrSpreadText.text = $"MMR spread: {_m.MmrSpread}   (B {_m.BlueAvgMmr} / R {_m.RedAvgMmr})";
            if (fairnessGrade()) fairnessGradeText.text = $"Match quality: {_m.FairnessGrade} ({_m.PreMatchFairness*100:0}%)";
            if (fairnessGauge) { fairnessGauge.fillAmount = _m.PreMatchFairness; fairnessGauge.color = FairColor(_m.PreMatchFairness); }

            DrawTimeline();
            BuildScoreboard(_m.Scoreboard(Team.Blue), blueParent);
            BuildScoreboard(_m.Scoreboard(Team.Red), redParent);
            ShowCoaching(localPlayerId);   // default to the local player
        }

        private bool fairnessGrade() => fairnessGradeText != null;

        private void DrawTimeline()
        {
            foreach (var s in _segments) if (s) Destroy(s.gameObject);
            _segments.Clear();
            if (timelineArea == null || segmentPrefab == null || _m.Timeline.Count < 2) return;

            float w = timelineArea.rect.width, h = timelineArea.rect.height;
            int maxAbs = 1;
            foreach (var s in _m.Timeline) maxAbs = Mathf.Max(maxAbs, Mathf.Abs(s.goldDiff));

            Vector2 prev = default; bool first = true;
            for (int i = 0; i < _m.Timeline.Count; i++)
            {
                float x = w * i / (_m.Timeline.Count - 1) - w / 2f;
                float y = (_m.Timeline[i].goldDiff / (float)maxAbs) * (h / 2f);
                var p = new Vector2(x, y);
                if (!first) DrawSegment(prev, p, _m.Timeline[i].goldDiff >= 0);
                prev = p; first = false;
            }

            if (peakBlueText) peakBlueText.text = $"Blue peak +{_m.PeakBlueLead}g";
            if (peakRedText) peakRedText.text = $"Red peak +{_m.PeakRedLead}g";
            if (finalDiffText)
            {
                int d = _m.FinalGoldDiff;
                finalDiffText.text = d == 0 ? "Even gold" : (d > 0 ? $"Blue +{d}g" : $"Red +{-d}g");
                finalDiffText.color = d >= 0 ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f);
            }
        }

        private void DrawSegment(Vector2 a, Vector2 b, bool blueSide)
        {
            var seg = Instantiate(segmentPrefab, timelineArea); seg.gameObject.SetActive(true);
            var rt = (RectTransform)seg.transform; Vector2 dir = b - a; float len = dir.magnitude;
            rt.sizeDelta = new Vector2(len, 4f);
            rt.anchoredPosition = a + dir * 0.5f;
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            seg.color = blueSide ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f);
            _segments.Add(seg);
        }

        private void BuildScoreboard(List<PlayerLine> team, RectTransform parent)
        {
            if (parent == null || playerRowPrefab == null) return;
            foreach (Transform c in parent) Destroy(c.gameObject);
            foreach (var p in team)
            {
                var row = Instantiate(playerRowPrefab, parent); row.gameObject.SetActive(true);
                var texts = row.GetComponentsInChildren<Text>();
                if (texts.Length > 0) { texts[0].text = p.displayName; texts[0].color = p.playerId == localPlayerId ? new Color(0.55f,0.85f,1f) : Color.white; }
                if (texts.Length > 1) texts[1].text = $"{p.kills}/{p.deaths}/{p.assists}";
                if (texts.Length > 2) texts[2].text = $"{p.cs} cs";
                if (texts.Length > 3) texts[3].text = Compact(p.gold) + "g";
                if (texts.Length > 4) texts[4].text = $"{Mathf.RoundToInt(p.DamageShare * 100)}% dmg";
                var imgs = row.GetComponentsInChildren<Image>();
                bool isMvp = p.playerId == _m.MvpPlayerId;
                if (imgs.Length > 1) imgs[1].enabled = isMvp;   // crown
                string id = p.playerId;
                row.onClick.RemoveAllListeners();
                row.onClick.AddListener(() => ShowCoaching(id));
                var bg = row.GetComponent<Image>();
                if (bg) bg.color = isMvp ? new Color(0.18f,0.16f,0.10f,0.95f) : (p.playerId == localPlayerId ? new Color(0.16f,0.20f,0.30f,0.95f) : new Color(0.13f,0.15f,0.21f,0.9f));
            }
        }

        private void ShowCoaching(string playerId)
        {
            var line = FindLine(playerId);
            if (coachingHeader) coachingHeader.text = line != null ? $"Coaching — {line.displayName} ({line.championId})" : "Coaching";
            if (coachingListParent == null || coachingRowPrefab == null) return;
            foreach (Transform c in coachingListParent) Destroy(c.gameObject);
            foreach (var insight in _m.CoachingFor(playerId))
            {
                var row = Instantiate(coachingRowPrefab, coachingListParent); row.gameObject.SetActive(true);
                var t = row.GetComponentInChildren<Text>(); if (t) t.text = insight.message;
                var icon = row.GetComponentInChildren<Image>();
                if (icon) icon.color = insight.severity switch { InsightSeverity.Praise => new Color(0.45f,0.85f,0.5f), InsightSeverity.Warning => new Color(0.9f,0.4f,0.4f), _ => new Color(0.95f,0.8f,0.35f) };
            }
        }

        public void SetLocalPlayer(string id) => localPlayerId = id;

        private PlayerLine FindLine(string id) { foreach (var p in _m.AllPlayers()) if (p.playerId == id) return p; return null; }
        private Team LocalTeam() { foreach (var p in _m.AllPlayers()) if (p.playerId == localPlayerId) return p.team; return Team.Blue; }
        private static string Compact(int v) => v >= 1000 ? $"{v/1000f:0.0}k" : v.ToString();
        private static string FormatTime(float s) { int m = (int)(s/60), sec = (int)(s%60); return $"{m:00}:{sec:00}"; }
        private static Color FairColor(float p) => p >= 0.85f ? new Color(0.45f,0.85f,0.5f) : p >= 0.6f ? new Color(0.7f,0.85f,0.5f) : p >= 0.4f ? new Color(0.95f,0.8f,0.35f) : new Color(0.9f,0.4f,0.4f);
    }
}
