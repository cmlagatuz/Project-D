// Project D  |  Codename: Zantetsuken
// P4DemoDriver.cs  (P4 UI — populates whichever P4 views are present with sample data)

using System.Collections.Generic;
using UnityEngine;
using ProjectD.P4.Tournaments;
using ProjectD.P4.Lobby;
using ProjectD.P4.Servers;
using ProjectD.P4.Support;
using ProjectD.P4UI;

namespace ProjectD.P4UI
{
    public sealed class P4DemoDriver : MonoBehaviour
    {
        [SerializeField] private TournamentBracketView bracket;
        [SerializeField] private CustomLobbyView lobby;
        [SerializeField] private ServerPickerView servers;
        [SerializeField] private SupportInboxView support;

        private void Start()
        {
            if (bracket)
            {
                var t = new Tournament { name = "Crystal Cup" };
                t.Seed(new[] { "Aeon", "Bahamut", "Cactuar", "Diablos", "Espers", "Fenrir", "Gilgamesh" });
                bracket.Bind(t);
            }
            if (lobby)
            {
                var l = new CustomLobby { perTeam = 5, maxSpectators = 3, joinCode = CustomLobby.GenerateCode(2025) };
                l.Init();
                l.Join("z", "Zantetsuken", TeamSide.Blue); l.Join("d", "DragoonX", TeamSide.Blue);
                l.Join("w", "WildRose", TeamSide.Red);
                lobby.Bind(l);
                lobby.OnStart += () => Debug.Log("[Project D] Lobby start -> load match.");
            }
            if (servers)
            {
                var s = new ServerSelection();
                s.Register(new ServerRegion { code = "NA", name = "North America", pingMs = 62, online = true, loadPct = 0.4f });
                s.Register(new ServerRegion { code = "EU", name = "Europe West", pingMs = 118, online = true, loadPct = 0.6f });
                s.Register(new ServerRegion { code = "SEA", name = "Southeast Asia", pingMs = 34, online = true, loadPct = 0.5f });
                s.Register(new ServerRegion { code = "KR", name = "Korea", pingMs = 45, online = true, loadPct = 0.7f });
                s.Register(new ServerRegion { code = "BR", name = "Brazil", pingMs = 150, online = false });
                servers.Bind(s);
                servers.OnRegionSelected += code => Debug.Log($"[Project D] Selected region: {code}");
            }
            if (support)
            {
                var sys = new SupportSystem(); long now = 1000;
                var t1 = sys.Open("me", TicketCategory.Billing, "Double charge on KP bundle", "I was charged twice.", now);
                sys.Reply(t1.id, "staff", "Thanks, we're reviewing your receipts.", true, now + 60);
                var t2 = sys.Open("me", TicketCategory.Bug, "Kain ult stuck airborne", "Happened twice this week.", now + 100);
                var t3 = sys.Open("me", TicketCategory.Report, "Toxic player follow-up", "Reported last game.", now + 200);
                sys.Resolve(t3.id, now + 300);
                support.Bind(sys, now + 400);
                support.SetVisibleTickets(new List<Ticket> { t2, t1, t3 });
            }
        }
    }
}
