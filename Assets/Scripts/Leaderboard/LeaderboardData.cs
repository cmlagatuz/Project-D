// Project D  |  Codename: Zantetsuken
// LeaderboardData.cs  (Leaderboard — apex standings data)
//
// The apex leaderboard covers ONLY the top tiers (Adamantite, Crystal). Each entry
// is a ranked player's standing plus the metadata needed to filter/sort the board:
// their most-played champion + lane, region, and friend flag. Engine-agnostic so the
// ranking/filtering runs on the server (source of truth) and the client mirrors it.

using System;
using System.Collections.Generic;
using ProjectD.Ranked;

namespace ProjectD.Leaderboard
{
    // Lanes used for filtering (matches the match-flow lane roles).
    public enum Lane { Any, Mid, Baron, Dragon, Support, Jungle }

    // Which population the board shows.
    public enum LeaderboardScope { Server, Friends }

    // A single ranked player's apex standing.
    [Serializable]
    public sealed class LeaderboardEntry
    {
        public string playerId;
        public string displayName;
        public string region;              // e.g. "NA", "EU", "SEA"

        public RankTier tier;              // Adamantite or Crystal only
        public Division division;          // E..A
        public int lp;                     // LP within the step (apex threshold 50)
        public int mmr;                    // hidden; used for tie-breaks / server sort

        public int globalStep;             // ladder step for exact ordering
        public int wins;
        public int losses;

        public string topChampionId;       // most-played champion this season
        public Lane mainLane = Lane.Any;   // most-played lane

        public bool isFriend;              // relative to the viewing player
        public long lastActiveUnix;        // recency (for tie-breaks / "active" badge)

        public int Games => wins + losses;
        public float WinRate => Games > 0 ? (float)wins / Games : 0f;

        public bool IsApex => tier == RankTier.Adamantite || tier == RankTier.Crystal;
    }

    // A page of ranked results with each entry's absolute rank number.
    public struct RankedRow
    {
        public int Rank;                   // 1-based position on the board
        public LeaderboardEntry Entry;
    }

    // Filter criteria applied before ranking.
    public struct LeaderboardQuery
    {
        public LeaderboardScope Scope;     // Server or Friends
        public RankTier? Tier;             // null = both apex tiers; else Adamantite/Crystal
        public string ChampionId;          // null/empty = any champion
        public Lane Lane;                  // Lane.Any = any lane
        public string Region;              // null/empty = all regions (server scope)
        public int Page;                   // 0-based
        public int PageSize;               // rows per page

        public static LeaderboardQuery Default(LeaderboardScope scope) => new LeaderboardQuery
        {
            Scope = scope, Tier = null, ChampionId = null, Lane = Lane.Any,
            Region = null, Page = 0, PageSize = 100
        };
    }

    public struct LeaderboardPage
    {
        public List<RankedRow> Rows;
        public int TotalMatching;          // total entries after filtering (for paging)
        public int ViewerRank;             // the viewer's own rank in this filtered board (-1 if not present)
        public RankedRow ViewerRow;        // the viewer's row (if present)
    }
}
