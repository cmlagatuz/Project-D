# Project D — Enhancement Review (vs. 62 UX references)

> Codename **Zantetsuken** · CONFIDENTIAL
> The reference screenshots are Riot's Wild Rift, used ONLY as UX benchmarks to
> compare against our own original systems. No art, text, or assets are reproduced;
> all builds are original Project D design. FF-flavored names remain placeholders.

Legend: ✅ built · 🟡 partial (enhance) · 🔴 missing.

## A. Meta / out-of-game (Store / KP)
- 🔴 Home / main-menu shell (modes, Store, profile, events entry points)
- 🟡 Store (KP) — terminology finalized; **no KP Store UI** (champions, borders, icons)
- 🔴 Profile customization (icon/border showcase) + Friends list
- 🔴 Account/profile leveling (the KP source) + XP breakdown/graph
- 🔴 Live-ops: featured, events, missions/quests, weekly chest, activities
- 🟡 Champion detail/preview page (stat bars, 3D/2D toggle, skins carousel, buy panel)
- 🟡 Leaderboard: apex ✅; per-champion "Server #1" board + full-ladder browse 🔴
- 🟡 Loadouts: build editor ✅; rune/keystone loadouts 🔴

## B. Modes & matchmaking
- 🟡 Mode picker, co-op vs AI, matchmaking/queue UI, "map mirrored / go to lane" hints

## C. In-match economy / Shop (Gil)
- ✅ Shop + 86 items + sell/undo + recommendations
- 🔴 End-of-game gold/XP-over-time graphs; gold/xp breakdown popups

## D. In-match HUD, controls, combat feedback
- ✅ Touch controls + ability animator
- 🔴 Recall cast bar; ability level-up/info tooltip; target/last-hit indicators;
     turret/lane status UI; status-effect (symbol) legend
- 🔴 Announcer + kill/objective banners (Double Kill, First Turret, objective slain,
     Nexus/Crystal-Core exposed) — VO/SFX keys exist, no announcement system/UI
- 🔴 Quick-chat / ping wheel comms

## E. Settings & onboarding
- ✅ Graphics (frame-rate gating), Sound, Accessibility (VO subtitles), Controls, HUD layout editor
- 🟡 "General" settings tab (notifications, account, language shortcut)
- 🔴 Tutorial / onboarding flow (tutorials, tutorial-loading)
- ✅ Loading screen (tips + matchup callout + voice)

## F. Post-game
- 🔴 Victory/Defeat → scoreboard → per-player stats → MVP vote → rewards/leveling handoff

---

## Prioritized backlog

### P0 — close the biggest loops
1. **KP Store UI + Account Leveling** (profile XP → KP → champions/borders/icons)
2. **Post-game flow** (Victory/Defeat, scoreboard, stats, MVP vote, rewards handoff)
3. **Announcer & event banners** (wire existing VO/SFX keys to on-screen banners)
4. **Champion detail/preview page** (stat bars, role tags, 3D/2D, skins carousel, KP buy)

### P1 — engagement + social + comms
5. Home / main-menu shell
6. Profile + Friends (icon/border showcase; ties into leaderboard Friends scope)
7. Live-ops (events, missions/quests, weekly chest)
8. Per-champion "Server #1" leaderboard + full-ladder browse
9. Quick-chat / ping wheel

### P2 — HUD polish + onboarding
10. Recall cast bar, ability level-up/info tooltip, target/turret/lane status, status legend
11. Mode picker + matchmaking/queue UI + pre-game hints
12. In-game gold/XP graphs + breakdown popups
13. Tutorial / onboarding flow

## Notes on batch-4 specifics
- **Per-champion leaderboard** = a distinct board from the apex ladder: best-rated
  player per champion on the server, with champion/role tabs. Extends our
  `LeaderboardEngine` with a "top entry grouped by championId" query.
- **Champion detail page** = the pre-unlock/preview surface: normalized stat bars
  (Damage/Toughness/Utility/Difficulty from `ChampionDefinition`), ability row,
  role tags, model preview toggle, skins carousel, and a KP unlock/buy panel — the
  natural home for the new **Store/KP** champion purchases.
