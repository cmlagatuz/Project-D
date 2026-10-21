// Project D  |  Codename: Zantetsuken
// AuthoritativeMatchServer.cs  (Sprint 5 — dedicated server over real transport)
//
// The authoritative simulation. Runs on a headless build (or in-editor for
// testing). It owns positions, cooldowns, resource, damage, objective ownership,
// sessions, and hit validation. Clients only send intents; the server validates
// everything and broadcasts snapshots (GCD 14.3).
//
// Wires together: ITransport (UdpTransport by default), SessionManager (reconnect),
// LagCompensator (rewind hit validation), and WorldSnapshot (resync). Champion
// combat numbers come from the existing engine-agnostic StatBlock/AbilitySystem.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Data;
using ProjectD.Gameplay.Combat;
using ProjectD.Gameplay.Champions;
using ProjectD.Networking.Transport;
using ProjectD.Networking.Reconnect;
using ProjectD.Networking.LagCompensation;
using ProjectD.Networking.Snapshot;

namespace ProjectD.Networking.Server
{
    public sealed class AuthoritativeMatchServer : MonoBehaviour
    {
        [Header("Transport")]
        [SerializeField] private int port = 7777;
        [SerializeField] private bool autoStart = true;

        [Header("Simulation")]
        [SerializeField] private ChampionDefinition defaultChampion;
        [SerializeField] private float tickRate = 30f;
        [SerializeField] private float snapshotRate = 20f;

        private ITransport _transport;
        private readonly SessionManager _sessions = new();
        private readonly LagCompensator _lagComp = new();
        private readonly ProjectD.Networking.Snapshot.InterestManager _interest = new();
        private readonly Dictionary<int, ServerEntity> _entities = new();
        // Per-viewer last snapshot, so delta compression is correct per connection.
        private readonly Dictionary<int, WorldSnapshot> _lastPerViewer = new();

        private float _serverTime;
        private int _tick;
        private int _nextEntityId = 1;
        private float _tickAccumulator, _snapshotAccumulator;

        private sealed class ServerEntity
        {
            public int EntityId;
            public int Team;
            public SimEntityKind Kind = SimEntityKind.Champion;
            public NetVec2 Position;
            public float Health, MaxHealth;
            public bool Alive = true;
            public float EstimatedRtt = 0.09f;      // updated from heartbeats
            public float InterpDelay = 0.1f;
            public float HitRadius = 0.5f;
            public float VisionRadius = 12f;
            public bool AlwaysRelevant;             // structures/objectives
            public readonly StatBlock Stats = new();
            public readonly AbilitySystem Abilities = new();
            public int LastProcessedInput;
        }

        public float ServerTime => _serverTime;
        public SessionManager Sessions => _sessions;
        public LagCompensator LagCompensator => _lagComp;

        // ---- Headless / bootstrap configuration ---- //
        public void ConfigureForHeadless(ChampionDefinition champ, int listenPort, int tick)
        {
            defaultChampion = champ;
            port = listenPort;
            tickRate = Mathf.Max(10, tick);
            autoStart = false; // bootstrap starts it explicitly
            StartServer();
        }

        /// <summary>Spawn a handful of neutral units so interest management/lag-comp
        /// have non-player targets to replicate in a bare server or greybox test.</summary>
        public void SpawnGreyboxNeutrals()
        {
            // Two lanes of minions + a jungle monster, team 2 (neutral) for the demo.
            for (int i = 0; i < 6; i++)
                SpawnNonPlayer(SimEntityKind.Minion, team: 2,
                    new NetVec2(-20 + i * 6, 0), maxHealth: 300, hitRadius: 0.4f, vision: 8f);
            SpawnNonPlayer(SimEntityKind.Monster, team: 2, new NetVec2(-14, 14), maxHealth: 1800, hitRadius: 1.2f, vision: 6f);
            SpawnNonPlayer(SimEntityKind.Monster, team: 2, new NetVec2(14, -14), maxHealth: 1800, hitRadius: 1.2f, vision: 6f);
            Debug.Log($"[Project D][Server] Spawned greybox neutrals; total entities: {_entities.Count}");
        }

        public int SpawnNonPlayer(SimEntityKind kind, int team, NetVec2 pos, float maxHealth,
                                  float hitRadius, float vision, bool alwaysRelevant = false)
        {
            int id = _nextEntityId++;
            var e = new ServerEntity
            {
                EntityId = id, Team = team, Kind = kind, Position = pos,
                Health = maxHealth, MaxHealth = maxHealth, HitRadius = hitRadius,
                VisionRadius = vision, AlwaysRelevant = alwaysRelevant
            };
            e.Stats.BaseMoveSpeed = kind == SimEntityKind.Minion ? 3.0f : 0f;
            _entities[id] = e;
            return id;
        }

