// Project D  |  Codename: Zantetsuken
// IAuthoritativeServer.cs  (GCD 14.2 architecture, 14.3 security)
//
// SPRINT 5. Boundary between client and the authoritative simulation. The client
// SENDS intents and RECEIVES validated results. No gameplay decision is trusted
// client-side. A concrete transport (Unity Netcode / Mirror / custom UDP) or the
// bundled LocalLoopbackServer implements this interface.
//
// Cast flow (predicted + reconciled):
//   1) Client predicts locally and calls SendCast(intent) with a sequence number.
//   2) Server validates (rank, cooldown, resource, range, target) and replies
//      with OnCastAcknowledged (authoritative cooldown/resource) or OnCastRejected
//      (reason + authoritative state to roll back to).
//   3) Client reconciles its predicted state against the ack/reject.

using ProjectD.Core;

namespace ProjectD.Networking
{
    public struct MoveIntent
    {
        public int SequenceNumber;
        public float TargetX, TargetZ;   // desired planar direction/position
        public float ClientSendTime;
    }

    public struct CastIntent
    {
        public int SequenceNumber;
        public int AbilitySlot;          // maps to Data.AbilitySlot
        public float OriginX, OriginZ;   // predicted cast origin (pawn position)
        public float AimX, AimZ;         // normalized aim direction
        public int TargetEntityId;       // -1 for skillshots
        public float ClientSendTime;
    }

    /// <summary>Authoritative outcome of a cast request.</summary>
    public struct CastAck
    {
        public int SequenceNumber;
        public int AbilitySlot;
        public bool Accepted;
        public string RejectReason;      // empty when accepted
        public float ServerCooldownSeconds;
        public float ServerResourceAfter;
        public float PredictedDamage;    // for local hit-feedback only
    }

    /// <summary>Correction packet used by client reconciliation.</summary>
    public struct StateCorrection
    {
        public int LastProcessedInput;   // highest input sequence the server applied
        public float X, Z;               // authoritative position
        public float Resource;           // authoritative resource
    }

    public interface IAuthoritativeServer
    {
        // ---- Client -> Server (requests only) ----
        void SendMove(in MoveIntent intent);
        void SendCast(in CastIntent intent);
        void SendPing(int worldX, int worldZ, byte pingType);

        // ---- Server -> Client (validated state) ----
        event System.Action<int, float, float> OnEntityMoved;    // id, x, z
        event System.Action<int, float, float> OnHealthChanged;  // id, current, max
        event System.Action<CastAck> OnCastResolved;             // ack OR reject
        event System.Action<StateCorrection> OnStateCorrection;  // reconciliation
        event System.Action<string> OnObjectiveRevealed;         // upcoming id
        event System.Action<int> OnMatchEnded;                   // losing team

        // Local clock the server considers authoritative (seconds).
        float ServerTime { get; }
    }
}
