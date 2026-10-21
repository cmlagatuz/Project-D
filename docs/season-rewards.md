# End-of-Season Rewards & Emblems

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Cosmetic rewards granted at season end based on the player's **highest rank reached**
that season (peak rank). Emblems, banners, honor, and bonus cosmetics only — no
combat power (monetization/integrity principle). Server-authoritative + unit-tested.

## What you earn
- **Tier emblem(s):** a season emblem + banner per tier. With `cumulativeUpToPeak`
  (default), you receive every tier's emblem **up to and including your peak tier**
  (a Crystal player gets all 7: Bronze -> Crystal). Set it false to grant only the
  peak tier's emblem.
- **Honor points:** escalating per tier (Bronze 100 ... Crystal 2200; ~6300 total for
  a full Crystal set). Progression currency, not power.
- **Bonus cosmetics:** higher tiers add icons, recall effects; Crystal adds an
  **animated frame + a title**.
- **Win milestones (independent of rank):** 25 / 50 / 100 ranked wins grant extra
  honor + cosmetics, so grinders are rewarded even off-peak.

## Eligibility
- A tier emblem requires a **minimum number of ranked games** (rises at apex:
  Bronze 5 ... Crystal 15) so a fluke placement doesn't hand out a Crystal emblem.
- Players who never completed placements get **no tier emblems** but still earn win
  milestones.

## Peak tracking
`RankState` records `peakStep` (highest ladder step reached this season) and
`rankedWins/rankedGames`. `RankService.ReportMatch` updates these every game, so the
reward is based on your best, not your final, rank.

## Online / account flow (server-authoritative)
```
Season closes on the server:
  var result = rewardService.FinalizeSeasonForPlayer(finishedState, config, seasonId);
  // -> stores a PendingSeasonReward on the account (PushPendingReward)

Next login (client):
  var pending = rewardService.GetPending();       // PullPendingReward
  seasonRewardsScreen.Present(rewardService, pending, result, seasonName);
  // player taps CLAIM -> rewardService.Claim(pending) grants cosmetics (idempotent)
```
Claim is **idempotent** (double-claims are no-ops), important for an online title.
The client never fabricates rewards — the server computes and stores them.

## Data
`season_rewards.seed.json` -> `SeasonRewardConfig` via **Tools > Project D > Import
Season Rewards**. 7 tier rewards + 3 win milestones, all original ids/names.
Reward names expose localization keys (`reward.emblem.*`, `reward.wins.*`).

## UI
- **`SeasonRewardsScreen`** — reveals earned rewards row-by-row with the peak rank +
  total honor and a Claim button.
- **`RewardRow`** — one earned reward (tier-colored swatch, name, honor).
Build the sandbox with **Tools > Project D > Build Ranked Demo**: climb, then tap
**NEW SEASON** to see the reward reveal before the soft reset.

## Tie-in with seasons
`RankedSeason.BeginNewSeason` soft-resets MMR after rewards are claimed, so the loop
is: play -> peak -> season ends -> rewards -> soft reset -> re-place.
