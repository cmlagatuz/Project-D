# P12 — Systems Reference (Live-Ops Automation & Governance)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic, deterministic, auditable, tested (P12Tests, 16).
1.  ClanWars/ClanWarSeason — scheduled seasons, upset-scaled war points, rating matchmaking, leaderboard.
2.  Marketplace/AuctionHouse — bid/buyout, 5% fee, anti-snipe extension, settle.
3.  Pass/SeasonPass2 — free/premium/prestige tracks, prestige gated behind full premium, claim dedupe.
4.  Automation/LiveOpsScheduler — one-shot + recurring events activated by time, with an audit trail.
5.  Moderation/ModerationPipeline — report priority scoring + auto action routing (+ human escalation).
6.  LocOps/LocalizationBoard — translation workflow state machine + coverage-gated language shipping.
7.  DevPortal/ContentPortal — submit->review->publish with versioning + separation of duties.
8.  Incidents/IncidentManager — sev-tiered lifecycle, MTTR, postmortem gate to close Sev1/Sev2.
9.  Rollout/Rollout — staged % ramp (Canary..Full) with deterministic bucketing + auto kill-switch.
10. Compliance/ComplianceRules — age gates, region feature rules, data-retention purge scheduling.

## Design principles
Automated + auditable, safe-by-default (kill-switches, gates), fair/transparent (tiered + appealable),
compliant (age/region/retention first-class), deterministic (reproducible for audits + tests).

## Tests
P12Tests (16) covers every system.
