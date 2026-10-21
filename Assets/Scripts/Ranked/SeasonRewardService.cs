// Project D  |  Codename: Zantetsuken
// SeasonRewardService.cs  (Ranked — grant + claim end-of-season rewards)
//
// At season end the server snapshots each player's finished RankState, runs the
// SeasonRewardCalculator, and stores a pending reward grant on the account. When the
// player next logs in, the client pulls the pending grant, shows the SeasonRewards
// screen, and claims it (idempotent, so double-claims are safe). Cosmetic-only.

using System;

namespace ProjectD.Ranked
{
    [Serializable]
    public sealed class PendingSeasonReward
    {
        public int seasonId;
        public string peakRankLabel;
        public int totalHonor;
        public bool claimed;
        // Flattened reward ids for persistence (the screen re-resolves display data).
        public string[] emblemIds = new string[0];
        public string[] bannerIds = new string[0];
        public string[] bonusCosmeticIds = new string[0];
    }

    public sealed class SeasonRewardService
    {
        /// <summary>Persist a computed grant to the account (server side).</summary>
        public Action<PendingSeasonReward> PushPendingReward;
        /// <summary>Load a pending (unclaimed) grant on login (client side).</summary>
        public Func<PendingSeasonReward> PullPendingReward;
        /// <summary>Grant the cosmetics/honor to the account (idempotent).</summary>
        public Action<PendingSeasonReward> GrantToAccount;

        /// <summary>
        /// Compute the season rewards for a finished state and store them as pending.
        /// Called by the server when a season closes.
        /// </summary>
        public SeasonRewardResult FinalizeSeasonForPlayer(RankState finishedSeason, SeasonRewardConfig config, int seasonId)
        {
            var adapter = new SeasonRewardConfigAdapter(config);
            var result = SeasonRewardCalculator.Evaluate(finishedSeason, adapter);

            var pending = new PendingSeasonReward
            {
                seasonId = seasonId,
                peakRankLabel = result.PeakRankLabel,
                totalHonor = result.TotalHonor,
                claimed = false
            };
            pending.emblemIds = ExtractEmblems(result);
            pending.bannerIds = ExtractBanners(result);
            pending.bonusCosmeticIds = ExtractBonus(result);

            PushPendingReward?.Invoke(pending);
            return result;
        }

        /// <summary>Client: is there an unclaimed grant to show?</summary>
        public PendingSeasonReward GetPending() => PullPendingReward?.Invoke();

        /// <summary>Claim the pending grant (idempotent). Returns true if newly claimed.</summary>
        public bool Claim(PendingSeasonReward pending)
        {
            if (pending == null || pending.claimed) return false;
            GrantToAccount?.Invoke(pending);
            pending.claimed = true;
            PushPendingReward?.Invoke(pending);  // persist the claimed flag
            return true;
        }

        // ---- helpers ---- //
        private static string[] ExtractEmblems(SeasonRewardResult r)
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var rw in r.Rewards) if (!string.IsNullOrEmpty(rw.EmblemId)) list.Add(rw.EmblemId);
            return list.ToArray();
        }
        private static string[] ExtractBanners(SeasonRewardResult r)
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var rw in r.Rewards) if (!string.IsNullOrEmpty(rw.BannerId)) list.Add(rw.BannerId);
            return list.ToArray();
        }
        private static string[] ExtractBonus(SeasonRewardResult r)
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var rw in r.Rewards) if (rw.BonusCosmeticIds != null) list.AddRange(rw.BonusCosmeticIds);
            return list.ToArray();
        }
    }
}
