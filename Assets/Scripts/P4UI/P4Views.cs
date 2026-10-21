// Project D  |  Codename: Zantetsuken
// P4Views.cs  (P4 UI — views bound to the P4 platform cores)
//
// Four platform-ops screens wired to the tested P4 systems:
//   - TournamentBracketView : round columns of match cells; report winners -> auto-advance.
//   - CustomLobbyView       : blue/red/spectator slots + join code + start gate.
//   - ServerPickerView      : region rows with ping + quality; recommends the best.
//   - SupportInboxView      : ticket list with state; open a ticket + reply.
// Server-authoritative in a real build; these render/act on the server-owned models.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P4.Tournaments;
using ProjectD.P4.Lobby;
using ProjectD.P4.Servers;
using ProjectD.P4.Support;

namespace ProjectD.P4UI
{
    // ---------------- Tournament bracket ----------------
    public sealed class TournamentBracketView : MonoBehaviour
    {
        [SerializeField] private RectTransform roundsParent;   // horizontal: one column per round
        [SerializeField] private RectTransform columnPrefab;   // vertical container
        [SerializeField] private Button matchCellPrefab;       // has 2 Text (teamA/teamB) + we tint winner
        [SerializeField] private Text championText;

        private Tournament _t;

        public void Bind(Tournament t) { _t = t; Rebuild(); }

        private void Rebuild()
        {
            if (roundsParent == null) return;
            foreach (Transform c in roundsParent) Destroy(c.gameObject);

            for (int round = 1; round <= _t.Rounds; round++)
            {
                var col = Instantiate(columnPrefab, roundsParent);
                col.gameObject.SetActive(true);
                var header = col.GetComponentInChildren<Text>();
                if (header) header.text = RoundName(round, _t.Rounds);

                var matches = new List<BracketMatch>(_t.RoundMatches(round));
                matches.Sort((a, b) => a.index - b.index);
                foreach (var m in matches)
                {
                    var cell = Instantiate(matchCellPrefab, col);
                    cell.gameObject.SetActive(true);
                    var texts = cell.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) texts[0].text = m.teamA ?? "—";
                    if (texts.Length > 1) texts[1].text = m.teamB ?? "—";
                    if (m.Decided) HighlightWinner(texts, m);
                    int r = round, idx = m.index; var mm = m;
                    cell.onClick.AddListener(() => CycleWinner(mm, r, idx, texts));
                }
            }
            if (championText) championText.text = _t.Champion() != null ? $"Champion: {_t.Champion()}" : "";
        }

        private void CycleWinner(BracketMatch m, int round, int index, Text[] texts)
        {
            if (string.IsNullOrEmpty(m.teamA) || string.IsNullOrEmpty(m.teamB)) return; // bye / not ready
            string next = m.winner == m.teamA ? m.teamB : m.teamA;   // toggle A/B
            if (_t.ReportResult(round, index, next)) Rebuild();
        }

        private static void HighlightWinner(Text[] texts, BracketMatch m)
        {
            var win = new Color(0.55f, 0.85f, 0.6f); var dim = new Color(0.6f, 0.62f, 0.68f);
            if (texts.Length > 0) texts[0].color = m.winner == m.teamA ? win : dim;
            if (texts.Length > 1) texts[1].color = m.winner == m.teamB ? win : dim;
        }

