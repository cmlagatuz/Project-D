# Authoritative Match Server — Spec (Sprint 5)

> Project D · Codename **Zantetsuken** · CONFIDENTIAL · GCD §14.2/§14.3/§14.4

The client is never trusted. Clients send intents; the server validates and
broadcasts results. Contract: `Assets/Scripts/Networking/IAuthoritativeServer.cs`.
Transport contract: `Assets/Scripts/Networking/Transport/ITransport.cs`.

## Implemented
- **`AuthoritativeMatchServer`** — fixed-tick sim (30 Hz), snapshot broadcast (20 Hz),
  authoritative movement (anti-teleport clamp), cast validation, damage resolution,
  and delta/keyframe snapshots.
- **`UdpTransport`** — real, dependency-free UDP with peer tracking, heartbeat
  timeouts, and a light reliable-ordered channel.
- **`SessionManager`** — session tokens, disconnect grace, same-entity reclaim.
- **`LagCompensator`** + **`EntityHistoryBuffer`** — rewind hit validation, clamped.
- **`WorldSnapshot`** — full + delta state for resync.

## Cast flow
1. Client predicts, sends `CastIntent` (seq #).
2. Server validates rank/cooldown/resource/origin; runs lag-compensated hit test.
3. `CastResolved{Accepted|Reject}` on the reliable channel; damage broadcast via snapshot.
4. `StateCorrection` + snapshots reconcile predicted client state.

## Reconnect flow
1. Peer timeout → `SessionManager.MarkDisconnected` (entity held, grace timer starts).
2. Client `ReconnectController` retries with the saved token.
3. `SessionManager.TryReconnect` reattaches the same entity; server sends `FullState`.

## To productionize
- Swap `UdpTransport` for Netcode/Mirror via `ITransport` (guide in
  `Transport/README_ADAPTERS.md`).
- Record movement history for all entities (minions/monsters), interest-managed
  snapshots, headless build target, matchmaking handshake, and persistent audit logs.
