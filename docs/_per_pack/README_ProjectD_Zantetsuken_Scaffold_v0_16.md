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
