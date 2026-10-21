# Champion Select / Pick Phase — Design & Build

> Project D · Codename **Zantetsuken** · CONFIDENTIAL · GCD §5.4

## Build it
1. **Tools ▸ Project D ▸ Import Champion Data** (generates the 10 champion assets).
2. **Tools ▸ Project D ▸ Build Champion Select Scene** → `Assets/Scenes/ChampionSelect.unity`.
3. Press **Play**. Hover any card to see full details; press **LOCK IN** to confirm the step.

## Screen layout
- **Header:** title, turn banner (whose turn + BAN/PICK), and a per-step countdown.
- **Left / Right columns:** Blue and Red pick slots (3 each; scales to 5 for full mode).
- **Center:** scrollable 4-wide roster grid of `ChampionCard`s, color-keyed by role,
  dimmed when banned or already taken.
- **Right detail panel:** for the hovered champion — name, source world + original
  release date, roles, lore, playstyle, strengths, weaknesses, combos/tips, base
  stats, and the full Passive/Q/W/E/Ultimate kit with cooldown/cost.
- **Bottom:** bans strip + LOCK IN button.

## Logic (`ChampionSelectController`)
Pure, engine-agnostic state machine:
- Configurable draft sequence (default: 2 bans then alternating picks).
- Per-step timer with auto-lock on timeout.
- Duplicate prevention within a match (banned or picked → not selectable).
- Events (`OnStepStarted`, `OnHoverChanged`, `OnPickLocked`, `OnBanLocked`,
  `OnTimerTick`, `OnDraftComplete`) that the view and the **server** both consume,
  so the draft can be validated authoritatively.

Covered by `Tests/DraftControllerTests.cs` (duplicate prevention, slot locking, completion).

## Production TODO
- Portraits/splash art (pending licensed assets), role/lane filters, search, hover
  voice lines, ready-check, reconnect into an in-progress draft, and handoff of the
  locked roster to `MatchBootstrap` for scene load.
