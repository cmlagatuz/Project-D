// Project D  |  Codename: Zantetsuken
// MockApexData.cs  (Leaderboard — deterministic demo pool)
//
// Generates a believable apex pool (Adamantite + Crystal players) for the demo /
// tests: varied champions, lanes, regions, LP, and a handful marked as friends and
// one as the viewer. Deterministic (seeded) so the board is stable across runs.
// In production this data comes from the server's ladder snapshot.

using System.Collections.Generic;
using ProjectD.Ranked;

namespace ProjectD.Leaderboard
{
    public static class MockApexData
    {
        private static readonly string[] Champions =
            { "wol", "garland", "firion", "emperor", "onion_knight", "cloud_of_darkness", "cecil", "kain", "bartz", "exdeath" };
        private static readonly Lane[] Lanes = { Lane.Mid, Lane.Baron, Lane.Dragon, Lane.Support, Lane.Jungle };
        private static readonly string[] Regions = { "NA", "EU", "SEA", "KR", "BR" };
        private static readonly string[] Handles =
        {
            "Zantetsuken", "VoidWalker", "CrystalKing", "DragoonX", "OnionMaster", "PaladinCecil",
            "WildRose", "ChaosBringer", "MimicLord", "TidalForce", "AeonReaver", "GungnirPro",
            "MythrilHeart", "AdamantGuard", "SummonerY", "RiverGuard", "ApexJungle", "MidDiff",
            "SupDiff", "BaronNashor", "FreelancerB", "ExdeathTT", "GarlandGod", "FirionFan"
        };

        public static List<LeaderboardEntry> Generate(string viewerId = "you", int count = 60, int seed = 20250823)
        {
            var rng = new System.Random(seed);
            var pool = new List<LeaderboardEntry>(count);

            for (int i = 0; i < count; i++)
            {
                // Bias the top of the board to Crystal, the rest to Adamantite.
                bool crystal = i < count * 0.55;
                var tier = crystal ? RankTier.Crystal : RankTier.Adamantite;
                var div = RandomApexDivision(rng);
                int lp = rng.Next(0, RankLadder.ApexLpPerDivision);   // 0..49
                int step = StepIndex(tier, div);
                int wins = rng.Next(40, 260);
                int losses = rng.Next(30, wins);
                bool friend = rng.NextDouble() < 0.18;

                string id = i == 0 ? viewerId : $"p{i:D2}";
                string name = i == 0 ? "You" : Handles[rng.Next(Handles.Length)] + rng.Next(1, 99);

                pool.Add(new LeaderboardEntry
                {
                    playerId = id,
                    displayName = name,
                    region = Regions[rng.Next(Regions.Length)],
                    tier = tier,
                    division = div,
                    lp = lp,
                    mmr = 1780 + step * 6 + lp + rng.Next(-20, 20),
                    globalStep = step,
                    wins = wins,
                    losses = losses,
                    topChampionId = Champions[rng.Next(Champions.Length)],
                    mainLane = Lanes[rng.Next(Lanes.Length)],
                    isFriend = friend || i == 0,
                    lastActiveUnix = 1_700_000_000 + rng.Next(0, 500_000)
                });
            }
            return pool;
        }

        private static Division RandomApexDivision(System.Random rng)
        {
            // E..A weighted toward the middle.
            var divs = new[] { Division.E, Division.D, Division.C, Division.B, Division.A };
            return divs[rng.Next(divs.Length)];
        }

        private static int StepIndex(RankTier tier, Division div)
        {
            for (int i = 0; i < RankLadder.StepCount; i++)
            { var s = RankLadder.StepAt(i); if (s.Tier == tier && s.Division == div) return i; }
            return 0;
        }
    }
}
