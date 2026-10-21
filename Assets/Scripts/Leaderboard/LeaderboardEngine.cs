// Project D  |  Codename: Zantetsuken
// LeaderboardEngine.cs  (Leaderboard — filter, rank, paginate)
//
// The authoritative ranking logic for the APEX leaderboard (Adamantite + Crystal).
// Given a pool of entries and a query, it:
//   1) keeps only apex entries (Adamantite/Crystal),
//   2) applies scope (Server vs Friends) + tier/champion/lane/region filters,
//   3) orders by ladder standing: higher globalStep first, then LP, then MMR,
//      then recency, then win-rate (stable, deterministic tie-breaks),
//   4) assigns 1-based ranks and returns the requested page,
//   5) also finds the VIEWER's own rank within the filtered board.
//
// Pure C# (no UnityEngine) so the server runs it and the client mirrors it (GCD 14.3).

using System;
using System.Collections.Generic;
using ProjectD.Ranked;

namespace ProjectD.Leaderboard
{
    public static class LeaderboardEngine
    {
        /// <summary>Only Adamantite + Crystal appear on the apex board.</summary>
        public static bool IsApexEntry(LeaderboardEntry e)
            => e != null && (e.tier == RankTier.Adamantite || e.tier == RankTier.Crystal);

        public static LeaderboardPage Build(IReadOnlyList<LeaderboardEntry> pool, LeaderboardQuery q, string viewerId)
        {
            // ---- 1) filter ----
            var filtered = new List<LeaderboardEntry>(pool.Count);
            foreach (var e in pool)
            {
                if (!IsApexEntry(e)) continue;
                if (q.Scope == LeaderboardScope.Friends && !e.isFriend && e.playerId != viewerId) continue;
                if (q.Tier.HasValue && e.tier != q.Tier.Value) continue;
                if (!string.IsNullOrEmpty(q.ChampionId) && e.topChampionId != q.ChampionId) continue;
                if (q.Lane != Lane.Any && e.mainLane != q.Lane) continue;
                if (!string.IsNullOrEmpty(q.Region) && !string.Equals(e.region, q.Region, StringComparison.OrdinalIgnoreCase)) continue;
                filtered.Add(e);
            }

            // ---- 2) rank order (deterministic) ----
            filtered.Sort(CompareStanding);

            // ---- 3) assign ranks + find viewer ----
            int viewerRank = -1;
            RankedRow viewerRow = default;
            var allRows = new List<RankedRow>(filtered.Count);
            for (int i = 0; i < filtered.Count; i++)
            {
                var row = new RankedRow { Rank = i + 1, Entry = filtered[i] };
                allRows.Add(row);
                if (filtered[i].playerId == viewerId) { viewerRank = i + 1; viewerRow = row; }
            }

            // ---- 4) page ----
            int pageSize = q.PageSize <= 0 ? 100 : q.PageSize;
            int start = Math.Max(0, q.Page) * pageSize;
            var pageRows = new List<RankedRow>();
            for (int i = start; i < Math.Min(start + pageSize, allRows.Count); i++)
                pageRows.Add(allRows[i]);

            return new LeaderboardPage
            {
                Rows = pageRows,
                TotalMatching = allRows.Count,
                ViewerRank = viewerRank,
                ViewerRow = viewerRow
            };
        }

        /// <summary>Standing comparison: best player sorts first.</summary>
        public static int CompareStanding(LeaderboardEntry a, LeaderboardEntry b)
        {
            // Higher ladder step first (Crystal A above Adamantite E, etc.).
            if (a.globalStep != b.globalStep) return b.globalStep.CompareTo(a.globalStep);
            // Then more LP.
            if (a.lp != b.lp) return b.lp.CompareTo(a.lp);
            // Then higher hidden MMR (server tie-break).
            if (a.mmr != b.mmr) return b.mmr.CompareTo(a.mmr);
            // Then more recently active.
            if (a.lastActiveUnix != b.lastActiveUnix) return b.lastActiveUnix.CompareTo(a.lastActiveUnix);
            // Then higher win-rate.
            int wr = b.WinRate.CompareTo(a.WinRate);
            if (wr != 0) return wr;
            // Final stable tie-break by id so ordering is deterministic across calls.
            return string.CompareOrdinal(a.playerId ?? "", b.playerId ?? "");
        }

        /// <summary>Distinct champion ids present on the apex board (for the champion filter dropdown).</summary>
        public static List<string> ApexChampions(IReadOnlyList<LeaderboardEntry> pool)
        {
            var set = new HashSet<string>();
            foreach (var e in pool)
                if (IsApexEntry(e) && !string.IsNullOrEmpty(e.topChampionId)) set.Add(e.topChampionId);
            var list = new List<string>(set);
            list.Sort(StringComparer.Ordinal);
            return list;
        }
    }
}
