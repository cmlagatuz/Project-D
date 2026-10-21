// Project D  |  Codename: Zantetsuken
// LocalLoopbackServer.cs  (Sprint 5)  [SUPERSEDED — kept as an offline test double]
//
// STATUS: As of the real-transport work, the production path is:
//   NetworkTransportClient (IAuthoritativeServer)  <-- UdpTransport -->  AuthoritativeMatchServer
// This in-process loopback is RETAINED only for offline single-scene testing and
// as a reference authority. It runs the same predict -> validate -> reconcile loop
// with simulated latency but no sockets, sessions, or lag compensation.
//
// Use NetworkTransportClient for anything involving reconnect, resync, multiple
// clients, or lag-compensated hit validation. PrototypePawn works with either,
// because both implement IAuthoritativeServer.
//
// IMPORTANT: this is the authority. It owns cooldowns, resource, and movement
// validation. The client never decides outcomes (GCD 14.3).

using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectD.Data;
using ProjectD.Gameplay.Combat;
using ProjectD.Gameplay.Champions;

namespace ProjectD.Networking
{
    public sealed class LocalLoopbackServer : MonoBehaviour, IAuthoritativeServer
    {
        [Header("Simulated Network")]
        [Tooltip("One-way latency in milliseconds applied to messages both directions.")]
        [SerializeField] private float oneWayLatencyMs = 45f;
        [Tooltip("Extra random jitter added to each message (ms).")]
        [SerializeField] private float jitterMs = 15f;

        [Header("Authoritative Champion (prototype: one local pawn)")]
        [SerializeField] private ChampionDefinition champion;
        [SerializeField] private int localEntityId = 1;
        [SerializeField] private int localTeam = 0;

        // Authoritative state for the one controlled pawn.
        private readonly AbilitySystem _serverAbilities = new();
        private readonly StatBlock _serverStats = new();
        private Vector3 _serverPos;
        private int _lastProcessedInput;
        private float _serverTime;

        // Deferred message queue to simulate latency.
        private readonly List<(float dueTime, Action deliver)> _pending = new();
        private System.Random _rng;

        public float ServerTime => _serverTime;

        public event Action<int, float, float> OnEntityMoved;
        public event Action<int, float, float> OnHealthChanged;
        public event Action<CastAck> OnCastResolved;
        public event Action<StateCorrection> OnStateCorrection;
        public event Action<string> OnObjectiveRevealed;
        public event Action<int> OnMatchEnded;

        private void Awake()
        {
            _rng = new System.Random(12345);
            InitServerState();
        }

        public void Configure(ChampionDefinition def, Vector3 startPos)
        {
            champion = def;
            _serverPos = startPos;
            InitServerState();
        }

        private void InitServerState()
        {
            if (champion == null) return;
            _serverStats.BaseMaxHealth = champion.baseHealth;
            _serverStats.BaseAttackDamage = champion.baseAttackDamage;
            _serverStats.BaseMagicPower = champion.baseMagicPower;
            _serverStats.BaseArmor = champion.baseArmor;
            _serverStats.BaseMagicResist = champion.baseMagicResist;
            _serverStats.BaseMoveSpeed = champion.baseMoveSpeed;
            _serverStats.BaseAttackSpeed = champion.baseAttacksPerSecond;
            _serverAbilities.CurrentResource = champion.baseResource;
            _serverAbilities.LearnOrRankUp(AbilitySlot.Q);
            _serverAbilities.LearnOrRankUp(AbilitySlot.W);
            _serverAbilities.LearnOrRankUp(AbilitySlot.E);
            _serverAbilities.LearnOrRankUp(AbilitySlot.Ultimate);
        }

        private void Update()
        {
            _serverTime += Time.deltaTime;
            _serverAbilities.ServerTime = _serverTime;

            // Authoritative resource regen (prototype value).
            if (champion != null)
                _serverAbilities.CurrentResource = Mathf.Min(
                    champion.baseResource, _serverAbilities.CurrentResource + 8f * Time.deltaTime);

            // Deliver any messages whose simulated latency has elapsed.
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (_serverTime >= _pending[i].dueTime)
                {
                    var deliver = _pending[i].deliver;
                    _pending.RemoveAt(i);
                    deliver();
                }
            }
        }