        private void Start()
        {
            if (autoStart) StartServer();
        }

        public void StartServer(ITransport transportOverride = null)
        {
            _transport = transportOverride ?? new UdpTransport();
            _transport.OnPeerConnected += _ => { /* wait for ConnectRequest payload */ };
            _transport.OnPeerDisconnected += HandlePeerDisconnected;
            _transport.OnData += HandleData;
            _transport.StartServer(port);
            Debug.Log($"[Project D] Authoritative server listening on :{port}");
        }

        private void OnDestroy() => _transport?.Dispose();

        private void Update()
        {
            if (_transport == null) return;
            _transport.Poll();

            float dt = Time.deltaTime;
            _serverTime += dt;

            _tickAccumulator += dt;
            float tickStep = 1f / tickRate;
            while (_tickAccumulator >= tickStep)
            {
                _tickAccumulator -= tickStep;
                SimulationTick(tickStep);
            }

            _snapshotAccumulator += dt;
            float snapStep = 1f / snapshotRate;
            if (_snapshotAccumulator >= snapStep)
            {
                _snapshotAccumulator = 0f;
                BroadcastSnapshot();
            }

            foreach (var expired in _sessions.ExpireStale(_serverTime))
                Debug.Log($"[Project D] Session for entity {expired.EntityId} expired (grace elapsed).");
        }

        // ------------------------------------------------------------------ //
        private void SimulationTick(float dt)
        {
            _tick++;
            foreach (var e in _entities.Values)
            {
                if (e.Kind == SimEntityKind.Champion)
                {
                    e.Abilities.ServerTime = _serverTime;
                    if (defaultChampion != null)
                        e.Abilities.CurrentResource = Mathf.Min(defaultChampion.baseResource,
                            e.Abilities.CurrentResource + 8f * dt);
                }

                // Record authoritative position into the lag-comp history each tick —
                // for ALL entities (champions, minions, monsters), so any of them can
                // be lag-compensated when validating a hit.
                _lagComp.Record(e.EntityId, _serverTime, e.Position, e.HitRadius, e.Alive);
            }
        }

        // ------------------------------------------------------------------ //
        private void HandleData(ConnectionId conn, byte[] payload)
        {
            var r = new NetReader(payload);
            var msg = (NetMsg)r.Byte();
            switch (msg)
            {
                case NetMsg.ConnectRequest: HandleConnectRequest(conn, r); break;
                case NetMsg.MoveIntent: HandleMove(conn, r); break;
                case NetMsg.CastIntent: HandleCast(conn, r); break;
                case NetMsg.ResyncRequest: SendFullState(conn); break;
                case NetMsg.Heartbeat: HandleHeartbeat(conn, r); break;
                case NetMsg.Disconnect: _transport.DisconnectPeer(conn); break;
            }
        }

        private void HandleConnectRequest(ConnectionId conn, NetReader r)
        {
            string token = r.Str();       // empty = brand new; non-empty = reconnect attempt
            int requestedTeam = r.Int();

            PlayerSession session = _sessions.TryReconnect(token, conn, _serverTime);
            bool reconnized = session != null;

            if (!reconnized)
            {
                int entityId = _nextEntityId++;
                session = _sessions.CreateSession(conn, entityId, requestedTeam);
                var e = new ServerEntity
                {
                    EntityId = entityId, Team = requestedTeam, Kind = SimEntityKind.Champion,
                    Position = new NetVec2(-30, -30), HitRadius = 0.5f, VisionRadius = 12f
                };
                InitEntityStats(e);
                _entities[entityId] = e;
                Debug.Log($"[Project D] New player -> entity {entityId}, token issued.");
            }
            else
            {
                Debug.Log($"[Project D] Reconnect accepted for entity {session.EntityId} (token valid).");
            }

            // Reply with the assignment + token, then a full state to (re)sync.
            var w = new NetWriter();
            w.Byte((byte)NetMsg.ConnectAccept);
            w.Int(session.EntityId);
            w.Str(session.Token);
            w.Int(_tick);
            w.Float(_serverTime);
            w.Bool(reconnized);
            _transport.Send(conn, w.ToArray(), SendMode.ReliableOrdered);

            SendFullState(conn);
        }

