# Project D — Game Modes & the World Rift (mobile-first, competitive-fair)
> Codename Zantetsuken · CONFIDENTIAL · All original design. FF/summon/world names are placeholders.
- Ranked & Normal are LOCKED to ONE canonical map (Crystalline Rift) — enforced in code by
  GameModeRegistry. No map variety in competitive queues (fairness, no confusion).
- NEW rotating mode: THE WORLD RIFT — 5v5 on a random champion's realm (10 worlds), each with a
  map-wide World Modifier and a contestable Manifestation (world boss, Dissidia-NT-inspired): both
  teams fight it, the finishing blow CLAIMS it, granting a timed team buff + a one-shot Summon Strike.
  Excluded from Ranked; cosmetic rewards + separate casual rating only.
- Systems: GameModeRegistry, WorldCatalog (10 worlds), WorldSelector, WorldModifierEngine,
  ManifestationBoss, WorldEventDirector, WorldRiftMatch, WorldRiftRewards. Engine-agnostic + tested.
See docs/game-modes-and-world-rift.md.
