# P2 — Complete (Quick-chat/Ping + HUD polish + Matchmaking + Graphs + Tutorial)

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.
> This pack CLOSES the P2 backlog from docs/enhancement-review.md.

## #9 Quick-chat / Ping wheel  (Comms)
- `QuickChat` core: ping types, 6-slot default wheel, quick-chat phrases, per-sender
  rate limiter, team-wide vs ally audience, VO/SFX/text keys.
- `PingWheel` UI: radial 6-slot wheel + quick-chat strip; OnPing/OnQuickChat/OnSfxCue.
- Demo: Tools > Project D > Build Ping Wheel Demo.

## HUD polish  (HUD)
- `RecallBarWidget` — interruptible recall channel bar.
- `AbilityLevelUpBar` — skill points + per-slot rank pips + "+" prompts; ult gated by
  champion level (5/9/13).
- `StatusLegendPanel` — status-effect symbol legend with descriptions.
- Demo: Tools > Project D > Build HUD Widgets Demo.

## Matchmaking + queue  (Matchmaking)
- `MatchmakingModel` — mode picker, queue with widening MMR bracket, ready-check,
  pre-game hints (map mirrored / go to lane / objectives / ward).
- `MatchmakingView` — mode buttons, PLAY/queue timer + estimate, MATCH FOUND accept/decline.
- Demo: Tools > Project D > Build Matchmaking Demo (auto match-found ~4s).

## In-game graphs  (Graphs)
- `MatchGraphModel` — gold/XP-over-time samples (peak leads, final diff).
- `EconomyBreakdown` — categorized gold sources -> fractions.
- `MatchGraphView` — gold-diff line (blue up / red down) + stacked breakdown bar.
- Demo: Tools > Project D > Build Match Graphs Demo.

## Tutorial / onboarding  (Tutorial)
- `TutorialFlow` — ordered steps, triggers, per-step rewards, skip, feature gating
  (Ranked at account level 10 post-tutorial).
- `TutorialView` — step callout + progress dots + completion rewards.
- Demo: Tools > Project D > Build Tutorial Demo.

## Integration
- Ping VO/SFX keys -> effects pack; hint/status/phrase keys -> localization.
- Tutorial + matchmaking rewards -> account leveling (XP -> KP).
- Graphs render the PostGame match data; matchmaking feeds the pre-game/loading flow.

## Tests
`P2Tests` (14) covers ping rate-limit/audience/wheel, recall completion/interrupt,
ult gating, status legend, bracket widen + ready-check + hints, graph lead/diff +
breakdown, tutorial progression/skip, and unlock gating.
