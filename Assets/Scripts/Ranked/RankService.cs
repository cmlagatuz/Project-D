// Project D  |  Codename: Zantetsuken
// RankService.cs  (Ranked — the account-facing hub the game reads)
//
// Owns the local mirror of the player's RankState, applies match results through the
// authoritative progression, and syncs to the account backend (this is an online
// ranked game). The SERVER is the source of truth: in production the client sends a
// completed-match token and the server runs the exact same RankProgression, then
// pushes the authoritative RankState back. This class exposes that flow with pull/push
// hooks so it works offline (local) or online (server-authoritative).

using System;

namespace ProjectD.Ranked
{
    public sealed class RankService
    {
        public RankState State { get; private set; } = RankState.NewUnranked();
        public SeasonInfo Season { get; private set; } = new SeasonInfo();

        /// <summary>Fired after any ranked update so the UI (badge, rank-up screen) refreshes.</summary>
        public event Action<RankChange> OnRankChanged;

        // ---- account backend hooks (online ranked) ----
        public Func<RankState> PullAccountRank;      // load authoritative state on login
        public Action<RankState> PushAccountRank;    // persist state after a change

        public void Initialize()
        {
            var loaded = PullAccountRank?.Invoke();
            State = loaded ?? RankState.NewUnranked();
        }

        public void SetSeason(SeasonInfo season) => Season = season;

        /// <summary>
        /// Apply a finished ranked match. In placements it only calibrates MMR; once
        /// placed it moves LP/rank. lobbyAvgOpponentMmr is the enemy team's average MMR.
        /// </summary>
        public RankChange ReportMatch(MatchResult result, int lobbyAvgOpponentMmr)
        {
            RankChange change;
            if (!State.placementsComplete)
            {
                bool finished = PlacementService.RecordPlacement(State, result, lobbyAvgOpponentMmr);
                change = finished
                    ? new RankChange { Kind = RankChangeKind.Placed, Before = State.Step, After = State.Step, LpAfter = State.lp }
                    : new RankChange { Kind = RankChangeKind.None, Before = State.Step, After = State.Step, LpAfter = State.lp };
                // When placements just finished, State already reflects the seeded rank.
            }
            else
            {
                change = RankProgression.Apply(State, result, lobbyAvgOpponentMmr);
            }

            // Season stat tracking for end-of-season rewards.
            State.UpdatePeak();
            if (result != MatchResult.Remake)
            {
                State.rankedGames++;
                if (result == MatchResult.Win) State.rankedWins++;
            }

            PushAccountRank?.Invoke(State);
            OnRankChanged?.Invoke(change);
            return change;
        }

        /// <summary>Roll into a new season (soft MMR reset + fresh placements).</summary>
        public void StartNewSeason(SeasonInfo season)
        {
            State = RankedSeason.BeginNewSeason(State);
            Season = season;
            PushAccountRank?.Invoke(State);
            OnRankChanged?.Invoke(new RankChange { Kind = RankChangeKind.None, Before = State.Step, After = State.Step, LpAfter = State.lp });
        }

        /// <summary>Estimate matchmaking bracket from the player's MMR (for the queue).</summary>
        public (int minMmr, int maxMmr) MatchmakingBracket(int width = 120)
            => (Math.Max(0, State.mmr - width), State.mmr + width);
    }
}
