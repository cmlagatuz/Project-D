# Project D — Enhancement Suite (P0 #2-4 + all P1 + all P2)

> Codename Zantetsuken · CONFIDENTIAL · All original design.
> Built from docs/enhancement-review.md. Screens referenced are UX benchmarks only.

Engine-agnostic, server-authoritative, unit-tested CORE logic for every remaining
backlog item (13 systems). UI/scene builders can bind to these directly.

## P0
- **PostGame** (`PostGameModel`, `PostGameService`) — Victory/Defeat, scoreboard sort,
  MVP + losing-team MVP from a role-agnostic performance score, per-player match-XP
  rewards (feeds account leveling/KP), first-win-of-day bonus, and an MvpVoteTally.
- **Announcer** (`Announcer`) — kill-streak + multi-kill tracking, a priority queue
  with per-kind cooldown, and builders for First Blood/Turret/Objective/Ace/Core-Exposed
  that carry VO + SFX + banner-text keys (wires the effects + localization packs).
- **ChampionDetail** (`ChampionDetailModel`) — normalized stat bars (Damage/Toughness/
  Utility/Difficulty), skins list, and KP unlock state (Owned/Purchasable/LevelLocked/
  NotEnoughKp) for the batch-4 champion preview page.

## P1
- **MainMenu** (`MainMenuModel`) — home shell destinations + notification badges (missions,
  friend requests, new store items, pending rewards, events) with a total badge count.
- **Profile** (`ProfileModel`) — the showcase card (equipped KP icon/border, rank showcase,
  season stats, favorite champion) shared to friends + scoreboard.
- **Friends** (`FriendsModel`) — friends + presence, request send/receive/accept
  (mutual auto-accept), sorted online-first; feeds the leaderboard Friends scope.
- **LiveOps** (`MissionsModel`, `WeeklyChestModel`) — daily/weekly/event missions with
  progress + idempotent claim; fragments -> chest -> key with a weekly cap.
- **ChampionLeaderboard** (`ChampionLeaderboard`) — per-champion "Server No.1" board +
  top-N per champion (the batch-4 CHAMPION board), distinct from the apex ladder.
- **Comms** (`QuickChat`) — ping wheel (6-slot) + quick-chat phrases, per-sender rate
  limiter, team-wide vs ally audience, VO/SFX/text keys.

## P2
- **HUD** (`HudSystems`) — RecallChannel cast bar (interruptible), AbilityLevelUp
  (skill points + rank caps + ult gates), StatusEffectLegend (symbol descriptions),
  TargetingModel (+ last-hit window helper).
- **Matchmaking** (`MatchmakingModel`) — mode picker, queue with a widening MMR bracket,
  match-found ready-check, and pre-game hints (map mirrored / go to lane / objectives).
- **Graphs** (`MatchGraphModel`, `EconomyBreakdown`) — gold/XP-over-time samples (peak
  lead, final diff) + categorized gold/xp breakdown fractions.
- **Tutorial** (`TutorialFlow`, `FeatureUnlocks`) — step-based onboarding with triggers,
  rewards, skip, and feature gating (Ranked at account level 10 post-tutorial).

## Integration notes
- PostGame rewards -> `AccountProgressionService.AddXp` (KP Store pack) -> level-ups -> KP.
- Announcer/Comms keys -> the effects (`EffectCatalog`) + localization packs.
- ChampionDetail unlock -> the KP Store `Buy` flow. Friends -> leaderboard Friends scope.
- All models are UnityEngine-free; the SERVER runs them and clients mirror (GCD 14.3).

## Tests
`SuiteTests` (23) covers every system: MVP + vote, multi-kill + queue priority, stat
bars + unlock, menu badges, friend flows, missions claim, chest cap, champion No.1,
ping rate-limit + audience, recall + ult gating, bracket widen + ready-check, graph
lead/diff + breakdown, tutorial progression + unlock gating.