        private static string RoundName(int round, int total)
        {
            int from = total - round;
            return from switch { 0 => "Final", 1 => "Semifinals", 2 => "Quarterfinals", _ => $"Round {round}" };
        }
    }

    // ---------------- Custom lobby ----------------
    public sealed class CustomLobbyView : MonoBehaviour
    {
        [SerializeField] private RectTransform blueParent, redParent, specParent;
        [SerializeField] private RectTransform slotPrefab;      // has a Text
        [SerializeField] private Text codeText, statusText;
        [SerializeField] private Button startButton, joinBlueButton, joinRedButton, joinSpecButton;
        [SerializeField] private string localPlayerId = "me", localName = "You";

        private CustomLobby _lobby;
        public event System.Action OnStart;

        public void Bind(CustomLobby lobby)
        {
            _lobby = lobby;
            if (joinBlueButton) joinBlueButton.onClick.AddListener(() => JoinOrMove(TeamSide.Blue));
            if (joinRedButton) joinRedButton.onClick.AddListener(() => JoinOrMove(TeamSide.Red));
            if (joinSpecButton) joinSpecButton.onClick.AddListener(() => JoinOrMove(TeamSide.Spectator));
            if (startButton) startButton.onClick.AddListener(() => { if (_lobby.CanStart()) OnStart?.Invoke(); });
            Refresh();
        }

        private void JoinOrMove(TeamSide side)
        {
            bool present = false;
            foreach (var s in All()) if (s.playerId == localPlayerId) { present = true; break; }
            if (present) _lobby.MoveTo(localPlayerId, side); else _lobby.Join(localPlayerId, localName, side);
            Refresh();
        }

        private void Refresh()
        {
            FillColumn(blueParent, _lobby.Blue);
            FillColumn(redParent, _lobby.Red);
            FillColumn(specParent, _lobby.Spectators);
            if (codeText) codeText.text = $"Join Code: {_lobby.joinCode}";
            if (statusText) statusText.text = _lobby.CanStart() ? "Ready to start!" : "Fill both teams to start.";
            if (startButton) startButton.interactable = _lobby.CanStart();
        }

        private void FillColumn(RectTransform parent, IReadOnlyList<LobbySlot> slots)
        {
            if (parent == null) return;
            foreach (Transform c in parent) Destroy(c.gameObject);
            foreach (var s in slots)
            {
                var row = Instantiate(slotPrefab, parent);
                row.gameObject.SetActive(true);
                var t = row.GetComponentInChildren<Text>();
                if (t) t.text = s.IsEmpty ? "— open —" : s.displayName;
            }
        }
        private IEnumerable<LobbySlot> All()
        {
            foreach (var s in _lobby.Blue) yield return s;
            foreach (var s in _lobby.Red) yield return s;
            foreach (var s in _lobby.Spectators) yield return s;
        }
    }

    // ---------------- Server picker ----------------
    public sealed class ServerPickerView : MonoBehaviour
    {
        [SerializeField] private RectTransform rowParent;
        [SerializeField] private Button rowPrefab;             // [region name][ping][quality]
        [SerializeField] private Text recommendedText;

        private ServerSelection _sel;
        public event System.Action<string> OnRegionSelected;

        public void Bind(ServerSelection sel)
        {
            _sel = sel;
            if (rowParent == null) return;
            foreach (Transform c in rowParent) Destroy(c.gameObject);
            var rec = _sel.Recommended();
            foreach (var r in _sel.All)
            {
                var row = Instantiate(rowPrefab, rowParent); row.gameObject.SetActive(true);
                var t = row.GetComponentsInChildren<Text>();
                if (t.Length > 0) t[0].text = string.IsNullOrEmpty(r.name) ? r.code : $"{r.name} ({r.code})";
                if (t.Length > 1) t[1].text = r.online ? $"{r.pingMs} ms" : "Offline";
                if (t.Length > 2) { t[2].text = r.online ? ServerSelection.Quality(r.pingMs) : "—"; t[2].color = QualityColor(r.online ? r.pingMs : 9999); }
                var bg = row.GetComponent<Image>();
                if (bg) bg.color = (rec != null && r.code == rec.code) ? new Color(0.18f,0.28f,0.20f,0.95f) : new Color(0.13f,0.15f,0.21f,0.9f);
                row.interactable = r.online;
                string code = r.code;
                row.onClick.AddListener(() => OnRegionSelected?.Invoke(code));
            }
            if (recommendedText) recommendedText.text = rec != null ? $"Recommended: {rec.code} ({rec.pingMs} ms · {ServerSelection.Quality(rec.pingMs)})" : "No server available";
        }

        private static Color QualityColor(int ping) => ping < 40 ? new Color(0.45f,0.85f,0.5f) : ping < 80 ? new Color(0.6f,0.85f,0.5f) : ping < 140 ? new Color(0.95f,0.8f,0.35f) : new Color(0.9f,0.45f,0.4f);
    }

    // ---------------- Support inbox ----------------
    public sealed class SupportInboxView : MonoBehaviour
    {
        [SerializeField] private RectTransform listParent;
        [SerializeField] private Button ticketRowPrefab;       // [subject][state]
        [SerializeField] private Text openCountText;
        [SerializeField] private RectTransform threadParent;   // messages of the open ticket
        [SerializeField] private RectTransform messagePrefab;  // a Text
        [SerializeField] private InputField replyInput;
        [SerializeField] private Button replyButton;
        [SerializeField] private string localPlayerId = "me";

        private SupportSystem _sys; private long _now; private Ticket _open;

        public void Bind(SupportSystem sys, long nowUnix)
        {
            _sys = sys; _now = nowUnix;
            if (replyButton) replyButton.onClick.AddListener(SendReply);
            RefreshList();
        }

        private void RefreshList()
        {
            if (listParent != null)
            {
                foreach (Transform c in listParent) Destroy(c.gameObject);
                foreach (var t in AllTickets())
                {
                    var row = Instantiate(ticketRowPrefab, listParent); row.gameObject.SetActive(true);
                    var labels = row.GetComponentsInChildren<Text>();
                    if (labels.Length > 0) labels[0].text = $"[{t.category}] {t.subject}";
                    if (labels.Length > 1) { labels[1].text = t.state.ToString(); labels[1].color = StateColor(t.state); }
                    var tk = t;
                    row.onClick.AddListener(() => OpenThread(tk));
                }
            }
            if (openCountText) openCountText.text = $"Open tickets: {_sys.OpenCount()}";
        }

        private void OpenThread(Ticket t)
        {
            _open = t;
            if (threadParent == null) return;
            foreach (Transform c in threadParent) Destroy(c.gameObject);
            foreach (var msg in t.Messages)
            {
                var row = Instantiate(messagePrefab, threadParent); row.gameObject.SetActive(true);
                var txt = row.GetComponentInChildren<Text>();
                if (txt) { txt.text = (msg.fromStaff ? "Support: " : "You: ") + msg.body; txt.color = msg.fromStaff ? new Color(0.6f,0.8f,1f) : Color.white; }
            }
        }

        private void SendReply()
        {
            if (_open == null || replyInput == null || string.IsNullOrWhiteSpace(replyInput.text)) return;
            if (_sys.Reply(_open.id, localPlayerId, replyInput.text.Trim(), fromStaff:false, _now))
            {
                replyInput.text = "";
                OpenThread(_open); RefreshList();
            }
        }

        // Demo: list all tickets (server would scope to the player).
        private IEnumerable<Ticket> AllTickets()
        {
            var seen = new List<Ticket>();
            // SupportSystem has no public enumerator; expose via a helper on the demo driver instead.
            return _tickets ?? seen;
        }
        private IReadOnlyList<Ticket> _tickets;
        public void SetVisibleTickets(IReadOnlyList<Ticket> tickets) { _tickets = tickets; RefreshList(); }

        private static Color StateColor(TicketState s) => s switch
        {
            TicketState.Open => new Color(0.95f,0.8f,0.35f),
            TicketState.InProgress => new Color(0.6f,0.8f,1f),
            TicketState.AwaitingPlayer => new Color(0.9f,0.6f,0.4f),
            TicketState.Resolved => new Color(0.55f,0.85f,0.6f),
            _ => new Color(0.6f,0.62f,0.68f)
        };
    }
}
