# Transport Adapters — Netcode / Mirror

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

The game logic depends only on `ITransport` (and `IAuthoritativeServer` above it).
`UdpTransport` is the bundled, dependency-free implementation that runs today. To
move to a managed stack at scale, implement `ITransport` once — no gameplay,
session, reconnect, or lag-compensation code changes.

## Unity Netcode for GameObjects (NGO)
Wrap `NetworkManager` + a `UnityTransport`:
- `StartServer(port)` → configure `UnityTransport.SetConnectionData` and `NetworkManager.StartServer()`.
- `Connect(host, port)` → `SetConnectionData(host, port)` + `StartClient()`.
- `Send(...)` / `Broadcast(...)` → send a `FastBufferWriter` over a named
  `CustomMessagingManager` channel; map `SendMode.ReliableOrdered` →
  `NetworkDelivery.ReliableSequenced`, `Unreliable` → `Unreliable`.
- `OnPeerConnected/Disconnected` → subscribe to `NetworkManager.OnClientConnected/DisconnectCallback`.
- `Poll()` → no-op (NGO pumps internally) or flush queued messages.
- `ConnectionId` ← wrap `clientId` (ulong) as int.

## Mirror
Wrap a `Transport` (KCP/Telepathy):
- Server: `transport.ServerStart()`; forward `OnServerConnected/DataReceived/Disconnected`.
- Client: `transport.ClientConnect(host)`; forward `OnClientDataReceived`.
- `Send` → `ServerSend/ClientSend` with `Channels.Reliable` or `Channels.Unreliable`.

## Contract requirements
An adapter must provide:
1. Unreliable datagram delivery (movement, snapshots) — loss-tolerant.
2. A reliable-ordered channel (connect handshake, cast acks, full state).
3. Connect + disconnect signals (disconnect feeds `SessionManager` reconnect grace).
4. `Poll()` that raises events on the main thread.

Because `AuthoritativeMatchServer`, `SessionManager`, `LagCompensator`, and
`WorldSnapshot` are engine-agnostic, the same server binary logic runs on a
headless dedicated server regardless of the chosen transport.
