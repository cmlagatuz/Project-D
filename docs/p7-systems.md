# P7 — Systems Reference (Backend Services & Infrastructure)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic + tested (P7Tests, 15).
1.  Matchmaker/Matchmaker — MMR ticket queue with widening windows; snake-draft balanced 5v5.
2.  Sessions/SessionManager — server allocation (region + capacity), lifecycle, release.
3.  Persistence/LeaderboardStore — sorted-set store; RankOf / Page / Around queries.
4.  AntiCheat/AntiCheatPipeline — weighted signal score (clamped) -> Watch/Shadowban/Suspend/Ban.
5.  Telemetry/TelemetrySink — event counts, unique users, ordered funnel conversion.
6.  CloudSave/CloudSaveService — optimistic concurrency; conflict merge by MAX per counter.
7.  Delivery/PatchManifest — download only changed files (hash diff) + resume from completed.
8.  LoadBalance/LoadBalancer — least-connections routing, region-preferred, cross-region failover.
9.  RateLimit/TokenBucket + RateLimiter — per-client/route token bucket with refill.
10. Backfill/MatchBackfill — fill leaver slots with the closest-MMR candidate in tolerance.

Plus: HUD Polish demo scene (Tools > Project D > Build Polished HUD Demo) with procedural art + animations.

## Tests
P7Tests (15) covers every system.
