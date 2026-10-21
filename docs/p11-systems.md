# P11 — Systems Reference (Social Economy & Community)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic + tested (P11Tests, 15). Cosmetic/soft-currency only; anti-abuse guardrails.
1.  Guilds/GuildProgression — guild levels, perks by level, contribution ledger, weekly quests.
2.  Trading/TradeSystem — offer/accept/finalize, tradeable rules (flag + cooldown), acceptance reset on change.
3.  Crafting/CraftingEconomy — craft cosmetics (shards+essence), disenchant duplicates (40% back).
4.  Prestige/PrestigeSystem — prestige only at max level, cosmetic-only border rewards + badges.
5.  Collections/Album — set progress, completion, idempotent set-bonus claim.
6.  LiveTournaments/CommunityTournament — open registration + strong-vs-weak seeding + state machine.
7.  Predictions/PredictionMarket — FREE predict-the-winner, capped fun points, 2x payout (no real money).
8.  Rotation/RotationScheduler — deterministic weekly free rotation + new-player pool.
9.  Onboarding/OnboardingFunnel — staged milestones with prereq gating + feature unlocks + graduation.
10. Endorsements/EndorsementSystem — positive-only endorsements, per-voter cap, reputation levels.

Plus: PostGame Results screen wired to a POLISHED Match Analytics view (animated fairness gauge,
timeline draw, staggered scoreboards, MVP glow). Scene: Tools > Project D > Build PostGame Results+Analytics.

## Tests
P11Tests (15) covers every system.