        private void HandleMove(ConnectionId conn, NetReader r)
        {
            var session = _sessions.GetByConnection(conn);
            if (session == null || !_entities.TryGetValue(session.EntityId, out var e)) return;

            int seq = r.Int();
            NetVec2 desired = r.Vec2();

            // Validate against authoritative speed (anti-teleport).
            float maxStep = e.Stats.MoveSpeed * (1f / tickRate) * 1.2f;
            NetVec2 delta = new NetVec2(desired.X - e.Position.X, desired.Z - e.Position.Z);
            float dist = e.Position.DistanceTo(desired);
            if (dist > maxStep && dist > 0f)
                e.Position = new NetVec2(e.Position.X + delta.X / dist * maxStep,
                                         e.Position.Z + delta.Z / dist * maxStep);
            else
                e.Position = desired;

            e.LastProcessedInput = Mathf.Max(e.LastProcessedInput, seq);
            session.LastAckedInput = e.LastProcessedInput;

            // Movement correction back to the owner (reconciliation).
            var w = new NetWriter();
            w.Byte((byte)NetMsg.StateCorrection);
            w.Int(e.LastProcessedInput);
            w.Vec2(e.Position);
            w.Float(e.Abilities.CurrentResource);
            _transport.Send(conn, w.ToArray(), SendMode.Unreliable);
        }

        private void HandleCast(ConnectionId conn, NetReader r)
        {
            var session = _sessions.GetByConnection(conn);
            if (session == null || !_entities.TryGetValue(session.EntityId, out var e)) return;

            int seq = r.Int();
            int slotInt = r.Int();
            NetVec2 origin = r.Vec2();
            NetVec2 aim = r.Vec2();

            var slot = (AbilitySlot)slotInt;
            AbilityDefinition ability = ResolveAbility(slot);

            var w = new NetWriter();
            w.Byte((byte)NetMsg.CastResolved);
            w.Int(seq);
            w.Int(slotInt);

            if (ability == null)
            {
                w.Bool(false); w.Str("InvalidAbility"); w.Float(e.Abilities.CurrentResource); w.Float(0); w.Int(-1);
            }
            else
            {
                bool originOk = origin.DistanceTo(e.Position) <= 2.0f;   // origin must match server pos
                CastResult result = e.Abilities.TryCast(ability, e.Stats);

                if (result == CastResult.Success && originOk)
                {
                    // LAG-COMPENSATED hit validation: rewind targets to shooter render time.
                    var query = new HitQuery
                    {
                        ShooterEntityId = e.EntityId,
                        ShooterTeam = e.Team,
                        Origin = e.Position,
                        Direction = aim,
                        Range = ability.range,
                        ProjectileRadius = ability.radius,
                        ShooterRttSeconds = e.EstimatedRtt,
                        ShooterInterpDelaySeconds = e.InterpDelay
                    };
                    HitResult hit = _lagComp.ValidateLineHit(query, _serverTime);

                    float dmg = e.Abilities.ResolveDamage(ability, e.Stats);
                    if (hit.Hit && _entities.TryGetValue(hit.TargetEntityId, out var target) &&
                        target.Team != e.Team && target.Alive)
                    {
                        ApplyDamage(target, dmg, ability.damageType, e);
                        Debug.Log($"[Project D] CONFIRMED hit {e.EntityId}->{hit.TargetEntityId} " +
                                  $"(rewound to t={hit.RewoundToTime:F3}, dmg={dmg:F0})");
                    }

                    w.Bool(true); w.Str(string.Empty);
                    w.Float(e.Abilities.CurrentResource);
                    w.Float(dmg);
                    w.Int(hit.Hit ? hit.TargetEntityId : -1);
                }
                else
                {
                    w.Bool(false); w.Str(originOk ? result.ToString() : "OutOfSync");
                    w.Float(e.Abilities.CurrentResource); w.Float(0); w.Int(-1);
                }
            }
            _transport.Send(conn, w.ToArray(), SendMode.ReliableOrdered);
        }

        private void HandleHeartbeat(ConnectionId conn, NetReader r)
        {
            var session = _sessions.GetByConnection(conn);
            if (session == null || !_entities.TryGetValue(session.EntityId, out var e)) return;
            float clientSendTime = r.Float();
            // Simple RTT estimate: server round-trips the client's timestamp elsewhere;
            // here we accept a client-reported RTT sample and clamp it for safety.
            float reportedRtt = r.Float();
            e.EstimatedRtt = Mathf.Clamp(reportedRtt, 0f, 0.4f);
        }

        private void HandlePeerDisconnected(ConnectionId conn)
        {
            var session = _sessions.MarkDisconnected(conn, _serverTime);
            if (session != null)
                Debug.Log($"[Project D] Entity {session.EntityId} disconnected; holding for reconnect " +
                          $"({_sessions.ReconnectGraceSeconds:F0}s grace).");
        }

