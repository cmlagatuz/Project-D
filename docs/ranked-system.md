# Ranked System — Tiers, Divisions & MMR

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

An online, server-authoritative ranked ladder with hidden MMR. All rank math is
engine-agnostic and unit-tested; the server runs the exact `RankProgression` and the
client mirrors the result.

## The ladder (low -> high)
| Tier | Divisions | LP per division |
|---|---|---|
| Bronze | D, C, B, A | 100 |
| Silver | D, C, B, A | 100 |
| Gold | D, C, B, A | 100 |
| Platinum | E, D, C, B, A | 100 |
| Mythril | E, D, C, B, A | 100 |
| Adamantite | E, D, C, B, A | **50 (apex)** |
| Crystal | E, D, C, B, A | **50 (apex)** |

Divisions climb **E -> A** (A is the top of a tier). Bronze-Gold skip E (D->A);
Platinum onward use E->A. Adamantite & Crystal are apex tiers that advance every
**50 LP** instead of 100. **32 steps** total from Bronze D to Crystal A.

## LP, promotion & demotion
- Win/loss changes LP within the current step. At **LP >= threshold** you PROMOTE
  (overflow LP carries to the next step; a big win can skip divisions). At **LP < 0**
  you DEMOTE into the previous step near its top.
- **Tier crossings** (e.g. Gold A -> Platinum E) are flagged for a bigger celebration.
- **Demotion protection:** a 3-game shield after every promotion, and you sit at 0 LP
  (not demoted) while the shield lasts. No demotion below Bronze D; no promotion past
  Crystal A (apex LP caps at the threshold).

## MMR (hidden matchmaking rating)
- Standard **Elo**: `expected = 1 / (1 + 10^((oppMMR - myMMR)/400))`.
- **K-factor** shrinks by tier (32 early -> 16 at Crystal) so apex ratings are stable;
  placements use a high K (60) to calibrate fast.
- MMR drives matchmaking brackets and shapes LP.

## LP shaped by MMR alignment
LP is a base value bent by how your MMR compares to your **displayed** rank's target:
- MMR **above** your rank -> gain more, lose less (climb quickly to true skill).
- MMR **below** your rank -> gain less, lose more (settle down).
- Aligned -> ~symmetric base LP (win ~22, loss ~18), clamped to sane bounds.

## Placements & seasons
- **Placements:** 7 games at high K calibrate MMR (rank hidden); then you're seeded at
  the step nearest your MMR (one step below, for immediate climb).
- **Seasons:** `RankedSeason.BeginNewSeason` soft-resets MMR toward the midpoint
  (~Gold/Plat) and requires placements again, so returning players re-place near true
  skill instead of grinding from Bronze.

## Online / account-authoritative
`RankService` mirrors the authoritative state and syncs to the account:
```csharp
service.PullAccountRank = () => account.LoadRank();   // on login
service.PushAccountRank = state => account.SaveRank(state); // after each match
var change = service.ReportMatch(MatchResult.Win, enemyTeamAvgMmr);
service.OnRankChanged += c => rankUpScreen.Present(c, service.State);
```
In production the client sends a signed completed-match token; the SERVER runs
`RankProgression.Apply` (the same code) and pushes the new `RankState` back — the
client never decides LP/rank itself.

## UI
- **`RankBadge`** — tier + division + LP with a tier-colored frame and progress bar.
- **`RankUpScreen`** — promotion/demotion/placed celebration; a bigger flourish for
  tier-ups. Build the sandbox with **Tools > Project D > Build Ranked Demo** (Win/Loss/
  Remake/New-Season buttons to climb Bronze D -> Crystal A live).

## Localization
Tier names expose keys `rank.tier.bronze` ... `rank.tier.crystal` (via
`RankFormatter.TierNameKey`) so they translate through the localization system;
division letters and LP are locale-neutral.
