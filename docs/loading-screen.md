# In-Between Loading Screen — Tips & Trivia

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

The transition shown between **Preparation** and **In-Game** while the match streams
in. It keeps players engaged with the picked-champion focus, a progress bar, and a
rotating tips/trivia carousel.

## Build it
1. **Tools ▸ Project D ▸ Import Loading Tips** → generates `ChampionTipSet` assets
   (10 champions) + a `GeneralTipSet`.
2. **Tools ▸ Project D ▸ Build Loading Screen** → `Assets/Scenes/LoadingScreen.unity`.
3. Press **Play** — the bar fills, the status reads *Loading… → Waiting for players… →
   Entering the arena…*, and tips/trivia rotate every few seconds.

## What's on screen
- **Champion focus** (bottom-left) — your picked champion: name, world · role, and a
  splash area (role-tinted until art is assigned).
- **Progress bar** (bottom) — weighted total of all load sources with a live percent
  and status line.
- **Tip card** (bottom-right) — a fading carousel of TIP / DID YOU KNOW? / GAMEPLAY
  TIP cards.

## Content (all original copy)
- **Per champion:** 3 "playing" tips, 2 "playing against" tips, 2 trivia lines.
- **General:** 12 gameplay tips (objectives, shop, boots slot, undo/sell, telegraphs…).

## Rotation logic (TipRotationController)
- Prioritizes the LOCAL champion (alternating playing tips + trivia up front).
- Adds one "playing against" tip per enemy pick.
- Interleaves general tips so it never feels repetitive.
- **Deterministic** for a given match seed (server can mirror the order) and
  guarantees **no two identical cards back-to-back**.

## Progress (LoadingProgress)
Register each real load source as a weighted task and `Report(id, 0..1)`:
```csharp
view.Report("scene", sceneOp.progress);
view.Report("assets", addressablesHandle.PercentComplete);
view.Report("network", handshake01);
view.SetAllPlayersReady(serverSaysEveryoneReady);   // gates the final transition
view.OnReadyToEnterMatch += () => LoadMatchScene();
```
In the prototype, `simulateProgress` fills the bar automatically so the scene runs
standalone. Turn it off and drive `Report(...)` from your real loaders.

## Where it fits (MatchFlow)
Preparation → **Loading Screen** → In-Game. Call `view.Configure(localChampId,
enemyChampionIds, matchSeed)` when the draft locks, then transition to the match
scene on `OnReadyToEnterMatch`.
