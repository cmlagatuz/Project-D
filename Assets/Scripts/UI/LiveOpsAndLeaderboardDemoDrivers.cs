// Project D  |  Codename: Zantetsuken
// LiveOpsAndLeaderboardDemoDrivers.cs  (Live-ops + Champion leaderboard sandboxes)
//
// Feeds sample data so you can see missions/weekly-chest and the per-champion Server
// No.1 board. In a real build the server tracks mission progress from matches and owns
// the champion-rating snapshot; these drivers just populate believable content.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.LiveOps;
using ProjectD.LiveOpsUI;
using ProjectD.LeaderboardX;
using ProjectD.LeaderboardXUI;

namespace ProjectD.P1UI2
{
    public sealed class LiveOpsDemoDriver : MonoBehaviour
    {
        [SerializeField] private LiveOpsScreen screen;

        private void Start()
        {
            if (screen == null) return;
            var defs = new List<MissionDefinition>
            {
                Mission("d_win", MissionCadence.Daily, "Win 2 Matches", "Win 2 games in any mode.", 2, Xp(500), Kp(20)),
                Mission("d_cs", MissionCadence.Daily, "Last-hit 150 Minions", "Across your games today.", 150, Xp(300), Frag(1)),
                Mission("d_kills", MissionCadence.Daily, "Score 15 Takedowns", "Kills or assists.", 15, Xp(300)),
                Mission("w_play", MissionCadence.Weekly, "Play 10 Ranked Games", "Any result counts.", 10, Xp(1500), Kp(150), Frag(2)),
                Mission("w_obj", MissionCadence.Weekly, "Slay 5 Summons/Divines", "Objective takedowns.", 5, Xp(1200), Frag(2)),
                Mission("e_event", MissionCadence.Event, "Crystal Festival: 3 Wins", "Limited-time event.", 3, Kp(300), Frag(1)),
            };
            var chest = new WeeklyChestSeed { fragmentsPerChest = 4, weeklyCap = 2, fragments = 2, chests = 1, keys = 2 };
            screen.Bind(defs, chest);

            // Pre-complete a couple so CLAIM is visible immediately.
            screen.SimulateProgress("d_win", 2);
            screen.SimulateProgress("d_kills", 15);

            screen.OnRewardGranted += (kind, amt) => Debug.Log($"[Project D] Reward granted: {kind} +{amt} -> account/chest");
        }

        static MissionDefinition Mission(string id, MissionCadence c, string title, string desc, int target, params MissionReward[] rewards)
            => new MissionDefinition { missionId = id, cadence = c, title = title, description = desc, targetProgress = target, rewards = rewards };
        static MissionReward Xp(int a) => new MissionReward { kind = MissionRewardKind.AccountXp, amount = a };
        static MissionReward Kp(int a) => new MissionReward { kind = MissionRewardKind.KP, amount = a };
        static MissionReward Frag(int a) => new MissionReward { kind = MissionRewardKind.ChestFragment, amount = a };
    }

    public sealed class ChampionLeaderboardDemoDriver : MonoBehaviour
    {
        [SerializeField] private ChampionLeaderboardScreen screen;

        private void Start()
        {
            if (screen == null) return;
            var champs = new[] { "wol","garland","firion","emperor","onion_knight","cloud_of_darkness","cecil","kain","bartz","exdeath" };
            var regions = new[] { "NA","EU","SEA","KR","BR" };
            var handles = new[] { "Zantetsuken","VoidWalker","CrystalKing","DragoonX","OnionMaster","WildRose","ChaosBringer","MimicLord","TidalForce","GungnirPro","MythrilHeart","ApexJungle" };
            var rng = new System.Random(20250824);
            var pool = new List<ChampionRating>();
            foreach (var champ in champs)
            {
                // several players per champion so focus mode has a ladder.
                int n = rng.Next(6, 12);
                for (int i = 0; i < n; i++)
                {
                    int games = rng.Next(60, 320);
                    int wins = rng.Next(games / 2, games);
                    pool.Add(new ChampionRating
                    {
                        PlayerId = $"{champ}_{i}", DisplayName = handles[rng.Next(handles.Length)] + rng.Next(1, 99),
                        ChampionId = champ, Region = regions[rng.Next(regions.Length)],
                        Rating = 1600 + rng.Next(0, 500), Games = games, Wins = wins,
                        RankLabel = new[] { "Adamantite A","Crystal E","Crystal C","Crystal A","Mythril A" }[rng.Next(5)],
                        LastPlayedUnix = 1_700_000_000 + rng.Next(0, 500000)
                    });
                }
            }
            screen.Bind(pool, regions);
        }
    }
}
