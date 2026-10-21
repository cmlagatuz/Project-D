// Project D  |  Codename: Zantetsuken
// NetworkTransportClient.cs  (Sprint 5 — client over real transport)
//
// Implements IAuthoritativeServer by talking to AuthoritativeMatchServer over an
// ITransport (UdpTransport by default). This is the drop-in REPLACEMENT for
// LocalLoopbackServer: PrototypePawn, NetworkedCastController and
// ClientMoveReconciler are unchanged because they only depend on the interface.
//
// Responsibilities:
//   - Connect / reconnect (remembers the session token to reclaim the same entity).
//   - Translate MoveIntent/CastIntent structs into wire messages.
//   - Parse ConnectAccept / CastResolved / StateCorrection / Snapshot / FullState
//     and raise the IAuthoritativeServer events the game code already listens to.
//   - Drive the ReconnectController on transport disconnects.

using System;
using UnityEngine;
using ProjectD.Networking.Transport;
using ProjectD.Networking.Reconnect;
using ProjectD.Networking.Snapshot;

namespace ProjectD.Networking
{
    public sealed class NetworkTransportClient : MonoBehaviour, IAuthoritativeServer
    {
        [Header("Server")]
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 7777;
        [SerializeField] private int requestedTeam = 0;
        [SerializeField] private bool autoConnect = true;

        private UdpTransport _transport;
        private ReconnectController _reconnect;
        private float _serverTimeBase;
        private float _localClock;

        public int LocalEntityId { get; private set; } = -1;
        public string SessionToken { get; private set; } = string.Empty;
        public bool IsConnected { get; private set; }
        public float ServerTime => _serverTimeBase + _localClock;

        // Last authoritative position of the local entity (from snapshots), so
        // callers/harness can supply a valid cast origin without a scene transform.
        public float LastKnownX { get; private set; }
        public float LastKnownZ { get; private set; }

        /// <summary>Programmatic setup used by the test harness / multi-client scenes.</summary>
        public void ConfigureEndpoint(string serverHost, int serverPort, int team, bool autoConnect)
        {
            host = serverHost; port = serverPort; requestedTeam = team; this.autoConnect = autoConnect;
        }

        /// <summary>Test hook: forcibly drop the socket to exercise reconnect/resync.</summary>
        public void SimulateConnectionLoss()
        {
            _transport?.Dispose();
            _transport = null;
            HandleDisconnected();
        }

        // IAuthoritativeServer events
        public event Action<int, float, float> OnEntityMoved;
        public event Action<int, float, float> OnHealthChanged;
        public event Action<CastAck> OnCastResolved;
        public event Action<StateCorrection> OnStateCorrection;
        public event Action<string> OnObjectiveRevealed;
        public event Action<int> OnMatchEnded;

        // Reconnect surface for UI.
        public ReconnectController Reconnect => _reconnect;

        private void Awake()
        {
            _reconnect = new ReconnectController(BeginConnect);
        }

        private void Start()
        {
            if (autoConnect) BeginConnect(isReconnect: false);
        }

        private void OnDestroy() => _transport?.Dispose();

        public void BeginConnect(bool isReconnect)
        {
            _transport?.Dispose();
            _transport = new UdpTransport();
            _transport.OnData += HandleData;
            _transport.OnPeerDisconnected += _ => HandleDisconnected();
            _transport.Connect(host, port);

            var w = new NetWriter();
            w.Byte((byte)NetMsg.ConnectRequest);
            w.Str(isReconnect ? SessionToken : string.Empty);
            w.Int(requestedTeam);
            _transport.SendToServer(w.ToArray(), SendMode.ReliableOrdered);

            Debug.Log(isReconnect
                ? "[Project D] Reconnecting with saved session token…"
                : "[Project D] Connecting to authoritative server…");
        }

        private float _connectRetryTimer;

        private void Update()
        {
            if (_transport == null) return;
            _localClock += Time.deltaTime;
            _transport.Poll();

            // Initial-connect robustness: UDP is connectionless and the server may
            // bind a frame later, so resend ConnectRequest until we get ConnectAccept.
            if (!IsConnected && _reconnect.State != ClientLinkState.Reconnecting)
            {
                _connectRetryTimer -= Time.deltaTime;
                if (_connectRetryTimer <= 0f)
                {
                    _connectRetryTimer = 0.5f;
                    var cw = new NetWriter();
                    cw.Byte((byte)NetMsg.ConnectRequest);
                    cw.Str(string.IsNullOrEmpty(SessionToken) ? string.Empty : SessionToken);
                    cw.Int(requestedTeam);
                    _transport.SendToServer(cw.ToArray(), SendMode.ReliableOrdered);
                }
            }

            // Heartbeat so the server can detect liveness + estimate RTT.
            if (IsConnected && Time.frameCount % 30 == 0)
            {
                var w = new NetWriter();
                w.Byte((byte)NetMsg.Heartbeat);
                w.Float(Time.time);
                w.Float(_reconnect.EstimatedRtt);
                _transport.SendToServer(w.ToArray(), SendMode.Unreliable);
            }

            _reconnect.Tick(Time.deltaTime);
        }

