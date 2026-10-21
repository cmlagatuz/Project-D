// Project D  |  Codename: Zantetsuken
// OpsConsoleView.cs  (P12 UI — the live-ops / governance console)
//
// An operator console for the P12 systems. Each panel reads a P12 core and renders a
// live, actionable tile:
//   - Live-Ops schedule : scheduled events with active/inactive state + a clock advance.
//   - Moderation queue   : triaged report cases (priority-sorted) with routed actions.
//   - Incident board     : open incidents by severity + MTTR, with an advance action.
//   - Experiment rollout : the current stage + % with Advance / Kill controls.
//   - Clan war standings : the war-points leaderboard.
// Bound to the cores; a demo driver simulates operator activity so tiles update.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P12.Automation;
using ProjectD.P12.Moderation;
using ProjectD.P12.Incidents;
using ProjectD.P12.Rollout;
using ProjectD.P12.ClanWars;

namespace ProjectD.P12UI
{
    public sealed class OpsConsoleView : MonoBehaviour
    {
        [Header("Live-Ops schedule")]
        [SerializeField] private RectTransform scheduleListParent;
        [SerializeField] private RectTransform scheduleRowPrefab;   // [name][state]
        [SerializeField] private Text clockText, auditCountText;
        [SerializeField] private Button advanceClockButton;

        [Header("Moderation queue")]
        [SerializeField] private RectTransform modListParent;
        [SerializeField] private RectTransform modRowPrefab;        // [offender][priority][action]
        [SerializeField] private Text modPendingText;

        [Header("Incident board")]
        [SerializeField] private RectTransform incListParent;
        [SerializeField] private RectTransform incRowPrefab;        // [title][sev][state]
        [SerializeField] private Text incOpenText;
        [SerializeField] private Button advanceIncidentButton;

        [Header("Experiment rollout")]
        [SerializeField] private Text rolloutStageText, rolloutPctText;
        [SerializeField] private Image rolloutBar;
        [SerializeField] private Button rolloutAdvanceButton, rolloutKillButton;

        [Header("Clan war standings")]
        [SerializeField] private RectTransform clanListParent;
        [SerializeField] private RectTransform clanRowPrefab;       // [rank+name][points][rating]

        // Bound cores.
        private LiveOpsScheduler _sched;
        private ModerationQueue _mod;
        private IncidentManager _incidents;
        private List<Incident> _incidentList = new();
        private Rollout _rollout;
        private ClanWarSeason _clanSeason;
        private long _clock = 1_000_000;
        private int _incidentCursor;

        public void Bind(LiveOpsScheduler sched, ModerationQueue mod, IncidentManager incidents, List<Incident> incidentList, Rollout rollout, ClanWarSeason clanSeason)
        {
            _sched = sched; _mod = mod; _incidents = incidents; _incidentList = incidentList; _rollout = rollout; _clanSeason = clanSeason;

            if (advanceClockButton) advanceClockButton.onClick.AddListener(AdvanceClock);
            if (advanceIncidentButton) advanceIncidentButton.onClick.AddListener(AdvanceIncident);
            if (rolloutAdvanceButton) rolloutAdvanceButton.onClick.AddListener(() => { _rollout.Advance(Random.Range(0f, 0.04f)); RefreshRollout(); });
            if (rolloutKillButton) rolloutKillButton.onClick.AddListener(() => { _rollout.Kill(); RefreshRollout(); });

            RefreshAll();
        }

        public void RefreshAll()
        {
            RefreshSchedule(); RefreshModeration(); RefreshIncidents(); RefreshRollout(); RefreshClans();
        }

        private void AdvanceClock()
        {
            _clock += 3600;   // +1 hour
            _sched.Tick(_clock);
            RefreshSchedule();
        }

        private void RefreshSchedule()
        {
            if (clockText) clockText.text = $"Clock: {(_clock % 86400) / 3600:00}:00";
            if (auditCountText) auditCountText.text = $"Audit entries: {_sched.Audit.Count}";
            if (scheduleListParent == null || scheduleRowPrefab == null) return;
            foreach (Transform c in scheduleListParent) Destroy(c.gameObject);
            foreach (var e in Iterate(_sched))
            {
                var row = Instantiate(scheduleRowPrefab, scheduleListParent); row.gameObject.SetActive(true);
                var t = row.GetComponentsInChildren<Text>();
                if (t.Length > 0) t[0].text = e.name ?? e.id;
                if (t.Length > 1) { t[1].text = e.active ? "ACTIVE" : "idle"; t[1].color = e.active ? new Color(0.45f,0.85f,0.5f) : new Color(0.6f,0.62f,0.7f); }
            }
        }
        // The scheduler exposes ActiveEvents; for the list we need all — track them locally.
        private IEnumerable<ScheduledEvent> Iterate(LiveOpsScheduler s) { foreach (var e in _allScheduled) yield return e; }
        private readonly List<ScheduledEvent> _allScheduled = new();
        public void RegisterScheduled(ScheduledEvent e) => _allScheduled.Add(e);

