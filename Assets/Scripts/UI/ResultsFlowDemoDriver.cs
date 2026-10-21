// Project D  |  Codename: Zantetsuken
// ResultsFlowDemoDriver.cs  (PostGame UI — drives the results -> analytics flow demo)
//
// Builds a sample finished match and presents the PostGameResultsScreen. Tapping
// "VIEW MATCH ANALYTICS" swaps to the polished analytics view; "BACK" returns.

using UnityEngine;
using ProjectD.PostGame;

namespace ProjectD.PostGameUI
{
    public sealed class ResultsFlowDemoDriver : MonoBehaviour
    {
        [SerializeField] private PostGameResultsScreen screen;
        [SerializeField] private string localPlayerId = "me";

        private void Start()
        {
            if (screen == null) return;
            screen.SetLocalPlayer(localPlayerId);
            screen.OnContinue += () => Debug.Log("[Project D] Continue -> return to menu / hand XP to leveling.");
            screen.Present(SampleMatch());
        }

        private static MatchAnalytics SampleMatch()
        {
            var m = new MatchAnalytics { matchId = "M-2049", winningTeam = Team.Blue, durationSeconds = 1875f };
            m.Blue.AddRange(new[]
            {
                L("me","You","kain","Jungle",Team.Blue, 12,3,8, 210,215, 14200, 28000, 3, 2, 1520),
                L("b_mid","MidDiff","emperor","Mid",Team.Blue, 8,4,9, 235,240, 13100, 24000, 2, 1, 1480),
                L("b_bar","WildRose","wol","Baron",Team.Blue, 3,5,14, 190,195, 10500, 9000, 4, 1, 1510),
                L("b_dra","DragoonX","firion","Dragon",Team.Blue, 9,6,6, 245,250, 12800, 26000, 1, 0, 1495),
                L("b_sup","SupDiff","cecil","Support",Team.Blue, 1,7,20, 35,40, 8200, 6000, 6, 0, 1470),
            });
            m.Red.AddRange(new[]
            {
                L("r_jg","OnionMaster","onion_knight","Jungle",Team.Red, 7,8,6, 195,200, 11500, 19000, 1, 0, 1500),
                L("r_mid","VoidWalker","cloud_of_darkness","Mid",Team.Red, 10,6,5, 230,235, 12900, 27000, 2, 1, 1550),
                L("r_bar","ChaosBringer","garland","Baron",Team.Red, 5,7,7, 205,210, 11000, 15000, 0, 0, 1490),
                L("r_dra","MimicLord","bartz","Dragon",Team.Red, 6,9,4, 240,245, 11800, 21000, 1, 0, 1520),
                L("r_sup","TidalForce","exdeath","Support",Team.Red, 2,8,12, 50,55, 8500, 7000, 0, 0, 1465),
            });
            int blue = 500, red = 500; var rng = new System.Random(7);
            for (int t = 0; t <= 1800; t += 60) { blue += rng.Next(280, 640); red += rng.Next(240, 600); m.Timeline.Add(new LeadSample { t = t, goldDiff = blue - red, xpDiff = (int)((blue - red) * 0.85f) }); }
            return m;
        }
        private static PlayerLine L(string id, string name, string champ, string role, Team team, int k, int d, int a, int csAt10, int cs, int gold, int dmg, int vision, int obj, int mmr)
            => new PlayerLine { playerId = id, displayName = name, championId = champ, role = role, team = team, kills = k, deaths = d, assists = a, csAt10 = csAt10, cs = cs, gold = gold, championDamage = dmg, visionScore = vision, objectivesTaken = obj, mmr = mmr };
    }
}
