# P9 — Systems Reference (Narrative, Progression & Player Expression)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic + tested (P9Tests, 15).
1.  Narrative/QuestChain — ordered story beats with prerequisites; available/complete/chain-complete.
2.  Codex/ChampionCodex — lore entries gated by free/mastery/achievement/quest; read state.
3.  Cinematics/CinematicTimeline — keyframed sequencer; fires events in order; skip; progress.
4.  VoiceLines/VoiceDirector — contextual VO with matchup bonus + priority + cooldown.
5.  WorldEvents/WorldEvent — server-wide progress goal + milestone claim + active window.
6.  Achievements/AchievementSystem — tiered goals; current tier; claim newly-completed tiers.
7.  Titles/TitleRegistry — earnable titles, ownership, one active + active text.
8.  Emporium/Emporium — discounts, bundle pricing, deterministic daily rotation.
9.  DailyLogin/DailyLogin — streak continue/reset, longest streak, weekly reward bumps.
10. ProgressPaths/ProgressionPaths — multi-track tiers, claim, tier progress bar.

Plus: P8 Content UI (Item build tree, Rune page editor, Coaching report) — Tools > Project D > Build Item Build Tree Demo.

## Tests
P9Tests (15) covers every system.
