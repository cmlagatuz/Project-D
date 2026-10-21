// Project D  |  Codename: Zantetsuken
// MatchAnalyticsDemoDriver.cs  (PostGame UI — feeds a sample finished match)
//
// Builds a believable 5v5 MatchAnalytics payload (Blue wins) and presents it, as if the
// player tapped "View Match Analytics" on the post-game results screen. In a real build
// the server supplies this from the match record.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.PostGame;
using ProjectD.PostGameUI;

namespace ProjectD.PostGameUI
{
    public sealed class MatchAnalyticsDemoDriver : MonoBehaviour
    {
        [SerializeField] private MatchAnalyticsView view;
        [SerializeField] private string localPlayerId = "me";

        private void Start()
        {
            if (view == null) return;
            view.SetLocalPlayer(localPlayerId);

            var m = new MatchAnalytics { matchId = "M-2049", winningTeam = Team.Blue, durationSeconds = 1875f };

            m.Blue.AddRange(new[]
            {
                Line("me","You","kain","Jungle",Team.Blue, 12,3,8, 210,215, 14200, 28000, 3, 2, 1520),
                Line("b_mid","MidDiff","emperor","Mid",Team.Blue, 8,4,9, 235,240, 13100, 24000, 2, 1, 1480),
                Line("b_bar","WildRose","wol","Baron",Team.Blue, 3,5,14, 190,195, 10500, 9000, 4, 1, 1510),
                Line("b_dra","DragoonX","firion","Dragon",Team.Blue, 9,6,6, 245,250, 12800, 26000, 1, 0, 1495),
                Line("b_sup","SupDiff","cecil","Support",Team.Blue, 1,7,20, 35,40, 8200, 6000, 6, 0, 1470),
            });
            m.Red.AddRange(new[]
            {
                Line("r_jg","OnionMaster","onion_knight","Jungle",Team.Red, 7,8,6, 195,200, 11500, 19000, 1, 0, 1500),
                Line("r_mid","VoidWalker","cloud_of_darkness","Mid",Team.Red, 10,6,5, 230,235, 12900, 27000, 2, 1, 1550),
                Line("r_bar","ChaosBringer","garland","Baron",Team.Red, 5,7,7, 205,210, 11000, 15000, 0, 0, 1490),
                Line("r_dra","MimicLord","bartz","Dragon",Team.Red, 6,9,4, 240,245, 11800, 21000, 1, 0, 1520),
                Line("r_sup","TidalForce","exdeath","Support",Team.Red, 2,8,12, 50,55, 8500, 7000, 0, 0, 1465),
            });

            // Gold-lead timeline (blue - red), sampled each minute — blue pulls ahead late.
            int blue = 500, red = 500; var rng = new System.Random(7);
            for (int t = 0; t <= 1800; t += 60)
            {
                blue += rng.Next(280, 640); red += rng.Next(240, 600);
                m.Timeline.Add(new LeadSample { t = t, goldDiff = blue - red, xpDiff = (int)((blue - red) * 0.85f) });
            }

            view.Present(m);
        }

        private static PlayerLine Line(string id, string name, string champ, string role, Team team, int k, int d, int a, int csAt10, int cs, int gold, int dmg, int vision, int obj, int mmr)
            => new PlayerLine { playerId = id, displayName = name, championId = champ, role = role, team = team, kills = k, deaths = d, assists = a, csAt10 = csAt10, cs = cs, gold = gold, championDamage = dmg, visionScore = vision, objectivesTaken = obj, mmr = mmr };
    }
}
