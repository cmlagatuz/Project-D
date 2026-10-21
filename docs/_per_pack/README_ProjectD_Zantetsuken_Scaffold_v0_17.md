# Project D — Ranked System (Tiers + MMR)

> Codename **Zantetsuken** · CONFIDENTIAL

Server-authoritative ranked ladder with hidden MMR for the online ranked mode.

## Ladder (low -> high)
Bronze, Silver, Gold (**D->A**) · Platinum, Mythril (**E->A**) · Adamantite, Crystal
(**E->A, 50 LP/division apex**). Divisions climb E->A; **32 steps** Bronze D -> Crystal A.

## Contents
- `Scripts/Ranked/` — `RankLadder`, `RankState`, `MmrModel` (Elo), `LpCalculator`,
  `RankProgression` (promo/demote + shield), `PlacementService`, `RankedSeason`,
  `RankService` (account sync), `RankFormatter`.
- `Scripts/UI/` — `RankBadge`, `RankUpScreen`, `RankedDemoDriver`.
- `Scripts/Editor/RankedDemoBuilder.cs` — **Tools > Project D > Build Ranked Demo**.
- `Scripts/Tests/RankedTests.cs` — ladder shape, promotion across tiers, demotion +
  shield, LP shaping by MMR, placements, formatter.
- `docs/ranked-system.md`.

## Highlights
- LP promotion carries overflow (skip divisions on big wins); tier crossings flagged.
- Demotion protection (3-game shield after promotion; hold at 0 LP); floor/ceiling clamps.
- MMR-aligned LP: underranked players gain more / lose less to climb to true skill.
- 7-game placements + seasonal soft-reset. Tier names localizable (rank.tier.*).

## Quick start
Build Ranked Demo -> Play. Tap WIN to climb Bronze D -> Crystal A; promotions/demotions
trigger the rank-up screen (tier-ups get a bigger flourish).


## End-of-Season Rewards (this update)
- **Tier emblems** by peak rank: cumulative up to the peak tier (Crystal player gets all 7), or peak-only.
- **Honor** escalating per tier (Bronze 100 ... Crystal 2200) + **bonus cosmetics** (Crystal adds an animated frame + title).
- **Win milestones** (25/50/100 ranked wins) independent of rank; **min-games gating** so fluke placements don't earn apex emblems.
- **Peak tracking** in `RankState` (best rank of the season, not final); server computes + stores a `PendingSeasonReward`, client claims it **idempotently**.
- New: `SeasonRewardData` (config SO), `SeasonRewardCalculator`, `SeasonRewardService`, `SeasonRewardsScreen`, `RewardRow`, importer, tests. Data: `season_rewards.seed.json`. Docs: `docs/season-rewards.md`.


## Apex Leaderboard (this update)
- Ranks the **top two tiers only** (Adamantite, Crystal); other tiers never appear.
- **Scopes:** Server (whole population, region-optional) and Friends (viewer + friends).
- **Filters:** tier (Both/Adamantite/Crystal), champion, and lane — combinable.
- Deterministic order: ladder step -> LP -> MMR -> recency -> win-rate; a pinned "your rank" card shows the viewer's spot even off-page.
- Server-authoritative: the same `LeaderboardEngine` runs on client + server; `EntryFromRank` builds apex-only snapshots.
- New: `LeaderboardData`, `LeaderboardEngine`, `LeaderboardService`, `MockApexData`, `LeaderboardRow`, `LeaderboardScreen`, demo builder, tests. Docs: `docs/apex-leaderboard.md`. Build: **Tools > Project D > Build Leaderboard Demo**.
