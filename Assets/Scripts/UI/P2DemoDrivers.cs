// Project D  |  Codename: Zantetsuken
// P2DemoDrivers.cs  (P2 sandboxes: ping wheel, graphs, tutorial)
//
// Feeds sample data so each P2 screen runs standalone. HUD widgets + matchmaking
// self-drive from their views; these drivers cover the graph + tutorial + ping logging.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.CommsUI;
using ProjectD.Comms;
using ProjectD.Graphs;
using ProjectD.GraphsUI;
using ProjectD.Tutorial;
using ProjectD.TutorialUI;

namespace ProjectD.P2UI
{
    public sealed class PingWheelDemoDriver : MonoBehaviour
    {
        [SerializeField] private PingWheel wheel;
        private void Start()
        {
            if (wheel == null) return;
            wheel.SetOpen(true);
            wheel.OnPing += t => { var ev = QuickChat.Build(t, "me", 0, 0, 0); Debug.Log($"[Project D] Ping: {QuickChat.Label(t)} (team-wide={ev.TeamWide})"); };
            wheel.OnQuickChat += k => Debug.Log($"[Project D] Quick chat: {k}");
            wheel.OnSfxCue += k => Debug.Log($"[Project D] SFX: {k}");
        }
    }

    public sealed class GraphDemoDriver : MonoBehaviour
    {
        [SerializeField] private MatchGraphView view;
        private void Start()
        {
            if (view == null) return;
            var g = new MatchGraphModel();
            int blue = 500, red = 500; var rng = new System.Random(7);
            for (int t = 0; t <= 1800; t += 60)
            {
                blue += rng.Next(200, 700); red += rng.Next(180, 680);
                g.SampleGold(t, blue, red); g.SampleXp(t, (int)(blue*0.9f), (int)(red*0.9f));
            }
            var b = new EconomyBreakdown();
            b.AddGold(GoldSource.Minions, 8200); b.AddGold(GoldSource.Kills, 3100);
            b.AddGold(GoldSource.Objectives, 2400); b.AddGold(GoldSource.Assists, 1500); b.AddGold(GoldSource.Passive, 1800);
            view.Bind(g, b);
        }
    }

    public sealed class TutorialDemoDriver : MonoBehaviour
    {
        [SerializeField] private TutorialView view;
        private void Start()
        {
            if (view == null) return;
            var steps = new List<TutorialStep>
            {
                new TutorialStep{ stepId="move", titleKey="Move", bodyKey="Drag the left stick to move to the marker.", completeOn=TutorialTrigger.Move, requiredCount=1, rewardXp=50 },
                new TutorialStep{ stepId="attack", titleKey="Attack", bodyKey="Tap an enemy to attack it.", completeOn=TutorialTrigger.BasicAttack, requiredCount=3, rewardXp=60 },
                new TutorialStep{ stepId="cs", titleKey="Last Hit", bodyKey="Last-hit 3 minions for gold.", completeOn=TutorialTrigger.LastHitMinion, requiredCount=3, rewardXp=80, rewardKp=10 },
                new TutorialStep{ stepId="cast", titleKey="Use an Ability", bodyKey="Cast your Q ability.", completeOn=TutorialTrigger.CastAbility, requiredCount=1, rewardXp=60 },
                new TutorialStep{ stepId="recall", titleKey="Recall", bodyKey="Recall to base.", completeOn=TutorialTrigger.Recall, requiredCount=1, rewardXp=60 },
                new TutorialStep{ stepId="ping", titleKey="Ping", bodyKey="Use a ping to communicate.", completeOn=TutorialTrigger.UsePing, requiredCount=1, rewardXp=40, rewardKp=20 },
            };
            view.OnCompleted += (xp, kp) => Debug.Log($"[Project D] Tutorial done -> +{xp} XP, +{kp} KP to account");
            view.Bind(steps);
        }
    }
}
