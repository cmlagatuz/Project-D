// Project D  |  Codename: Zantetsuken
// MatchAnalytics.cs  (PostGame — a single match's analytics payload)
//
// The data the post-game "Match Analytics" screen renders for ONE finished match.
// Server-authoritative in a real build; assembled from the match record (P0 PostGame
// results + the P5 gameplay sim). Engine-agnostic + unit-testable. Reuses the P10
// fairness + coaching cores to derive match-quality + per-player tips.

using System;
using System.Collections.Generic;
using ProjectD.P10.Fairness;
using ProjectD.P10.Coaching;

namespace ProjectD.PostGame
{
    // Team enum already declared in PostGameModel.cs (same namespace) — removed duplicate here.

    public sealed class PlayerLine
    {
        public string playerId, displayName, championId, role;
        public Team team;
        public int kills, deaths, assists, csAt10, cs, gold, visionScore, championDamage, objectivesTaken;
        public int mmr;
        public float Kda => deaths == 0 ? (kills + assists) : (float)(kills + assists) / deaths;
        public float DamageShare;   // filled in during Build (0..1 of team damage)
    }

    // A sampled gold/xp lead over the match (blue - red).
    public struct LeadSample { public float t; public int goldDiff; public int xpDiff; }

    public sealed class MatchAnalytics
    {
        public string matchId;
        public Team winningTeam;
        public float durationSeconds;
        public List<PlayerLine> Blue = new(), Red = new();
        public List<LeadSample> Timeline = new();

        // Derived (computed in Build).
        public int BlueAvgMmr, RedAvgMmr, MmrSpread;
        public float PreMatchFairness;
        public bool Blowout;
        public string FairnessGrade;
        public int PeakBlueLead, PeakRedLead, FinalGoldDiff;
        public string MvpPlayerId;

        public IEnumerable<PlayerLine> AllPlayers()
        {
            foreach (var p in Blue) yield return p;
            foreach (var p in Red) yield return p;
        }

        /// <summary>Assemble the derived analytics from the raw lines + timeline.</summary>
        public void Build()
        {
            BlueAvgMmr = Avg(Blue); RedAvgMmr = Avg(Red);
            var all = new List<int>(); foreach (var p in AllPlayers()) all.Add(p.mmr);
            MmrSpread = MatchFairness.MmrSpread(all);
            PreMatchFairness = MatchFairness.PreMatchFairness(BlueAvgMmr, RedAvgMmr);

            // Damage share per team.
            FillDamageShare(Blue); FillDamageShare(Red);

            // Timeline-derived leads.
            PeakBlueLead = 0; PeakRedLead = 0;
            foreach (var s in Timeline)
            {
                if (s.goldDiff > PeakBlueLead) PeakBlueLead = s.goldDiff;
                if (-s.goldDiff > PeakRedLead) PeakRedLead = -s.goldDiff;
            }
            FinalGoldDiff = Timeline.Count > 0 ? Timeline[Timeline.Count - 1].goldDiff : 0;

            // Blowout + grade (use final gold + kill diff).
            int killDiff = TeamKills(Blue) - TeamKills(Red);
            Blowout = MatchFairness.WasBlowout(FinalGoldDiff, killDiff, durationSeconds);
            FairnessGrade = MatchFairness.Grade(PreMatchFairness, Blowout);

            // MVP = highest performance score across both teams.
            PlayerLine best = null; float bestScore = float.MinValue;
            foreach (var p in AllPlayers()) { float s = PerformanceScore(p); if (s > bestScore) { bestScore = s; best = p; } }
            MvpPlayerId = best?.playerId;
        }

        /// <summary>Coaching insights for a specific player (reuses the P10 coaching core).</summary>
        public List<Insight> CoachingFor(string playerId)
        {
            var p = Find(playerId);
            if (p == null) return new List<Insight>();
            var profile = new MatchProfile
            {
                minutes = durationSeconds / 60f, csAt10 = p.csAt10, deaths = p.deaths,
                visionScore = p.visionScore, kills = p.kills, assists = p.assists,
                objectivesTaken = p.objectivesTaken, role = p.role
            };
            return CoachingInsights.Analyze(profile);
        }

        public List<PlayerLine> Scoreboard(Team team)
        {
            var list = new List<PlayerLine>(team == Team.Blue ? Blue : Red);
            list.Sort((a, b) => PerformanceScore(b).CompareTo(PerformanceScore(a)));
            return list;
        }

        // ---- helpers ----
        public static float PerformanceScore(PlayerLine p)
            => p.kills * 3f + p.assists * 1.5f - p.deaths * 1.5f + p.championDamage / 250f + p.gold / 500f + p.visionScore * 0.8f + p.objectivesTaken * 2f;

        private PlayerLine Find(string id) { foreach (var p in AllPlayers()) if (p.playerId == id) return p; return null; }
        private static int Avg(List<PlayerLine> l) { if (l.Count == 0) return 0; long s = 0; foreach (var p in l) s += p.mmr; return (int)(s / l.Count); }
        private static int TeamKills(List<PlayerLine> l) { int k = 0; foreach (var p in l) k += p.kills; return k; }
        private static void FillDamageShare(List<PlayerLine> team)
        {
            long total = 0; foreach (var p in team) total += p.championDamage;
            foreach (var p in team) p.DamageShare = total > 0 ? p.championDamage / (float)total : 0f;
        }
    }
}
