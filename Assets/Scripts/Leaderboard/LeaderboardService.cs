// Project D  |  Codename: Zantetsuken
// LeaderboardService.cs  (Leaderboard — data source + query hub)
//
// The client-facing hub. It requests apex standings from the backend (server scope)
// or from the friends graph (friends scope), builds a page via LeaderboardEngine,
// and exposes the champion/lane filter options. For the online ranked game the
// SERVER computes/stores the ranked snapshot; this service pulls a page and mirrors
// it. Offline/demo mode uses an injected in-memory pool.

using System;
using System.Collections.Generic;
using ProjectD.Ranked;

namespace ProjectD.Leaderboard
{
    public sealed class LeaderboardService
    {
        public string ViewerId { get; set; } = "";

        // Backend hooks (online). Return the full apex pool for a scope; the engine
        // filters/paginates locally so filter changes don't need a round-trip.
        public Func<LeaderboardScope, IReadOnlyList<LeaderboardEntry>> FetchPool;

        // Offline/demo fallback pool.
        private IReadOnlyList<LeaderboardEntry> _localPool = new List<LeaderboardEntry>();
        public void SetLocalPool(IReadOnlyList<LeaderboardEntry> pool) => _localPool = pool ?? new List<LeaderboardEntry>();

        /// <summary>Get a filtered, ranked, paginated board for a query.</summary>
        public LeaderboardPage Query(LeaderboardQuery q)
        {
            var pool = FetchPool != null ? FetchPool(q.Scope) : _localPool;
            return LeaderboardEngine.Build(pool, q, ViewerId);
        }

        /// <summary>Champion ids available to filter by, for the current scope.</summary>
        public List<string> ChampionFilterOptions(LeaderboardScope scope)
        {
            var pool = FetchPool != null ? FetchPool(scope) : _localPool;
            return LeaderboardEngine.ApexChampions(pool);
        }

        /// <summary>All lanes (the dropdown is static; Any first).</summary>
        public static Lane[] LaneFilterOptions() => new[]
        { Lane.Any, Lane.Mid, Lane.Baron, Lane.Dragon, Lane.Support, Lane.Jungle };

        /// <summary>
        /// Build a LeaderboardEntry for a player from their RankState + season stats.
        /// The server does this when snapshotting the ladder; exposed here for tools/tests.
        /// Returns null if the player is not apex (won't appear on this board).
        /// </summary>
        public static LeaderboardEntry EntryFromRank(
            string playerId, string displayName, string region,
            RankState state, string topChampionId, Lane mainLane, bool isFriend, long lastActiveUnix)
        {
            if (state == null) return null;
            var tier = state.Tier;
            if (tier != RankTier.Adamantite && tier != RankTier.Crystal) return null;

            return new LeaderboardEntry
            {
                playerId = playerId,
                displayName = displayName,
                region = region,
                tier = tier,
                division = state.Division,
                lp = state.lp,
                mmr = state.mmr,
                globalStep = state.globalStep,
                wins = state.rankedWins,
                losses = Math.Max(0, state.rankedGames - state.rankedWins),
                topChampionId = topChampionId,
                mainLane = mainLane,
                isFriend = isFriend,
                lastActiveUnix = lastActiveUnix
            };
        }
    }
}
