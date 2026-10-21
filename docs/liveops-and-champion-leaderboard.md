# Live-Ops Screen + Per-Champion "Server No.1" Leaderboard (P1 UI)

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

## Live-Ops screen (missions + weekly chest + events)
`LiveOpsScreen` + `MissionCard` (+ MissionsModel, WeeklyChestModel cores):
- **Cadence tabs** (Daily / Weekly / Event) filter the mission grid.
- **Mission cards** — title, description, progress bar (progress/target), rewards
  (XP / KP / chest fragments), and a **CLAIM** button that lights up when complete;
  claiming grants XP/KP to the account and fragments to the chest (idempotent).
- **Weekly chest widget** — fragment progress toward the next chest, assembled chest +
  key counts, and an **OPEN CHEST** action (needs a chest + a key; weekly cap enforced).
- Demo: Tools > Project D > Build Live-Ops Demo.

## Per-Champion "Server No.1" leaderboard (batch-4)
`ChampionLeaderboardScreen` + `ChampionLeaderRow` (+ ChampionLeaderboard core):
- **SERVER No.1 grid** (default) — the single best-rated player per champion, gold
  crown on each #1, sorted by rating; region filter narrows the pool.
- **Champion focus** — click a champion to see its full **top-N ladder** (rank #,
  player, rank label, win-rate, rating); BACK returns to the grid.
- Distinct from the apex ladder — answers "who is the #1 <champion>?" for every champion.
- Demo: Tools > Project D > Build Champion Leaderboard Demo.

## Integration
- Mission rewards -> account leveling (XP -> levels -> KP) and the weekly chest.
- Mission unclaimed count + new events feed the main-menu notification badges.
- Champion ratings come from the server's per-champion snapshot; region list shared
  with the apex leaderboard's region filter.
