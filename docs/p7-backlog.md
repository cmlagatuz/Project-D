# Project D — P7 Backlog (Backend Services & Infrastructure)

> Codename Zantetsuken · CONFIDENTIAL · All original design.
P7 = the server-side infrastructure that P0-P6 run on. These are authoritative,
engine-agnostic cores (no UnityEngine) that model the live-service backend so gameplay,
meta, and tooling have reliable services. Unit-tested + deterministic.

## 10 systems
1.  Matchmaker service — MMR-based ticket queue that forms balanced 5v5 matches.
2.  Session manager — game-server allocation, session lifecycle, capacity.
3.  Leaderboard persistence — sorted-set store with upsert + rank/page queries.
4.  Anti-cheat pipeline — signal ingestion -> weighted risk score -> action.
5.  Telemetry/analytics — event ingestion, funnels, and aggregate metrics.
6.  Cloud save — versioned save blobs with conflict resolution (last-writer + merge).
7.  Content delivery — patch manifest diff (download only changed files) + resume.
8.  Load balancing — least-connections routing across healthy nodes.
9.  Rate limiting — token-bucket per client/route to protect the API.
10. Match backfill — fill empty slots after a leaver mid-queue/mid-game.
