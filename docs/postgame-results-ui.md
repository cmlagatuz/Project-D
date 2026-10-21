# PostGame Results Screen UI

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

Wires the PostGame core (PostGameModel/Service) to a runnable results screen.

## Screen
- **Outcome banner** — VICTORY / DEFEAT (colored accent) + match duration.
- **Two scoreboards** — Blue & Red, each sorted by performance score, with column
  headers (Player · K/D/A · CS · DMG · Gold). The local player's row is accented.
- **MVP** — the top-performance player is crowned + named. A role-agnostic score means
  supports/tanks can MVP via utility, not just kills.
- **MVP vote** — each row has a VOTE MVP button; the local player votes once (can't vote
  self), counts show as stars, and voting locks after casting. `MvpVoteTally` dedupes.
- **Rewards** — the local player's match XP (+ first-win KP bonus) with an XP bar
  flourish; Continue hands the XP to account leveling (KP Store pack).

## Components
- `ScoreboardRow` — one player row (bind stats + self/MVP/vote flags).
- `PostGameResultsView` — builds both scoreboards, crowns MVP, tallies votes, reveals
  rewards; events OnLocalVote (send to server) + OnContinue (hand off XP).
- `PostGameDemoDriver` — feeds a sample 5v5 (Blue wins) result.

## Server-authoritative
The SERVER computes the PostGameResult and the final community MVP; the client renders
it and sends the local vote back. `view.ApplyCommunityMvp(id)` highlights the voted MVP.

## Integration
`OnContinue` -> `KpStoreService.AddXp(reward.accountXp)` (level-ups grant KP) and, if
`bonusKp>0`, `GrantKp`. This closes the match -> meta progression loop.

## Demo
Tools > Project D > Build PostGame Results Demo -> Play: VICTORY, scoreboards with the
MVP crown, tap VOTE MVP, view rewards, Continue.
