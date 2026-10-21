# Project D — Networking Architecture (Sprint 5, real transport)

> Codename **Zantetsuken** · CONFIDENTIAL · GCD §14.2/§14.3/§14.4

## Layered design

```
Game code (PrototypePawn, NetworkedCastController, ClientMoveReconciler)
        │  depends only on ↓
   IAuthoritativeServer  ─────────────► NetworkTransportClient (client)
                                              │  wire protocol (NetMsg)
                                              ▼
                                        ITransport  ◄── UdpTransport (bundled)
                                              ▲            or Netcode/Mirror adapter
                                              │
                                        AuthoritativeMatchServer (server)
                                              ├─ SessionManager      (reconnect/resync)
                                              ├─ LagCompensator       (rewind hit validation)
                                              └─ WorldSnapshot        (full/delta state)
```

The client and server share the engine-agnostic pieces (`NetPrimitives`,
`WorldSnapshot`, `SessionManager`, `LagCompensator`), so the same logic runs on a
headless dedicated server and inside unit tests.

## Transport
- **`ITransport`** — connect/disconnect signals, `Send`/`Broadcast` with
  `Unreliable` or `ReliableOrdered`, and `Poll()`.
- **`UdpTransport`** — dependency-free `System.Net.Sockets` UDP with peer tracking,
  heartbeat timeouts (feeds reconnect), and a light reliable-ordered channel
  (seq + ack) for the handshake, cast acks, and full-state packets.
- **Netcode / Mirror** — implement `ITransport` once; nothing above it changes.
  See `Assets/Scripts/Networking/Transport/README_ADAPTERS.md`.

## Server-authoritative cast + move flow
1. Client predicts locally (instant feel) and sends `MoveIntent` / `CastIntent`.
2. Server validates movement against authoritative speed (anti-teleport) and
   validates casts (rank/cooldown/resource/origin) — nothing is trusted.
3. Casts run **lag-compensated hit validation** (below); results broadcast in
   snapshots. Cast acks/rejects return on the reliable channel.
4. `StateCorrection` + snapshots reconcile the client's predicted state.

## Reconnect & resync
- On connect the server mints a **session token** (`SessionManager`) tied to the
  player's entity.
- If the transport peer times out, the session is flagged **Disconnected** and the
  entity is held for a **grace window** (default 90 s) instead of being removed.
- The client's **`ReconnectController`** retries with exponential backoff, resending
  the saved token. The server **reclaims the same entity** and streams a
  **`FullState`** keyframe so the client resyncs from authority.
- An explicit **`ResyncRequest`** lets a desynced-but-connected client pull a fresh
  keyframe at any time.

## Lag-compensated hit validation
- The server records every entity's authoritative position into an
  **`EntityHistoryBuffer`** each tick (a ring buffer of timestamped samples).
- When a client fires, **`LagCompensator`** rewinds all candidate targets to the
  shooter's render time — `serverNow − (RTT/2 + interpolationDelay)` — by
  interpolating the history, then runs a segment-vs-circle test.
- **Anti-abuse:** the rewind is clamped to `MaxRewindSeconds` (default 0.25 s), so a
  client cannot rewind targets arbitrarily far; dead-in-the-past targets are ignored;
  the reported client time is never trusted beyond the clamp.

## Tests (EditMode)
- `LagCompensationTests` — history interpolation/clamping, line-vs-circle, correct
  rewind hit at a target's past position, and rewind clamping for anti-abuse.
- `ReconnectSessionTests` — reclaim within grace, expiry after grace, unknown-token
  rejection, and the client backoff/failure state machine.
- `SnapshotSerializationTests` — full round-trip, delta-only-changed, and primitive
  reader/writer fidelity.

## Prototype note
The greybox scene runs **both** the `AuthoritativeMatchServer` and a
`NetworkTransportClient` in one process over UDP loopback (127.0.0.1:7777), so the
entire real-transport + reconnect + lag-comp stack executes on Play. For a true
multi-device test, run a headless server build and point clients at its address.
`LocalLoopbackServer` remains only as an offline test double.

---

## Sprint 5+ additions: dedicated server, interest management, multi-client

### Headless dedicated server
- **`DedicatedServerBootstrap`** — batchmode entry point; reads `-port` / `-tick`,
  disables vsync, sets a fixed server framerate, starts `AuthoritativeMatchServer`,
  and spawns greybox neutrals so replication has non-player targets.
- **Build:** *Tools ▸ Project D ▸ Build Headless Dedicated Server* (Linux/Windows,
  server subtarget). Run: `ProjectD_Server -batchmode -nographics -server -port 7777 -tick 30`.
- CLI/CI: `-executeMethod ProjectD.EditorTools.DedicatedServerBuild.BuildLinux`.

### Lag compensation for ALL entities
- `SimEntityRegistry` + the server now record **champions, minions, AND monsters**
  into `LagCompensator` history each tick, so a skillshot is rewound-validated
  against minions/jungle camps too — not just players.

### Interest-managed snapshots
- **`InterestManager`** builds a per-viewer relevant set: own team + always-relevant
  objects (structures/objectives) + enemies/neutrals inside the team's vision. The
  server sends **per-connection delta snapshots** against each viewer's own baseline.
- Benefits: bandwidth scales with what a player can see (not the whole world), and
  clients never receive fogged-enemy positions (anti-cheat).

### Multi-client / multi-device testing
- **`TwoClientHarness`** stands up one server + two clients in-process (UDP loopback),
  scripts opposing movement + a Q cast, and logs each client seeing the OTHER — proving
  cross-client replication and lag-compensated hits. A **Drop Client B** button
  exercises reconnect/resync live.
- **Build:** *Tools ▸ Project D ▸ Build Two-Client Test Harness Scene*.
- **True multi-device:** run the headless server build, point each device's
  `NetworkTransportClient` at its address (`ConfigureEndpoint`).

### New tests
- `InterestManagementTests` — own-team always sent, enemy hidden out of vision /
  revealed in vision, always-relevant structures, neutral minion vision gating.