        // ---- IAuthoritativeServer: client -> server ---- //
        public void SendMove(in MoveIntent intent)
        {
            if (_transport == null) return;
            var w = new NetWriter();
            w.Byte((byte)NetMsg.MoveIntent);
            w.Int(intent.SequenceNumber);
            w.Vec2(new NetVec2(intent.TargetX, intent.TargetZ));
            _transport.SendToServer(w.ToArray(), SendMode.Unreliable);
        }

        public void SendCast(in CastIntent intent)
        {
            if (_transport == null) return;
            var w = new NetWriter();
            w.Byte((byte)NetMsg.CastIntent);
            w.Int(intent.SequenceNumber);
            w.Int(intent.AbilitySlot);
            w.Vec2(new NetVec2(intent.OriginX, intent.OriginZ));
            w.Vec2(new NetVec2(intent.AimX, intent.AimZ));
            _transport.SendToServer(w.ToArray(), SendMode.ReliableOrdered);
        }

        public void SendPing(int worldX, int worldZ, byte pingType) { /* prototype: optional */ }

        public void RequestResync()
        {
            if (_transport == null) return;
            var w = new NetWriter();
            w.Byte((byte)NetMsg.ResyncRequest);
            _transport.SendToServer(w.ToArray(), SendMode.ReliableOrdered);
        }

        // ---- server -> client ---- //
        private void HandleData(ConnectionId _, byte[] payload)
        {
            var r = new NetReader(payload);
            var msg = (NetMsg)r.Byte();
            switch (msg)
            {
                case NetMsg.ConnectAccept: HandleConnectAccept(r); break;
                case NetMsg.CastResolved: HandleCastResolved(r); break;
                case NetMsg.StateCorrection: HandleStateCorrection(r); break;
                case NetMsg.Snapshot:
                case NetMsg.FullState: HandleSnapshot(r); break;
                case NetMsg.MatchEnded: OnMatchEnded?.Invoke(r.Int()); break;
            }
        }

        private void HandleConnectAccept(NetReader r)
        {
            LocalEntityId = r.Int();
            SessionToken = r.Str();
            r.Int();                       // tick
            _serverTimeBase = r.Float();
            _localClock = 0f;
            bool reconnected = r.Bool();
            IsConnected = true;
            _reconnect.NotifyConnected();
            Debug.Log(reconnected
                ? $"[Project D] Reconnected as entity {LocalEntityId}. Resyncing…"
                : $"[Project D] Connected as entity {LocalEntityId}. Session token stored.");
        }

        private void HandleCastResolved(NetReader r)
        {
            var ack = new CastAck
            {
                SequenceNumber = r.Int(),
                AbilitySlot = r.Int(),
                Accepted = r.Bool(),
                RejectReason = r.Str(),
                ServerResourceAfter = r.Float(),
                PredictedDamage = r.Float()
            };
            r.Int();                      // hit target entity id (for VFX/telemetry)
            OnCastResolved?.Invoke(ack);
        }

        private void HandleStateCorrection(NetReader r)
        {
            // Wire format must match AuthoritativeMatchServer.HandleMove:
            //   Int(LastProcessedInput), Vec2(position), Float(resource)
            int lastInput = r.Int();
            NetVec2 pos = r.Vec2();
            float resource = r.Float();
            OnStateCorrection?.Invoke(new StateCorrection
            {
                LastProcessedInput = lastInput,
                X = pos.X,
                Z = pos.Z,
                Resource = resource
            });
        }

        private void HandleSnapshot(NetReader r)
        {
            var snap = WorldSnapshot.Read(r);
            _serverTimeBase = snap.ServerTime; _localClock = 0f;
            foreach (var e in snap.Entities)
            {
                OnEntityMoved?.Invoke(e.EntityId, e.Position.X, e.Position.Z);
                OnHealthChanged?.Invoke(e.EntityId, e.Health, e.MaxHealth);
                if (e.EntityId == LocalEntityId)
                {
                    LastKnownX = e.Position.X;
                    LastKnownZ = e.Position.Z;
                    OnStateCorrection?.Invoke(new StateCorrection
                    {
                        LastProcessedInput = int.MaxValue, // snapshot supersedes; owner reconciles resource
                        X = e.Position.X, Z = e.Position.Z, Resource = e.Resource
                    });
                }
            }
        }

        private void HandleDisconnected()
        {
            if (!IsConnected) return;
            IsConnected = false;
            Debug.LogWarning("[Project D] Connection lost — entering reconnect flow.");
            _reconnect.NotifyDisconnected();
        }
    }
}
