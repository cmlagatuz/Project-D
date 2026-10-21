# P6 — Systems Reference (AI, Content & Balance Tooling)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic + tested (P6Tests, 14).
1. BotAI/BotBrain — utility scoring picks the best action.
2. Pathfinding/NavGrid — A* around walls; null when blocked.
3. Abilities/AbilityScript — data-driven effects + AD/AP scaling + combo validator.
4. Threat/ThreatTable — per-attacker threat + decay + taunt.
5. Balance/BalanceTuning — per-level curves + versioned patch diffs.
6. Validation/ContentValidator — champion/item linter.
7. Difficulty/BotDifficulty — 5 tiers + skillshot roll.
8. Encounters/EncounterScript — triggers, fire-once, complete.
9. HotReload/HotReloadConfig — live tuning + change events + typed getters.
10. DeterministicRng — xorshift128, reproducible per seed + named substreams.
Plus HUD Polish: ProceduralSprites (rounded/circle/ring/glow/gradient), UIStyle, UITween (easings/punch/count).