        private void RefreshModeration()
        {
            if (modPendingText) modPendingText.text = $"Pending: {_mod.Pending}";
            if (modListParent == null || modRowPrefab == null) return;
            foreach (Transform c in modListParent) Destroy(c.gameObject);
            foreach (var mc in _mod.Triage())
            {
                var row = Instantiate(modRowPrefab, modListParent); row.gameObject.SetActive(true);
                var t = row.GetComponentsInChildren<Text>();
                if (t.Length > 0) t[0].text = $"{mc.offenderId} ({mc.category})";
                if (t.Length > 1) { t[1].text = mc.priority.ToString("0.00"); t[1].color = PriColor(mc.priority); }
                if (t.Length > 2) { t[2].text = mc.routed.ToString(); t[2].color = ActionColor(mc.routed); }
            }
        }

        private void AdvanceIncident()
        {
            if (_incidentList.Count == 0) return;
            var inc = _incidentList[_incidentCursor % _incidentList.Count];
            var next = inc.state switch
            {
                IncidentState.Open => IncidentState.Investigating,
                IncidentState.Investigating => IncidentState.Mitigated,
                IncidentState.Mitigated => IncidentState.Resolved,
                _ => IncidentState.Resolved
            };
            _incidents.Advance(inc.id, next, _clock);
            _incidentCursor++;
            RefreshIncidents();
        }

        private void RefreshIncidents()
        {
            var open = _incidents.OpenIncidents();
            if (incOpenText) incOpenText.text = $"Open: {open.Count}";
            if (incListParent == null || incRowPrefab == null) return;
            foreach (Transform c in incListParent) Destroy(c.gameObject);
            foreach (var i in _incidentList)
            {
                var row = Instantiate(incRowPrefab, incListParent); row.gameObject.SetActive(true);
                var t = row.GetComponentsInChildren<Text>();
                if (t.Length > 0) t[0].text = i.title;
                if (t.Length > 1) { t[1].text = i.severity.ToString(); t[1].color = SevColor(i.severity); }
                if (t.Length > 2) { t[2].text = i.state.ToString(); t[2].color = i.state == IncidentState.Resolved ? new Color(0.45f,0.85f,0.5f) : new Color(0.95f,0.8f,0.35f); }
            }
        }

        private void RefreshRollout()
        {
            if (rolloutStageText) rolloutStageText.text = $"Stage: {_rollout.stage}";
            if (rolloutPctText) rolloutPctText.text = $"{_rollout.Percent * 100:0}% of users";
            if (rolloutBar) { rolloutBar.fillAmount = _rollout.Percent; rolloutBar.color = _rollout.stage == RolloutStage.KilledSwitch ? new Color(0.9f,0.4f,0.4f) : new Color(0.45f,0.85f,0.5f); }
            if (rolloutStageText && _rollout.stage == RolloutStage.KilledSwitch) rolloutStageText.color = new Color(0.9f,0.4f,0.4f);
            if (rolloutAdvanceButton) rolloutAdvanceButton.interactable = _rollout.stage != RolloutStage.KilledSwitch && _rollout.stage != RolloutStage.Full;
        }

        private void RefreshClans()
        {
            if (clanListParent == null || clanRowPrefab == null) return;
            foreach (Transform c in clanListParent) Destroy(c.gameObject);
            var board = _clanSeason.Leaderboard();
            for (int i = 0; i < board.Count; i++)
            {
                var row = Instantiate(clanRowPrefab, clanListParent); row.gameObject.SetActive(true);
                var t = row.GetComponentsInChildren<Text>();
                if (t.Length > 0) t[0].text = $"#{i + 1}  {board[i].clanId}";
                if (t.Length > 1) t[1].text = $"{board[i].warPoints} pts";
                if (t.Length > 2) t[2].text = $"{board[i].warRating} WR";
            }
        }

        private static Color PriColor(float p) => p >= 0.7f ? new Color(0.9f,0.4f,0.4f) : p >= 0.45f ? new Color(0.95f,0.6f,0.35f) : new Color(0.95f,0.8f,0.35f);
        private static Color ActionColor(ModAction a) => a switch { ModAction.PermBan => new Color(0.9f,0.35f,0.35f), ModAction.TempBan => new Color(0.9f,0.5f,0.35f), ModAction.Mute => new Color(0.95f,0.75f,0.35f), ModAction.EscalateHuman => new Color(0.6f,0.7f,1f), _ => new Color(0.7f,0.72f,0.8f) };
        private static Color SevColor(Severity s) => s switch { Severity.Sev1 => new Color(0.9f,0.35f,0.35f), Severity.Sev2 => new Color(0.9f,0.55f,0.35f), Severity.Sev3 => new Color(0.95f,0.8f,0.35f), _ => new Color(0.7f,0.72f,0.8f) };
    }
}