        private float Delay() => (oneWayLatencyMs + (float)_rng.NextDouble() * jitterMs) / 1000f;
        private void Defer(Action a) => _pending.Add((_serverTime + Delay(), a));

        // ---- Client -> Server -------------------------------------------- //
        public void SendMove(in MoveIntent intent)
        {
            var copy = intent;
            Defer(() => ServerProcessMove(copy));
        }

        public void SendCast(in CastIntent intent)
        {
            var copy = intent;
            Defer(() => ServerProcessCast(copy));
        }

        public void SendPing(int worldX, int worldZ, byte pingType) { /* prototype: no-op */ }

        // ---- Authoritative processing (server side) ---------------------- //
        private void ServerProcessMove(MoveIntent m)
        {
            _lastProcessedInput = Mathf.Max(_lastProcessedInput, m.SequenceNumber);

            // Validate: clamp movement to the champion's authoritative speed.
            Vector3 desired = new Vector3(m.TargetX, 0f, m.TargetZ);
            float maxStep = _serverStats.MoveSpeed * Time.deltaTime * 1.15f; // small tolerance
            Vector3 delta = desired - _serverPos;
            if (delta.magnitude > maxStep) delta = delta.normalized * maxStep;
            _serverPos += delta;

            // Broadcast authoritative position + a periodic reconciliation packet.
            Defer(() =>
            {
                OnEntityMoved?.Invoke(localEntityId, _serverPos.x, _serverPos.z);
                OnStateCorrection?.Invoke(new StateCorrection
                {
                    LastProcessedInput = _lastProcessedInput,
                    X = _serverPos.x,
                    Z = _serverPos.z,
                    Resource = _serverAbilities.CurrentResource
                });
            });
        }

        private void ServerProcessCast(CastIntent c)
        {
            var slot = (AbilitySlot)c.AbilitySlot;
            AbilityDefinition ability = ResolveAbility(slot);

            CastAck ack = new CastAck { SequenceNumber = c.SequenceNumber, AbilitySlot = c.AbilitySlot };

            if (ability == null)
            {
                ack.Accepted = false;
                ack.RejectReason = "InvalidAbility";
            }
            else
            {
                // Range check against the authoritative pawn position.
                Vector3 origin = new Vector3(c.OriginX, 0f, c.OriginZ);
                bool inRange = Vector3.Distance(origin, _serverPos) <= 2.0f; // origin must match server pos
                CastResult result = _serverAbilities.TryCast(ability, _serverStats);

                if (result == CastResult.Success && inRange)
                {
                    ack.Accepted = true;
                    ack.RejectReason = string.Empty;
                    ack.ServerCooldownSeconds = _serverStats.CooldownAfterHaste(ability.baseCooldown);
                    ack.ServerResourceAfter = _serverAbilities.CurrentResource;
                    ack.PredictedDamage = _serverAbilities.ResolveDamage(ability, _serverStats);
                }
                else
                {
                    ack.Accepted = false;
                    ack.RejectReason = inRange ? result.ToString() : "OutOfSync";
                    ack.ServerResourceAfter = _serverAbilities.CurrentResource;
                }
            }

            Defer(() =>
            {
                OnCastResolved?.Invoke(ack);
                OnStateCorrection?.Invoke(new StateCorrection
                {
                    LastProcessedInput = _lastProcessedInput,
                    X = _serverPos.x,
                    Z = _serverPos.z,
                    Resource = _serverAbilities.CurrentResource
                });
            });
        }

        private AbilityDefinition ResolveAbility(AbilitySlot slot) => champion == null ? null : slot switch
        {
            AbilitySlot.Q => champion.abilityQ,
            AbilitySlot.W => champion.abilityW,
            AbilitySlot.E => champion.abilityE,
            AbilitySlot.Ultimate => champion.ultimate,
            _ => null
        };
    }
}
