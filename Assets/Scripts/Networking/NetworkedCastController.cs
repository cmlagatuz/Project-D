// Project D  |  Codename: Zantetsuken
// NetworkedCastController.cs  (Sprint 5, client side)
//
// Wraps a LOCAL AbilitySystem for instant predictive feel, sends CastIntents to
// the authoritative server, and reconciles when the server acks/rejects:
//   - On predict: locally start cooldown + spend resource so the button responds
//     with zero input latency.
//   - On ACCEPT: snap local cooldown/resource to the authoritative values.
//   - On REJECT: roll back the predicted cooldown/resource (undo the prediction)
//     and surface the reason for UI feedback.
//
// This isolates all prediction/reconciliation from PrototypePawn so the same
// controller works for bots, tests, and the real transport.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Data;
using ProjectD.Gameplay.Combat;
using ProjectD.Gameplay.Champions;
using ProjectD.Networking;

namespace ProjectD.Networking
{
    public sealed class NetworkedCastController
    {
        private readonly IAuthoritativeServer _server;
        private readonly ChampionDefinition _champion;
        private readonly AbilitySystem _predicted = new();   // client-predicted state
        private readonly StatBlock _stats;

        private int _nextSequence = 1;

        // Remember predicted spends so a rejection can roll them back exactly.
        private readonly Dictionary<int, PredictedCast> _inFlight = new();

        public struct PredictedCast
        {
            public AbilitySlot Slot;
            public float ResourceBefore;
        }

        public struct CastFeedback
        {
            public AbilitySlot Slot;
            public bool Accepted;
            public string Reason;
            public float PredictedDamage;
        }

        /// <summary>Fired after the server resolves a cast (accept or reject).</summary>
        public event System.Action<CastFeedback> OnCastFeedback;

        public AbilitySystem Predicted => _predicted;

        public NetworkedCastController(IAuthoritativeServer server, ChampionDefinition champion, StatBlock stats)
        {
            _server = server;
            _champion = champion;
            _stats = stats;

            _predicted.CurrentResource = champion != null ? champion.baseResource : 300f;
            _predicted.LearnOrRankUp(AbilitySlot.Q);
            _predicted.LearnOrRankUp(AbilitySlot.W);
            _predicted.LearnOrRankUp(AbilitySlot.E);
            _predicted.LearnOrRankUp(AbilitySlot.Ultimate);

            _server.OnCastResolved += HandleCastResolved;
            _server.OnStateCorrection += HandleStateCorrection;
        }

        public void Dispose()
        {
            _server.OnCastResolved -= HandleCastResolved;
            _server.OnStateCorrection -= HandleStateCorrection;
        }

        /// <summary>Advance the predicted clock (call each frame from the pawn).</summary>
        public void Tick(float deltaTime)
        {
            _predicted.ServerTime += deltaTime;
            if (_champion != null)
                _predicted.CurrentResource = Mathf.Min(
                    _champion.baseResource, _predicted.CurrentResource + 8f * deltaTime);
        }

        public float CooldownFill(AbilitySlot slot, float baseCooldown)
            => _predicted.CooldownRemaining01(slot, baseCooldown, _stats);

        /// <summary>
        /// Client requests a cast. Returns true if the local prediction passed
        /// (button should animate); the authoritative result arrives via feedback.
        /// </summary>
        public bool RequestCast(AbilitySlot slot, Vector3 origin, Vector3 aimDir)
        {
            AbilityDefinition ability = ResolveAbility(slot);
            if (ability == null) return false;

            float resourceBefore = _predicted.CurrentResource;

            // 1) Predict locally for instant feel.
            CastResult local = _predicted.TryCast(ability, _stats);
            if (local != CastResult.Success)
            {
                OnCastFeedback?.Invoke(new CastFeedback { Slot = slot, Accepted = false, Reason = local.ToString() });
                return false;
            }

            // 2) Send the intent to the authority.
            int seq = _nextSequence++;
            _inFlight[seq] = new PredictedCast { Slot = slot, ResourceBefore = resourceBefore };

            _server.SendCast(new CastIntent
            {
                SequenceNumber = seq,
                AbilitySlot = (int)slot,
                OriginX = origin.x,
                OriginZ = origin.z,
                AimX = aimDir.x,
                AimZ = aimDir.z,
                TargetEntityId = -1,
                ClientSendTime = Time.time
            });

            return true;
        }

        // ---- Reconciliation ---------------------------------------------- //
        private void HandleCastResolved(CastAck ack)
        {
            bool tracked = _inFlight.TryGetValue(ack.SequenceNumber, out var predicted);
            _inFlight.Remove(ack.SequenceNumber);

            if (ack.Accepted)
            {
                // Snap predicted resource to the authoritative post-cast value.
                _predicted.CurrentResource = ack.ServerResourceAfter;
                OnCastFeedback?.Invoke(new CastFeedback
                {
                    Slot = (AbilitySlot)ack.AbilitySlot,
                    Accepted = true,
                    Reason = string.Empty,
                    PredictedDamage = ack.PredictedDamage
                });
            }
            else
            {
                // Roll back the mispredicted spend (server refused the cast).
                if (tracked) _predicted.CurrentResource = predicted.ResourceBefore;
                OnCastFeedback?.Invoke(new CastFeedback
                {
                    Slot = (AbilitySlot)ack.AbilitySlot,
                    Accepted = false,
                    Reason = string.IsNullOrEmpty(ack.RejectReason) ? "Rejected" : ack.RejectReason
                });
            }
        }

        private void HandleStateCorrection(StateCorrection c)
        {
            // Authoritative resource wins on every correction packet.
            _predicted.CurrentResource = c.Resource;
        }

        private AbilityDefinition ResolveAbility(AbilitySlot slot) => _champion == null ? null : slot switch
        {
            AbilitySlot.Q => _champion.abilityQ,
            AbilitySlot.W => _champion.abilityW,
            AbilitySlot.E => _champion.abilityE,
            AbilitySlot.Ultimate => _champion.ultimate,
            _ => null
        };
    }
}