        // ------------------------------------------------------------------ //
        private void ApplyDamage(ServerEntity target, float amount, ProjectD.Core.DamageType type, ServerEntity source)
        {
            var dmg = new DamageInstance { Amount = amount, Type = type, SourceEntityId = source.EntityId, SourceTeam = source.Team };
            float dealt = DamageSystem.Resolve(dmg, target.Stats.Armor, target.Stats.MagicResist, target.Health, target.MaxHealth);
            target.Health = Mathf.Max(0f, target.Health - dealt);
            if (target.Health <= 0f) target.Alive = false;
            // Health change is reflected in the next interest-managed snapshot tick.
        }

        // Interest-managed broadcast: each connected player gets ONLY the entities
        // relevant to them (own team + always-relevant objects + enemies/neutrals in
        // their team's vision), delta-compressed against that viewer's last snapshot.
        private void BroadcastSnapshot()
        {
            var view = BuildReplicationView();
            foreach (var session in _sessions.All)
            {
                if (session.State != SessionState.Active) continue;
                if (!_entities.TryGetValue(session.EntityId, out var viewerEntity)) continue;

                var relevant = _interest.BuildRelevantSet(session.EntityId, (byte)viewerEntity.Team, view);
                var snap = _interest.ToSnapshot(_tick, _serverTime, relevant);

                _lastPerViewer.TryGetValue(session.EntityId, out var baseline);
                var toSend = baseline != null ? snap.MakeDeltaFrom(baseline) : snap;

                var w = new NetWriter();
                w.Byte((byte)NetMsg.Snapshot);
                toSend.Write(w);
                _transport.Send(session.Connection, w.ToArray(), SendMode.Unreliable);

                _lastPerViewer[session.EntityId] = snap;
            }
        }

        // Full keyframe for a specific viewer (connect / reconnect / resync).
        private void SendFullState(ConnectionId conn)
        {
            var session = _sessions.GetByConnection(conn);
            List<Snapshot.ReplicatedEntity> view = BuildReplicationView();

            WorldSnapshot snap;
            if (session != null && _entities.TryGetValue(session.EntityId, out var viewerEntity))
            {
                var relevant = _interest.BuildRelevantSet(session.EntityId, (byte)viewerEntity.Team, view);
                snap = _interest.ToSnapshot(_tick, _serverTime, relevant);
                _lastPerViewer[session.EntityId] = snap; // reset delta baseline on resync
            }
            else
            {
                snap = _interest.ToSnapshot(_tick, _serverTime, view); // pre-entity: send all
            }
            snap.IsKeyframe = true; snap.BaselineTick = -1;

            var w = new NetWriter();
            w.Byte((byte)NetMsg.FullState);
            snap.Write(w);
            _transport.Send(conn, w.ToArray(), SendMode.ReliableOrdered);
        }

        private List<Snapshot.ReplicatedEntity> BuildReplicationView()
        {
            var list = new List<Snapshot.ReplicatedEntity>(_entities.Count);
            foreach (var e in _entities.Values)
                list.Add(new Snapshot.ReplicatedEntity
                {
                    EntityId = e.EntityId, Team = (byte)e.Team, Position = e.Position,
                    Health = e.Health, MaxHealth = e.MaxHealth,
                    Resource = e.Kind == SimEntityKind.Champion ? e.Abilities.CurrentResource : 0f,
                    Alive = e.Alive, AlwaysRelevant = e.AlwaysRelevant, VisionRadius = e.VisionRadius
                });
            return list;
        }

        private void InitEntityStats(ServerEntity e)
        {
            var c = defaultChampion;
            if (c == null) { e.MaxHealth = e.Health = 600; e.Stats.BaseMoveSpeed = 3.5f; return; }
            e.Stats.BaseMaxHealth = c.baseHealth; e.Stats.BaseAttackDamage = c.baseAttackDamage;
            e.Stats.BaseMagicPower = c.baseMagicPower; e.Stats.BaseArmor = c.baseArmor;
            e.Stats.BaseMagicResist = c.baseMagicResist; e.Stats.BaseMoveSpeed = c.baseMoveSpeed;
            e.Stats.BaseAttackSpeed = c.baseAttacksPerSecond;
            e.MaxHealth = e.Health = c.baseHealth;
            e.Abilities.CurrentResource = c.baseResource;
            e.Abilities.LearnOrRankUp(AbilitySlot.Q); e.Abilities.LearnOrRankUp(AbilitySlot.W);
            e.Abilities.LearnOrRankUp(AbilitySlot.E); e.Abilities.LearnOrRankUp(AbilitySlot.Ultimate);
        }

        private AbilityDefinition ResolveAbility(AbilitySlot slot) => defaultChampion == null ? null : slot switch
        {
            AbilitySlot.Q => defaultChampion.abilityQ,
            AbilitySlot.W => defaultChampion.abilityW,
            AbilitySlot.E => defaultChampion.abilityE,
            AbilitySlot.Ultimate => defaultChampion.ultimate,
            _ => null
        };
    }
}
