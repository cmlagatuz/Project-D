# Rank Decay — Inactivity at High Tiers

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Apex ladders keep the top honest by decaying LP when a player goes inactive. Drops
into the existing Ranked pack (uses `RankTier` / `RankLadder`) and needs NO change to
the sealed `RankState` — it persists a tiny `RankDecayState` alongside the account.

## Rules
- Only **Platinum and above** decay (below that, inactivity does nothing).
- Each player banks **protection days**; inactivity spends the bank first, then decays
  **one tick per idle day**. Playing a ranked game tops the bank back up (+1/day, cap 28).
- LP lost per tick: Platinum 35, Mythril 50, Adamantite/Crystal 75 (apex divisions are
  50 LP, so an apex tick can cross a division).
- Decay **never drops below the Platinum floor** — inactivity can't knock you out of
  the apex ladder entirely.

## Integration
```csharp
var decay = new RankDecayState();
RankDecay.GrantInitialBank(decay);                 // on first reaching Platinum
RankDecay.OnRankedGamePlayed(decay, nowUnix);      // after each ranked game
// daily server tick / login:
var r = RankDecay.Compute(state.globalStep, state.lp, state.placementsComplete, decay, nowUnix);
state.globalStep = r.NewGlobalStep; state.lp = r.NewLp;
```
`DaysUntilDecay(decay, nowUnix)` powers a "decay in N days" profile warning. Because
decay lowers LP/step, decaying players show as **falling** on the apex leaderboard's
live movement indicator on the next snapshot.

## Tests
`RankDecayTests` (10) — low tiers don't decay, banked days absorb inactivity, per-day
decay past the bank, step demotion, Platinum-floor clamp, unplaced players, bank
top-up + cap, and the days-until-decay countdown.
