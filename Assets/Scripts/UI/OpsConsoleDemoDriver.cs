// Project D  |  Codename: Zantetsuken
// OpsConsoleDemoDriver.cs  (P12 UI — seeds the ops console with real P12 core data)
//
// Populates the console from the P12 cores: scheduled events, triaged moderation cases,
// sev-tiered incidents, an experiment rollout, and a clan-war season. Operator actions
// (advance clock/incident, ramp/kill rollout) then mutate the real cores live.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.P12.Automation;
using ProjectD.P12.Moderation;
using ProjectD.P12.Incidents;
using ProjectD.P12.Rollout;
using ProjectD.P12.ClanWars;
using ProjectD.P12UI;

namespace ProjectD.P12UI
{
    public sealed class OpsConsoleDemoDriver : MonoBehaviour
    {
        [SerializeField] private OpsConsoleView view;

        private void Start()
        {
            if (view == null) return;
            long now = 1_000_000;

            // Live-ops schedule.
            var sched = new LiveOpsScheduler();
            var e1 = new ScheduledEvent { id = "crystal_fest", name = "Crystal Festival", startUnix = now - 3600, endUnix = now + 86400 };
            var e2 = new ScheduledEvent { id = "double_xp", name = "Double XP Weekend", startUnix = now + 7200, endUnix = now + 172800 };
            var e3 = new ScheduledEvent { id = "clash", name = "Weekly Clash", kind = ScheduleKind.Recurring, startUnix = now - 100000, endUnix = now - 90000, recurEverySec = 604800 };
            sched.Schedule(e1); sched.Schedule(e2); sched.Schedule(e3);
            sched.Tick(now);
            view.RegisterScheduled(e1); view.RegisterScheduled(e2); view.RegisterScheduled(e3);

            // Moderation queue.
            var mod = new ModerationQueue();
            mod.Enqueue(new ReportCase { id = "c1", offenderId = "player42", category = ReportCategory.Chat, reportCount = 2, behaviorScore = 0.5f, createdUnix = now - 3600 }, now);
            mod.Enqueue(new ReportCase { id = "c2", offenderId = "cheater7", category = ReportCategory.Cheating, reportCount = 4, behaviorScore = 0.2f, antiCheatSeverity = 2.8f, createdUnix = now - 7200 }, now);
            mod.Enqueue(new ReportCase { id = "c3", offenderId = "afker3", category = ReportCategory.Gameplay, reportCount = 6, behaviorScore = 0.3f, createdUnix = now - 1800 }, now);
            mod.Enqueue(new ReportCase { id = "c4", offenderId = "namebad", category = ReportCategory.Name, reportCount = 1, behaviorScore = 0.6f, createdUnix = now - 500 }, now);

            // Incidents.
            var incidents = new IncidentManager();
            var incList = new List<Incident>
            {
                incidents.Open("i1", "Login latency spike", Severity.Sev2, now - 1800),
                incidents.Open("i2", "Store checkout errors", Severity.Sev1, now - 900),
                incidents.Open("i3", "Cosmetic art missing", Severity.Sev4, now - 600),
            };

            // Experiment rollout.
            var rollout = new Rollout { key = "new_shop_ui", guardrailThreshold = 0.05f };
            rollout.Advance(0.01f);   // -> Canary

            // Clan war season.
            var season = new ClanWarSeason { id = "cw1", startUnix = now - 100000, endUnix = now + 500000 };
            season.Register("Dragoons", 1650); season.Register("Moogles", 1500); season.Register("Espers", 1580); season.Register("Cactuars", 1420);
            season.Advance(now);
            season.ReportBattle("Dragoons", true, 1580);
            season.ReportBattle("Espers", true, 1500);
            season.ReportBattle("Moogles", true, 1420);
            season.ReportBattle("Dragoons", true, 1500);

            view.Bind(sched, mod, incidents, incList, rollout, season);
        }
    }
}
