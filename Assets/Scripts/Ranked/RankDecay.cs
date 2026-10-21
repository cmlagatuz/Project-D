// Project D  |  Codename: Zantetsuken
// RankDecay.cs  (Ranked — LP decay for inactivity at high tiers)
//
// High-tier ladders keep the top honest by decaying LP when a player goes inactive.
// Only APEX-ish tiers decay (Platinum and above by default). A player banks a grace
// of "decay-protection days" which they spend before decay starts; playing a ranked
// game tops the bank back up. Decay never demotes below the lowest decaying tier's
// floor, so inactivity can't knock a player out of the apex ladder entirely.
//
// DESIGN NOTE: this is a self-contained CALCULATOR that works on primitives + the
// existing RankLadder/RankTier. It does NOT require modifying the sealed RankState:
// persist the small RankDecayState (banked days + last-game time) alongside the
// account and feed the current step/lp in. Server-authoritative + unit-testable.

using System;

namespace ProjectD.Ranked
{
    // Persisted alongside RankState (2 small fields). The account stores this so the
    // server can compute decay on a daily tick or at login without touching RankState.
    [Serializable]
    public sealed class RankDecayState
    {
        public int bankedDays;          // protection days remaining before decay begins
        public long lastRankedGameUnix; // server time of the player's last ranked game
    }

    public static class RankDecay
    {
        /// <summary>Lowest tier that decays. Below this, inactivity does nothing.</summary>
        public const RankTier LowestDecayingTier = RankTier.Platinum;

        public const int MaxBankedDaysDefault = 28;   // cap on stored protection
        public const int BankedDaysPerGame = 1;       // each ranked game adds a day (up to cap)
        public const int InitialBankedDays = 14;      // fresh apex players start with a buffer

        /// <summary>LP removed per daily decay tick, by tier (apex loses more).</summary>
        public static int LpPerDecayTick(RankTier tier) => tier switch
        {
            RankTier.Platinum => 35,
            RankTier.Mythril => 50,
            RankTier.Adamantite => 75,   // apex divisions are 50 LP, so this is >1 division
            RankTier.Crystal => 75,
            _ => 0
        };

        public static bool TierDecays(RankTier tier) => (int)tier >= (int)LowestDecayingTier;

        public struct DecayResult
        {
            public int NewGlobalStep;
            public int NewLp;
            public int NewBankedDays;
            public int TicksApplied;      // daily decay steps applied
            public int LpLost;            // total LP removed
            public int StepsDropped;      // ladder steps demoted by decay
            public bool Decayed => TicksApplied > 0;
        }

        /// <summary>
        /// Compute decay from the last ranked game to 'now'. Spends banked protection
        /// days first; each idle day beyond the bank applies one decay tick. Does NOT
        /// mutate inputs — returns the new step/lp/banked-days for the caller to store.
        /// </summary>
        public static DecayResult Compute(
            int globalStep, int lp, int bankedDays, bool placementsComplete,
            long nowUnix, long lastRankedGameUnix, int maxBankedDays = MaxBankedDaysDefault)
        {
            var result = new DecayResult { NewGlobalStep = globalStep, NewLp = lp, NewBankedDays = bankedDays };
            if (!placementsComplete) return result;

            var tier = RankLadder.StepAt(globalStep).Tier;
            if (!TierDecays(tier)) return result;

            long secondsIdle = Math.Max(0, nowUnix - lastRankedGameUnix);
            int daysIdle = (int)(secondsIdle / 86400L);
            if (daysIdle <= 0) return result;

            int spent = Math.Min(bankedDays, daysIdle);
            int bank = bankedDays - spent;
            int decayDays = daysIdle - spent;

            int beforeStep = globalStep;
            int lpLost = 0;

            for (int i = 0; i < decayDays; i++)
            {
                var curTier = RankLadder.StepAt(globalStep).Tier;
                if (!TierDecays(curTier)) break;   // dropped to the decaying floor

                lp -= LpPerDecayTick(curTier);
                lpLost += LpPerDecayTick(curTier);
                result.TicksApplied++;

                // Demote steps while LP is negative, but never below the decaying floor.
                while (lp < 0)
                {
                    if (AtDecayFloor(globalStep)) { lp = 0; break; }
                    globalStep--;
                    int prevThreshold = RankLadder.StepAt(globalStep).LpThreshold;
                    lp += prevThreshold;
                    lp = Math.Max(0, Math.Min(lp, prevThreshold - 1));
                }
            }

            result.NewGlobalStep = globalStep;
            result.NewLp = lp;
            result.NewBankedDays = Math.Max(0, Math.Min(bank, maxBankedDays));
            result.LpLost = lpLost;
            result.StepsDropped = beforeStep - globalStep;
            return result;
        }

        /// <summary>Convenience overload using RankDecayState.</summary>
        public static DecayResult Compute(int globalStep, int lp, bool placementsComplete,
                                          RankDecayState decay, long nowUnix, int maxBankedDays = MaxBankedDaysDefault)
        {
            var r = Compute(globalStep, lp, decay.bankedDays, placementsComplete, nowUnix, decay.lastRankedGameUnix, maxBankedDays);
            decay.bankedDays = r.NewBankedDays; // reflect spent protection
            return r;
        }

        /// <summary>Call when a ranked game finishes: top up protection + stamp the time.</summary>
        public static void OnRankedGamePlayed(RankDecayState decay, long nowUnix, int maxBankedDays = MaxBankedDaysDefault)
        {
            if (decay == null) return;
            decay.bankedDays = Math.Min(maxBankedDays, decay.bankedDays + BankedDaysPerGame);
            decay.lastRankedGameUnix = nowUnix;
        }

        /// <summary>Grant a fresh apex player the starting buffer (e.g. on promotion to Platinum).</summary>
        public static void GrantInitialBank(RankDecayState decay)
        {
            if (decay == null) return;
            if (decay.bankedDays < InitialBankedDays) decay.bankedDays = InitialBankedDays;
        }

        /// <summary>Days of inactivity before decay actually starts biting.</summary>
        public static int DaysUntilDecay(RankDecayState decay, long nowUnix)
        {
            long idle = Math.Max(0, nowUnix - decay.lastRankedGameUnix);
            int daysIdle = (int)(idle / 86400L);
            int remaining = decay.bankedDays - daysIdle;
            return remaining > 0 ? remaining : 0;
        }

        private static bool AtDecayFloor(int globalStep)
        {
            for (int i = 0; i < RankLadder.StepCount; i++)
                if (RankLadder.StepAt(i).Tier == LowestDecayingTier)
                    return globalStep <= i;
            return false;
        }
    }
}
