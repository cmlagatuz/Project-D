# Project D — Production Sprint Plan

> Codename **Zantetsuken** · CONFIDENTIAL · GCD §15.1 / §15.3

- **Sprint 1 — Core loop skeleton.** Movement, camera, basic attack, one champion. ✅
- **Sprint 2 — Objectives fairness.** Seeded summon shuffle-bag + divine rotation. ✅
- **Sprint 3 — Lanes, minions, structures.** Goblin waves, towers, lane seal, Crystal Core. ✅
- **Sprint 4 — One champion per role + jungle.** First 10 champions authored. ✅
- **Sprint 5 — Networking vertical.**
  - [x] `IAuthoritativeServer` contract (cast ack/reject + state correction).
  - [x] Client prediction + reconciliation (`NetworkedCastController`, `ClientMoveReconciler`).
  - [x] Champion Select / draft (controller + view + scene builder).
  - [x] **Real transport** (`ITransport` + dependency-free `UdpTransport`; Netcode/Mirror adapter guide).
  - [x] **Authoritative dedicated server** (`AuthoritativeMatchServer`) replacing the loopback.
  - [x] **Reconnect + resync** (`SessionManager` grace/token reclaim, `ReconnectController` backoff, `FullState`/`ResyncRequest`).
  - [x] **Lag-compensated hit validation** (`EntityHistoryBuffer` + `LagCompensator` rewind, clamped for anti-abuse).
  - [x] Unit tests: lag comp, sessions/reconnect, snapshot serialization.
  - [x] Headless **dedicated-server build target** (`DedicatedServerBootstrap` + `DedicatedServerBuild`).
  - [x] **Two-client test harness** (`TwoClientHarness` + scene builder) for multi-client/reconnect.
  - [x] Lag-comp history for **all entities** (minions/monsters) via `SimEntityRegistry`.
  - [x] **Interest-managed, per-viewer delta snapshots** (`InterestManager`).
  - [x] Per-unit **model folders** + `ModelLinker` one-click prefab binding.
  - **Next:** productionize transport (Netcode/Mirror adapter), matchmaking handshake,
    persistent audit logs, and objective/structure spawns into the sim registry.
- **Sprint 6 — Vertical slice polish + gate** (GCD Appendix C).
